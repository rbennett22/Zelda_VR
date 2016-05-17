using UnityEngine;
using System.Collections.Generic;

using Cubiquity.Impl;

#if UNITY_EDITOR

using UnityEditor;

#endif

namespace Cubiquity
{
    /// Base class representing behaviour common to all volumes.
    /**
	 * Volumes are probably the single most fundamental concept of %Cubiquity. They essentially provide a 3D array of voxels which the user can
	 * modify at will, and they take care of synchronizing and maintaining a mesh representation of the voxels for rendering and collision detection.
	 *
	 * The Volume class itself is actually an abstract base class which cannot be instantiated by user code directly. Instead is serves as a base
	 * for the ColoredCubesVolume and TerrainVolume and encapsulates some of the common behaviour which such derived classes need. It is used in
	 * conjunction with the VolumeData, VolumeRenderer and VolumeCollider to form a structure as given below:
	 *
	 * \image html VolumeComposition.png
	 *
	 * Note how this is conceptually similar to the way that Unity's mesh classes are structured, where the MeshFilter works in conjunction with
	 * the Mesh, MeshRenderer and MeshCollider classes.
	 */

    [ExecuteInEditMode]
    public abstract class Volume : MonoBehaviour
    {
        [SerializeField]
        private VolumeData mData = null;

        /// Represents the actual 3D grid of voxels describing your object or environment.
        /**
		 * You can acces the VolumeData through this property in order to get/set the voxels on an individual basis, or to assign an existing
		 * instance of VolumeData which you have created elsewhere. Modifying or assigning the volume data will cause the mesh representation
		 * to be automatically updated.
		 */
        public VolumeData data
        {
            get { return this.mData; }
            set
            {
                if (this.mData != value)
                {
                    UnregisterVolumeData();
                    this.mData = value;
                    RegisterVolumeData();

                    // Delete the octree, so that next time Update() is called a new octree is constructed to match the new volume data.
                    RequestFlushInternalData();
                }
            }
        }

        /// Indicates whether the mesh representation is currently up to date with the volume data.
        /**
		 * Note that this property may fluctuate rapidly during real-time editing as the system tries to keep up with the users
		 * modifications, and also that it may lag a few frames behind the true syncronization state.
		 *
		 * \sa OnMeshSyncComplete, OnMeshSyncLost
		 */
        public bool isMeshSyncronized
        {
            get { return mIsMeshSyncronized; }
            protected set
            {
                // Check if the state of the mesh sync variable has actually changed.
                if (mIsMeshSyncronized != value)
                {
                    // If so update it.
                    mIsMeshSyncronized = value;

                    // And fire the appropriate event. The isMeshSyncronized flag works in edit mode,
                    // but we only fire the events in play mode (unless we find an edit-mode use too?)
                    if (Application.isPlaying)
                    {
                        if (mIsMeshSyncronized)
                        {
                            if (OnMeshSyncComplete != null) { OnMeshSyncComplete(); }
                        }
                        else
                        {
                            if (OnMeshSyncLost != null) { OnMeshSyncLost(); }
                        }
                    }
                }
            }
        }
        private bool mIsMeshSyncronized = false;

        /// Delegate type used by OnMeshSyncComplete() and OnMeshSyncLost()
        public delegate void MeshSyncAction();
        /// This event is fired once the mesh representation is up-to-date with the volume data.
        /**
		 * The process of keeping the mesh data syncronized to the volume data is computationally expensive, and it is quite possible for the
		 * mesh to lag behind. This is particularly common when fresh volume data is first assigned as it can take a few seconds for the initial
		 * mesh to be generated. If you wish to wait for the mesh to be generated before (e.g.) spawning your player object then you can use
		 * this event for this purpose.
		 *
		 * The mesh can also lag beind during intensive editing operations, and this can cause a series of OnMeshSyncComplete events to occur
		 * as the system repeatedly catches up. Therefore, in the previous player-spawning example you would probably want to disconnect the
		 * event after the first one has occured.
		 *
		 * Please see MeshSyncHandler.cs in the provided examples for a demonstration of usage.
		 *
		 * \sa isMeshSyncronized, OnMeshSyncLost
		 */
        public event MeshSyncAction OnMeshSyncComplete;

        /// This event is fired if the mesh representation is no longer up-to-date with the volume data.
        /**
		 * Syncronization between the mesh and the volume data will be lost when you first assign new volume data, and also during editing
		 * operations.
		 *
		 * Please see MeshSyncHandler.cs in the provided examples for a demonstration of usage.
		 *
		 * \sa isMeshSyncronized, OnMeshSyncComplete
		 */
        public event MeshSyncAction OnMeshSyncLost;

        /// Sets an upper limit on the rate at which the mesh representation is updated to match the volume data.
        /**
		 * %Cubiquity continuously checks whether the the mesh representation (used for rendering and physics) is synchronized with the underlying
		 * volume data. Such synchronization can be lost whenever the volume data is modified, and %Cubiquity will then regenerate the mesh. This
		 * regeneration process can take some time, and so typically you want to spread the regeneration over a number of frames.
		 *
		 * Internally %Cubiquity breaks down the volume into a number regions each corresponding to an octree node, and these can be resynchronized
		 * individually. Therefore this property controls how many of the octree nodes will be resynchronized each frame. A small value will result
		 * in a better frame rate when modifications are being performed, but at the possible expense of the rendered mesh noticeably lagging behind
		 * the modifications which are being performed.
		 *
		 * NOTE: This property is currently hidden from the user until we have a better understanding of how it should behave. For example, should
		 * that same value be used in edit mode vs. play mode? What if there is/isn't a collision mesh? Or what if we want to syncronize every 'x'
		 * updates rather than 'x' times per update?
		 */
        /// \cond
        protected uint maxSyncOperationsInPlayMode = 4;

        protected uint maxSyncOperationsInEditMode = 16; // Can be higher than in play mode as we have no collision mehses
                                                         /// \endcond

        // The root node of our octree. It is protected so that derived classes can use it, but users
        // are not supposed to create derived classes themselves so we hide this property from the docs.
        /// \cond
        [System.NonSerialized]
        protected GameObject rootOctreeNodeGameObject;

        /// \endcond

        // Used to check when the game object changes layer, so we can move the children to match.
        private int previousLayer = -1;

        // Used to catch the user using the same volume data for multiple volumes (which they should not do).
        // It's not a really robust approach but it works well enough and only serves to issue a warning anyway.
        private static Dictionary<int, int> volumeDataAndVolumes = new Dictionary<int, int>();

        private bool flushRequested;

        // ------------------------------------------------------------------------------
        // These editor-only functions are used to emulate repeated calls to Update() in edit mode. Setting the '[ExecuteInEditMode]' attribute does cause
        // Update() to be called automatically in edit mode, but it only happens in response to user-driven events such as moving the mouse in the editor
        // window. We want to support background loading of our terrain and so we hook into the 'EditorApplication.update' event for this purpose.
        // ------------------------------------------------------------------------------
#if UNITY_EDITOR

        private int editModeUpdates = 0;

        // Public so that we can manually drive it from the editor as required,
        // but user code should not do this so it's hidden from the docs.
        /// \cond
        public void ForceUpdate()
        {
            Update();
        }

        /// \cond
        public void EditModeUpdateHandler() // Public so we can call it from Editor scripts
        {
            DebugUtils.Assert(Application.isPlaying == false, "EditModeUpdateHandler() should never be called in play mode!");

            if (enabled)
            {
                if (isMeshSyncronized)
                {
                    if (editModeUpdates % 20 == 0)
                    {
                        //Debug.Log("Low freq update");
                        ForceUpdate();
                        //SceneView.RepaintAll();
                    }
                }
                else
                {
                    //Debug.Log("High freq update");
                    ForceUpdate();
                    //SceneView.RepaintAll();
                }
            }

            editModeUpdates++;
        }
        /// \endcond
#endif
        // ------------------------------------------------------------------------------

        void Awake()
        {
            RegisterVolumeData();
        }

        void OnEnable()
        {
            // We have taken steps to make sure that our octree does not get saved to disk or persisted between edit/play mode,
            // but it will still survive if we just disable and then enable the volume. This is because the OnDisable() and
            // OnEnable() methods do now allow us to modify our game object hierarchy. Note that this disable/enable process
            // may also happen automatically such as during a script reload? Requesting a flush of the octree is the safest option.
            RequestFlushInternalData();
        }

        void OnDisable()
        {
            // Ideally the VolumeData would handle it's own initialization and shutdown, but it's OnEnable()/OnDisable() methods don't seems to be
            // called when switching between edit/play mode if it has been turned into an asset. Therefore we do it here as well just to be sure.
            if (data != null)
            {
                data.ShutdownCubiquityVolume();
            }
        }

        void OnDestroy()
        {
            UnregisterVolumeData();
        }

        // Public as the editor sometimes needs to flush the internal data,
        // but user code should not do this so it's hidden from the docs.
        /// \cond
        public void RequestFlushInternalData()
        {
            flushRequested = true;
        }
        /// \endcond

        private void FlushInternalData()
        {
            // It should be enough to delete the root octree node in this function but we're seeing cases
            // of octree nodes surviving the transition between edit and play modes. I'm not quite sure
            // why, but the approach below of deleting all child objects seems to solve the problem.

            // Find all the child objects
            List<GameObject> childObjects = new List<GameObject>();
            foreach (Transform childTransform in gameObject.transform)
            {
                childObjects.Add(childTransform.gameObject);
            }

            // Destroy all children
            foreach (GameObject childObject in childObjects)
            {
                Impl.Utility.DestroyOrDestroyImmediate(childObject);
            }

            rootOctreeNodeGameObject = null;
        }

        protected abstract bool SynchronizeOctree(uint maxSyncOperations);

        private void Update()
        {
            if (flushRequested)
            {
                FlushInternalData();
                flushRequested = false;

                // It seems prudent to return at this point, and leave the actual updating to the next call of this function.
                // This is because we've just destroyed a bunch of stuff by flushing and Unity actually defers Destroy() until
                // later in the frame. It actually seems t work ok without the return, but it makes me feel a little safer.
                return;
            }

            // Check whether the gameObject has been moved to a new layer.
            if (gameObject.layer != previousLayer)
            {
                // If so we update the children to match and then clear the flag.
                gameObject.SetLayerRecursively(gameObject.layer);
                previousLayer = gameObject.layer;
            }

            // Set shader parameters.
            VolumeRenderer volumeRenderer = gameObject.GetComponent<VolumeRenderer>();
            if (volumeRenderer != null)
            {
                if (volumeRenderer.material != null)
                {
                    Vector3i volumeSize = (data.enclosingRegion.upperCorner - data.enclosingRegion.lowerCorner);
                    volumeSize.x++; volumeSize.y++; volumeSize.z++;
                    volumeRenderer.material.SetVector("_VolumeSize", (Vector3)volumeSize);

                    // NOTE - The following line passes transform.worldToLocalMatrix as a shader parameter. This is explicitly
                    // forbidden by the Unity docs which say:
                    //
                    //   IMPORTANT: If you're setting shader parameters you MUST use Renderer.worldToLocalMatrix instead.
                    //
                    // However, we don't have a renderer on this game object as the rendering is handled by the child OctreeNodes.
                    // The Unity doc's do not say why this is the case, but my best guess is that it is related to secret scaling
                    // which Unity may perform before sending data to the GPU (probably to avoid precision problems). See here:
                    //
                    //   http://forum.unity3d.com/threads/153328-How-to-reproduce-_Object2World
                    //
                    // It seems to work in our case, even with non-uniform scaling applied to the volume. Perhaps we are just geting
                    // lucky, pehaps it just works on our platform, or perhaps it is actually valid for some other reason. Just be aware.
                    volumeRenderer.material.SetMatrix("_World2Volume", transform.worldToLocalMatrix);
                }
            }

            if (data != null && data.volumeHandle.HasValue)
            {
                // When we are in game mode we limit the number of nodes which we update per frame, to maintain a nice.
                // framerate The Update() method is called repeatedly and so over time the whole mesh gets syncronized.
                if (Application.isPlaying)
                {
                    isMeshSyncronized = SynchronizeOctree(maxSyncOperationsInPlayMode);
                }
                else
                {
                    isMeshSyncronized = SynchronizeOctree(maxSyncOperationsInEditMode);
                }
            }
        }
        /// \endcond

        // Public so that we can manually drive it from the editor as required,
        // but user code should not do this so it's hidden from the docs.
        /// \cond
        public void OnGUI()
        {
            // This code doesn't belong in Volume? There should
            // probably be one global copy of this, not one per volume.

            /*GUILayout.BeginArea(new Rect(10, 10, 300, 300));
            GUI.skin.label.alignment = TextAnchor.MiddleLeft;
            string debugPanelMessage = "Cubiquity Debug Panel\n";
            if(isMeshSyncronized)
            {
                debugPanelMessage += "Mesh sync: Completed";
            }
            else
            {
                debugPanelMessage += "Mesh sync: In progress...";
            }
            GUILayout.Box(debugPanelMessage);
            GUILayout.EndArea();*/
        }
        /// \endcond

        private void RegisterVolumeData()
        {
            if (mData != null)
            {
                int volumeID = GetInstanceID();
                int volumeDataID = mData.GetInstanceID();

                int existingVolumeID;
                if (volumeDataAndVolumes.TryGetValue(volumeDataID, out existingVolumeID))
                {
                    if (existingVolumeID != volumeID)
                    {
                        // It's being used by a different instance, so warn the user.
                        // In play mode the best we can do is give the user the instance IDs.
                        string volumeName = "Instance ID = " + volumeID;
                        string existingVolumeName = "Instance ID = " + existingVolumeID;
                        string volumeDataName = "Instance ID = " + volumeDataID;

                        // But in the editor we can try and find names for them.
#if UNITY_EDITOR
                        Object volume = EditorUtility.InstanceIDToObject(volumeID);
                        if (volume) volumeName = volume.name;

                        Object existingVolume = EditorUtility.InstanceIDToObject(existingVolumeID);
                        if (existingVolume) existingVolumeName = existingVolume.name;

                        volumeDataName = AssetDatabase.GetAssetPath(volumeDataID);
#endif

                        // Let the user know what has gone wrong.
                        string warningMessage = "Multiple use of volume data detected! Did you attempt to duplicate or clone an existing volume? " +
                            "Each volume data should only be used by a single volume - please see the Cubiquity for Unity3D user manual and API documentation for more information. " +
                            "\nBoth '" + existingVolumeName + "' and '" + volumeName + "' reference the volume data called '" + volumeDataName + "'." +
                            "\nNote: If you see this message regarding an asset which you have already deleted then you may need to close the scene and/or restart Unity.";
                        Debug.LogWarning(warningMessage);
                    }
                }
                else
                {
                    volumeDataAndVolumes.Add(volumeDataID, volumeID);
                }
            }
        }

        private void UnregisterVolumeData()
        {
            if (mData != null)
            {
                // Remove the volume data entry from our duplicate-checking dictionary.
                // This could fail, e.g. if the user does indeed create two volumes with the same volume data
                // then deleting the first will remove the entry which then won't exist when deleting the second.
                volumeDataAndVolumes.Remove(mData.GetInstanceID());
            }
        }

#if UNITY_EDITOR

        // Because our hierarchy of Octree nodes is generated at runtime (and is depenant on the camera position) we do not
        // want to serialize it with the scene. Setting the 'DontSave' flag on the root would seem like an intiative solution
        // but actually results in errors which, while harmless, will be disconcerting to the users. See this Unity Answers thread
        // for more details: http://answers.unity3d.com/questions/609621/hideflagsdontsave-causes-checkconsistency-transfor.html
        //
        // We have found two posible solutions to this problem. The first solution is to make use of a 'ghost object' which sits
        // at the root of our scene and has it's transformation updated every frame to match out current volume's transformation.
        // The octree can then be attached to this ghost object rather than the real volume. This get's rid of the harmless error
        // message because the ghost is at the root of the scene, and so there is no parent object to complain when it is not
        // found on scene load. However, this solution has some known problems:
        //
        //     - The bounding box of the real volume is no longer correct, as it has no tree/mesh data of it's own. You can't
        //       manually set the bounding box, so we'd probably have to create a fake degenerate mesh and attach it to the
        //       volume. The coresponding MeshFilter/MeshRenderer then show up in the volume inspector, I believe even with the
        //       'HideInHierarchy' flag set (they are then greyed out).
        //
        //     - The 'Show Wireframe' feature doesn't work, as you can only see the wireframe of a selected object and you can't
        //       select the hidden ghost object.
        //
        //     - I also have concerns about how well selection/gizmos will work with a ghost object. For example, clicking the
        //       ghost in the scene view should select the ral object. But this isn't really tested yet.
        //
        // The second solution is to attach the octree to the real volume game object, and then attempt to delete it before
        // serialization is performed. Unity won't allow us to modify the object hierarchy in the OnDisable() method, so we have
        // to resort to using the slightly 'hacky' functions below. The only know drawback of this approach is that it means
        // the scene is flagged as 'unsaved' immediatly after saving it (probably because the octree is then rebuilt). But
        // thinking about it now, this probably affects the 'ghost object' solution as well. This second solution is what we
        // have implemented below.
        private class OnSaveHandler : UnityEditor.AssetModificationProcessor
        {
            public static string[] OnWillSaveAssets(string[] assets)
            {
                // Flush internal data of any volumes in the scene
                Object[] volumes = Object.FindObjectsOfType(typeof(Volume));
                foreach (Object volume in volumes)
                {
                    ((Volume)volume).FlushInternalData();
                }

                // Return the list of assets to be saved (we haven't changed this).
                return assets;
            }
        }

        // I'm not sure this code is really needed. Any internal data is discarded in the OnEnable() function and this ensures
        // correct functionality. We are also using 'OnWillSaveAssets()' to ensure the octree doesn't get saved when writing to
        // disk. However, we can still have a situation where the octree is serialized (internal serialization, not saving the
        // scene) when switching between edit and play mode, and although the resulting behaviour is correct (because OnEnable()
        // will later discard the old octree) it could be wasteful.
        //
        // The following code attempts to resolve this by flushing the octree when switching between modes. But it's not clear
        // if this flushing actually happens in time, or if it really helps performance. But there's no reason to think it hurts
        // so it's left here pending further investigation.
        [InitializeOnLoad]
        private class OnPlayHandler
        {
            static OnPlayHandler()
            {
                // Catch the event which occurs when switching modes.
                EditorApplication.playmodeStateChanged += OnPlaymodeStateChanged;
            }

            static void OnPlaymodeStateChanged()
            {
                // We only need to discard the octree in edit mode, beacause when leaving play mode serialization is not
                // performed. This event occurs both when leaving the old mode and again when entering the new mode, but
                // when entering edit mode the root null should already be null and discarding it again is harmless.
                {
                    Object[] volumes = Object.FindObjectsOfType(typeof(Volume));
                    foreach (Object volume in volumes)
                    {
                        ((Volume)volume).FlushInternalData();
                    }
                }
            }
        }

#endif
    }
}