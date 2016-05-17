using UnityEngine;

namespace Cubiquity
{
    /// Defines a number of commonly used paths.
    /**
	 * Be aware that these paths may depend on underlying Unity paths such as 'Application.streamingAssetsPath', and as such they may differ between editor
	 * and standalone builds as well as between platforms.
	 */

    public class Paths
    {
        /// Locatoion of the Cubiquity SDK containing the native code libraries and additional executables (converters, etc).
        /**
		 * \return The path given by 'Application.streamingAssetsPath + "/Cubiquity/SDK"'
		 */
        public static string SDK
        {
            get { return Application.streamingAssetsPath + "/Cubiquity/SDK"; }
        }

        /// Location of the Cubiquity '.vdb' files.
        /**
		 * If you create your own voxel databases (e.g. by using a converter) then you should place them in this folder. You will then be able to use them
		 * to construct a VolumeData from code, or to create a volume data asset through the Cubiquity menus or volume inspectors (see the user manual).
		 *
		 * \return The path given by 'Application.streamingAssetsPath + "/Cubiquity/VoxelDatabases"'
		 */
        public static string voxelDatabases
        {
            get { return Application.streamingAssetsPath + "/Cubiquity/VoxelDatabases"; }
        }

        /// Location of the '.vdb' files which are created for new volume data assets.
        /**
		 * When you create a new asset (i.e. not from an existing voxel database) Cubiquity for Unity3D will create a new .vdb file to store the data. This
		 * is placed in a subdirectory to keep it seperate from any other voxel databases you might have, as this keeps things tidier. Note that .vdb's
		 * generated in this way will have a random filename, and also that they will not be automatically removed if you later delete the asset.
		 *
		 * \return The path given by 'Application.streamingAssetsPath + "/Cubiquity/VoxelDatabases/CreatedForAssets"'
		 */
        public static string voxelDatabasesCreatedForAssets
        {
            get { return Application.streamingAssetsPath + "/Cubiquity/VoxelDatabases/CreatedForAssets"; }
        }
    }
}