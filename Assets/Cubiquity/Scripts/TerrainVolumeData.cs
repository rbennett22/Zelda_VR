using Cubiquity.Impl;
using System;
using UnityEngine;

namespace Cubiquity
{
    /// An implementation of VolumeData which stores a MaterialSet for each voxel.
    /**
	 * This class provides the actual 3D grid of material weights which are used by the TerrainVolume. You can use the provided interface to directly
	 * manipulate the volume by getting or setting the weights of each voxel.
	 *
	 * Instances of this class should be created using the templatized 'Create...()' functions in the VolumeData base class. For example:
	 *
	 * \snippet ProceduralGeneration\ProceduralTerrainVolume.cs DoxygenSnippet-CreateEmptyTerrainVolumeData
	 *
	 * Note that you <em>should not</em> use ScriptableObject.CreateInstance(...) to create instances of classes derived from VolumeData.
	 */

    [System.Serializable]
    public sealed class TerrainVolumeData : VolumeData
    {
        /// Gets the material weights of the specified position.
        /**
		 * \param x The 'x' position of the voxel to get.
		 * \param y The 'y' position of the voxel to get.
		 * \param z The 'z' position of the voxel to get.
		 * \return The material weights of the voxel.
		 */
        public MaterialSet GetVoxel(int x, int y, int z)
        {
            // The initialization can fail (bad filename, database locked, etc), so the volume handle could still be null.
            if (volumeHandle.HasValue)
            {
                return CubiquityDLL.GetMaterialSetVoxel(volumeHandle.Value, x, y, z);
            }
            else
            {
                return new MaterialSet();
            }
        }

        /// Sets the material weights of the specified position.
        /**
		 * \param x The 'x' position of the voxel to set.
		 * \param y The 'y' position of the voxel to set.
		 * \param z The 'z' position of the voxel to set.
		 * \param materialSet The material weights the voxel should be set to.
		 */
        public void SetVoxel(int x, int y, int z, MaterialSet materialSet)
        {
            // The initialization can fail (bad filename, database locked, etc), so the volume handle could still be null.
            if (volumeHandle.HasValue)
            {
                if (x >= enclosingRegion.lowerCorner.x && y >= enclosingRegion.lowerCorner.y && z >= enclosingRegion.lowerCorner.z
                    && x <= enclosingRegion.upperCorner.x && y <= enclosingRegion.upperCorner.y && z <= enclosingRegion.upperCorner.z)
                {
                    CubiquityDLL.SetVoxel(volumeHandle.Value, x, y, z, materialSet);
                }
            }
        }

        public override void CommitChanges()
        {
            if (!IsVolumeHandleNull())
            {
                if (writePermissions == WritePermissions.ReadOnly)
                {
                    throw new InvalidOperationException("Cannot commit changes to read-only voxel database (" + fullPathToVoxelDatabase + ")");
                }

                CubiquityDLL.AcceptOverrideChunks(volumeHandle.Value);
                //We can discard the chunks now that they have been accepted.
                CubiquityDLL.DiscardOverrideChunks(volumeHandle.Value);
            }
        }

        public override void DiscardChanges()
        {
            if (!IsVolumeHandleNull())
            {
                CubiquityDLL.DiscardOverrideChunks(volumeHandle.Value);
            }
        }

        /// \cond
        protected override void InitializeEmptyCubiquityVolume(Region region)
        {
            // We check 'mVolumeHandle' instead of 'volumeHandle' as the getter for the latter will in turn call this method.
            if (mVolumeHandle != null) { Debug.LogAssertion("Volume handle should be null prior to initializing volume"); return; }

            if (!initializeAlreadyFailed) // If it failed before it will fail again - avoid spamming error messages.
            {
                try
                {
                    // Create an empty region of the desired size.
                    volumeHandle = CubiquityDLL.NewEmptyTerrainVolume(region.lowerCorner.x, region.lowerCorner.y, region.lowerCorner.z,
                        region.upperCorner.x, region.upperCorner.y, region.upperCorner.z, fullPathToVoxelDatabase, DefaultBaseNodeSize);
                }
                catch (CubiquityException exception)
                {
                    volumeHandle = null;
                    initializeAlreadyFailed = true;
                    Debug.LogException(exception);
                    Debug.LogError("Failed to open voxel database '" + fullPathToVoxelDatabase + "'");
                }
            }
        }
        /// \endcond

        /// \cond
        protected override void InitializeExistingCubiquityVolume()
        {
            // We check 'mVolumeHandle' instead of 'volumeHandle' as the getter for the latter will in turn call this method.
            if (mVolumeHandle != null) { Debug.LogAssertion("Volume handle should be null prior to initializing volume"); return; }

            if (!initializeAlreadyFailed) // If it failed before it will fail again - avoid spamming error messages.
            {
                try
                {
                    // Create an empty region of the desired size.
                    volumeHandle = CubiquityDLL.NewTerrainVolumeFromVDB(fullPathToVoxelDatabase, writePermissions, DefaultBaseNodeSize);
                }
                catch (CubiquityException exception)
                {
                    volumeHandle = null;
                    initializeAlreadyFailed = true;
                    Debug.LogException(exception);
                    Debug.LogError("Failed to open voxel database '" + fullPathToVoxelDatabase + "'");
                }
            }
        }
        /// \endcond

        /// \cond
        public override void ShutdownCubiquityVolume()
        {
            // Shutdown could get called multiple times. E.g by OnDisable() and then by OnDestroy().
            if (!IsVolumeHandleNull())
            {
                // We only save if we are in editor mode, not if we are playing.
                bool saveChanges = (!Application.isPlaying) && (writePermissions == WritePermissions.ReadWrite);

                if (saveChanges)
                {
                    CommitChanges();
                }
                else
                {
                    DiscardChanges();
                }

                CubiquityDLL.DeleteVolume(volumeHandle.Value);
                volumeHandle = null;
            }
        }
        /// \endcond
    }
}