using Cubiquity.Impl;
using UnityEngine;

namespace Cubiquity
{
    /// Allows the creation of dynamic terrains featuring caves and overhangs.
    /**
	 * The TerrainVolume behaves as a logical extention to Unity's built in terrains, but using a true volumetric (3D) represnetation of the world
	 * instead of a 2D heightmap. This allows the construction of features such as caves and overhangs which are not normally possible in Unity. It
	 * is also far more flexible in terms of the modifications which can be performed at runtime, allowing (for example) a tunnel to be dug into a
	 * hillside. This kind of modification is not possible using Unity's built in terrains because it would result in a structure which cannot be
	 * represented by the heightmap.
	 *
	 * Aside from these enhancements, %Cubiquity's TerrainVolume provides similar functionality to the standard Unity terrain. Tools are provided to
	 * sculpt that shape of the terrain and also to paint on it with a number of materials. The TerrainVolume can also be modified from code by
	 * getting the underlying TerrainVolumeData and manipulating it directly though the provided API. This allows you to write your own tools or
	 * actions as required by your specific gameplay mechanics.
	 *
	 * \image html smooth-terrain.jpg
	 *
	 * Each voxel of the TerrainVolume represent the material which exists at that location. More accuratly, a voxel can actually represent a *combination*
	 * of materials, such that it could be 50% rock, 40% soil, and 10% sand, for example. Please see the documentation on MaterialSet for a more
	 * comprehensive coverage of this.
	 *
	 * The TerrainVolume class is used in conjunction with TerrainVolumeData, TerrainVolumeRenderer, and TerrainVolumeCollider. Each of these derives
	 * from a base class, and you should see the documentation and diagram accompanying the Volume class for an understanding of how they all fit
	 * together.
	 */

    [ExecuteInEditMode]
    public class TerrainVolume : Volume
    {
        /**
		 * \copydoc Volume::data
		 */
        public new TerrainVolumeData data
        {
            get { return (TerrainVolumeData)base.data; }
            set { base.data = value; }
        }

        /// Convinience method for creating a GameObject with a set of terrain components attached.
        /**
		 * Adding a volume to a scene requires creating a GameObject and then attching the required Cubiquity components such a renderer and a
		 * collider. This method simply automates the process and also attaches the provided volume data.
		 *
		 * \param data The volume data which should be attached to the construced volume.
		 * \param addRenderer Specifies whether a renderer component should be added so that the volume is displayed.
		 * \param addCollider Specifies whether a collider component should be added so that the volume can participate in collisions.
		 */
        public static GameObject CreateGameObject(TerrainVolumeData data, bool addRenderer, bool addCollider)
        {
            // Create our main game object representing the volume.
            GameObject terrainVolumeGameObject = new GameObject("Terrain Volume");

            //Add the required volume component.
            TerrainVolume terrainVolume = terrainVolumeGameObject.GetOrAddComponent<TerrainVolume>();

            // Set the provided data.
            terrainVolume.data = data;

            // Add the renderer and collider if desired.
            if (addRenderer) { terrainVolumeGameObject.AddComponent<TerrainVolumeRenderer>(); }
            if (addCollider) { terrainVolumeGameObject.AddComponent<TerrainVolumeCollider>(); }

            // Return the created object
            return terrainVolumeGameObject;
        }

        // It seems that we need to implement this function in order to make the volume pickable in the editor.
        // It's actually the gizmo which get's picked which is often bigger than than the volume (unless all
        // voxels are solid). So somtimes the volume will be selected by clicking on apparently empty space.
        // We shold try and fix this by using raycasting to check if a voxel is under the mouse cursor?
        void OnDrawGizmos()
        {
            // If there's no data then we don't need to (and can't?) draw the gizmos
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

                // Draw an invisible box surrounding the volume. This is what actually gets picked.
                Gizmos.color = new Color(1.0f, 0.0f, 0.0f, 0.0f);
                Gizmos.DrawCube(transform.position - halfVoxelOffset + new Vector3(offsetX, offsetY, offsetZ), new Vector3(width, height, depth));
            }
        }

        /// \cond
        protected override bool SynchronizeOctree(uint availableSyncOperations)
        {
            VolumeRenderer volumeRenderer = gameObject.GetComponent<VolumeRenderer>();
            VolumeCollider volumeCollider = gameObject.GetComponent<VolumeCollider>();

            Vector3 camPos = CameraUtils.getCurrentCameraPosition();

            // This is messy - perhaps the LOD thresold shold not be a parameter to update. Instead it could be passed
            // as a parameter during traversal, so different traversal could retrieve differnt LODs. We then wouldn't
            // want a single 'renderThisNode' member of Cubiquity nodes, but instead some threshold we could compare to.
            float lodThreshold = GetComponent<VolumeRenderer>() ? GetComponent<VolumeRenderer>().lodThreshold : 1.0f;

            int minimumLOD = GetComponent<VolumeRenderer>() ? GetComponent<VolumeRenderer>().minimumLOD : 0;

            // Although the LOD system is partially functional I don't feel it's ready for release yet.
            // The following line disables it by forcing the highest level of detail to always be used.
            minimumLOD = 0;

            // Next line commented out so the system starts up with LOD disabled.
            //if (volumeRenderer.hasChanged)
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