using Cubiquity.Impl;
using System;
using UnityEngine;

namespace Cubiquity
{
    /// An implementation of VolumeData which stores a QuantizedColor for each voxel.
    /**
	 * This class provides the actual 3D grid of color values which are used by the ColoredCubesVolume. You can use the provided interface to directly
	 * manipulate the volume by getting or setting the color of each voxel.
	 *
	 * Instances of this class should be created using the templatized 'Create...()' functions in the VolumeData base class. For example:
	 *
	 * \snippet MazeFromImage\ColoredCubeMazeFromImage.cs DoxygenSnippet-CreateEmptyColoredCubesVolumeData
	 *
	 * Note that you <em>should not</em> use ScriptableObject.CreateInstance(...) to create instances of classes derived from VolumeData.
	 */

    [System.Serializable]
    public sealed class ColoredCubesVolumeData : VolumeData
    {
        /// Gets the color of the specified position.
        /**
		 * \param x The 'x' position of the voxel to get.
		 * \param y The 'y' position of the voxel to get.
		 * \param z The 'z' position of the voxel to get.
		 * \return The color of the voxel.
		 */
        public QuantizedColor GetVoxel(int x, int y, int z)
        {
            // The initialization can fail (bad filename, database locked, etc), so the volume handle could still be null.
            if (volumeHandle.HasValue)
            {
                return CubiquityDLL.GetQuantizedColorVoxel(volumeHandle.Value, x, y, z);
            }
            else
            {
                return new QuantizedColor();
            }
        }

        /// Sets the color of the specified position.
        /**
		 * \param x The 'x' position of the voxel to set.
		 * \param y The 'y' position of the voxel to set.
		 * \param z The 'z' position of the voxel to set.
		 * \param quantizedColor The color the voxel should be set to.
		 */
        public void SetVoxel(int x, int y, int z, QuantizedColor quantizedColor)
        {
            // The initialization can fail (bad filename, database locked, etc), so the volume handle could still be null.
            if (volumeHandle.HasValue)
            {
                if (x >= enclosingRegion.lowerCorner.x && y >= enclosingRegion.lowerCorner.y && z >= enclosingRegion.lowerCorner.z
                    && x <= enclosingRegion.upperCorner.x && y <= enclosingRegion.upperCorner.y && z <= enclosingRegion.upperCorner.z)
                {
                    CubiquityDLL.SetVoxel(volumeHandle.Value, x, y, z, quantizedColor);
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
                    volumeHandle = CubiquityDLL.NewEmptyColoredCubesVolume(region.lowerCorner.x, region.lowerCorner.y, region.lowerCorner.z,
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
                    volumeHandle = CubiquityDLL.NewColoredCubesVolumeFromVDB(fullPathToVoxelDatabase, writePermissions, DefaultBaseNodeSize);
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