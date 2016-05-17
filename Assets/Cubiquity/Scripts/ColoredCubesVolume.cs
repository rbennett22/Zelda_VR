using Cubiquity.Impl;
using UnityEngine;

namespace Cubiquity
{
    /// Allows environments to be built from millions of colored cubes
    /**
	 * The ColoredCubesVolume can be used to achieve retro-style environents which capture the visual essence of pixel art but extended into three
	 * dimensions. Large numbers of cubes can be added or removed each frame in order to create highly dynamic worlds.
	 *
	 * \image html cubic-terrain.jpg
	 *
	 * Conceptually this kind of volume is easy to understand. Access to the underlying voxel data can be obtained through the 'data' property,
	 * and this returns a instance of ColoredCubesVolumeData with each voxel of the data being a QuantizedColor. The ColoredCubesVolume and
	 * ColoredCubesVolumeData are used in conjunction with the ColoredCubesRenderer and ColoredCubesCollider. Please see the documentation of
	 * the Volume class for more details and a diagram showing how these components are related.
	 */

    [ExecuteInEditMode]
    public class ColoredCubesVolume : Volume
    {
        /**
		 * \copydoc Volume::data
		 */
        public new ColoredCubesVolumeData data
        {
            get { return (ColoredCubesVolumeData)base.data; }
            set { base.data = value; }
        }

        /// Convinience method for creating a GameObject with a set of colored cubes components attached.
        /**
		 * Adding a volume to a scene requires creating a GameObject and then attching the required Cubiquity components such a renderer and a
		 * collider. This method simply automates the process and also attaches the provided volume data.
		 *
		 * \param data The volume data which should be attached to the construced volume.
		 * \param addRenderer Specifies whether a renderer component should be added so that the volume is displayed.
		 * \param addCollider Specifies whether a collider component should be added so that the volume can participate in collisions.
		 */
        public static GameObject CreateGameObject(ColoredCubesVolumeData data, bool addRenderer, bool addCollider)
        {
            // Create our main game object representing the volume.
            GameObject coloredCubesVolumeGameObject = new GameObject("Colored Cubes Volume");

            //Add the required volume component.
            ColoredCubesVolume coloredCubesVolume = coloredCubesVolumeGameObject.GetOrAddComponent<ColoredCubesVolume>();

            // Set the provided data.
            coloredCubesVolume.data = data;

            // Add the renderer and collider if desired.
            if (addRenderer) { coloredCubesVolumeGameObject.AddComponent<ColoredCubesVolumeRenderer>(); }
            if (addCollider) { coloredCubesVolumeGameObject.AddComponent<ColoredCubesVolumeCollider>(); }

            // Return the created object
            return coloredCubesVolumeGameObject;
        }

        // It seems that we need to implement this function in order to make the volume pickable in the editor.
        // It's actually the gizmo which get's picked which is often bigger than than the volume (unless all
        // voxels are solid). So somtimes the volume will be selected by clicking on apparently empty space.
        // We shold try and fix this by using raycasting to check if a voxel is under the mouse cursor?
        void OnDrawGizmos()
        {
            if (data != null)
            {
                // Compute the size of the volume.
                int width = (data.enclosingRegion.upperCorner.x - data.enclosingRegion.lowerCorner.x) + 1;
                int height = (data.enclosingRegion.upperCorner.y - data.enclosingRegion.lowerCorner.y) + 1;
                int depth = (data.enclosingRegion.upperCorner.z - data.enclosingRegion.lowerCorner.z) + 1;
                float offsetX = width / 2;
                float offsetY = height / 2;
                float offsetZ = depth / 2;

                // The origin is at the centre of a voxel, but we want this box to start at the corner of the voxel.
                Vector3 halfVoxelOffset = new Vector3(0.5f, 0.5f, 0.5f);

                // Draw an invisible box surrounding the olume. This is what actually gets picked.
                Gizmos.color = new Color(1.0f, 0.0f, 0.0f, 0.0f);
                Gizmos.DrawCube(transform.position - halfVoxelOffset + new Vector3(offsetX, offsetY, offsetZ), new Vector3(width, height, depth));
            }
        }

        /// \cond
        protected override bool SynchronizeOctree(uint availableSyncOperations)
        {
            VolumeCollider volumeCollider = gameObject.GetComponent<VolumeCollider>();
            ColoredCubesVolumeRenderer volumeRenderer = gameObject.GetComponent<ColoredCubesVolumeRenderer>();

            Vector3 camPos = CameraUtils.getCurrentCameraPosition();

            // This is messy - perhaps the LOD thresold shold not be a parameter to update. Instead it could be passed
            // as a parameter during traversal, so different traversal could retrieve differnt LODs. We then wouldn't
            // want a single 'renderThisNode' member of Cubiquity nodes, but instead some threshold we could compare to.
            float lodThreshold = GetComponent<VolumeRenderer>() ? GetComponent<VolumeRenderer>().lodThreshold : 0.0f;

            int minimumLOD = GetComponent<VolumeRenderer>() ? GetComponent<VolumeRenderer>().minimumLOD : 0;

            // Although the LOD system is partially functional I don't feel it's ready for release yet.
            // The following line disables it by forcing the highest level of detail to always be used.
            minimumLOD = 0;

            // Next line commented out so the system starts up with LOD disabled.
            //if (volumeRenderer != null && volumeRenderer.hasChanged)
            {
                CubiquityDLL.SetLodRange(data.volumeHandle.Value, minimumLOD, 0);
            }

            bool cubiquityUpToDate = CubiquityDLL.UpdateVolume(data.volumeHandle.Value, camPos.x, camPos.y, camPos.z, lodThreshold);

            if (CubiquityDLL.HasRootOctreeNode(data.volumeHandle.Value) == 1)
            {
                uint rootNodeHandle = CubiquityDLL.GetRootOctreeNode(data.volumeHandle.Value);

                if (rootOctreeNodeGameObject == null)
                {
                    rootOctreeNodeGameObject = OctreeNode.CreateOctreeNode(rootNodeHandle, gameObject);
                }

                OctreeNode.syncNode(ref availableSyncOperations, rootOctreeNodeGameObject, rootNodeHandle, gameObject);

                if (volumeRenderer != null && volumeRenderer.hasChanged)
                {
                    OctreeNode.syncNodeWithVolumeRenderer(rootOctreeNodeGameObject, volumeRenderer, true);
                }

                if (volumeCollider != null && volumeCollider.hasChanged)
                {
                    OctreeNode.syncNodeWithVolumeCollider(rootOctreeNodeGameObject, volumeCollider, true);
                }
            }

            // These properties might have to be synced with the volume (e.g. LOD settings) or with components
            // (e.g. shadow/material settings). Therefore we don't clear the flags until all syncing is completed.
            if (volumeRenderer != null)
            {
                volumeRenderer.hasChanged = false;
            }
            if (volumeCollider != null)
            {
                volumeCollider.hasChanged = false;
            }

            // If there were still sync operations available then there was no more syncing to be done with the
            // Cubiquity octree. So if the Cubiquity octree was also up to date then we have synced everything.
            return cubiquityUpToDate && availableSyncOperations > 0;
        }
        /// \endcond
    }
}