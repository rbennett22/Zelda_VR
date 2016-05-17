using UnityEngine;

#if UNITY_EDITOR

using UnityEditor;

#endif

using System;
using System.IO;
using System.Collections.Generic;

using Cubiquity.Impl;

namespace Cubiquity
{
    /// Base class representing the actual 3D grid of voxel values
    /**
	 * This class primarily serves as a light-weight wrapper around the \ref secVoxelDatabase "voxel databases" which are used by the %Cubiquity engine,
	 * allowing them to be treated as Unity3D assets. The voxel databases themselves are typically stored in the 'Streaming Assets' or 'Temporary Cache'
	 * folder (depending on the usage scenario), with the actual path being specified by the 'fullPathToVoxelDatabase' property. The VolumeData and it's
	 * subclasses then forward requests such as finding the dimensions of the voxel data or getting/setting the values of the individual voxels.
	 *
	 * An instance of VolumeData can be created from an existing voxel database, or it can create an empty voxel database on demand. The class also
	 * abstracts the properties and functionality which are common to all types of volume data regardless of the type of the underlying voxel. Note that
	 * users should not interact with the VolumeData directly but should instead work with one of the derived classes.
	 *
	 * When you modify the contents of a VolumeData (though GetVoxel() and SetVoxel()) the changes are immediatly visible, but they are not actually
	 * written to the underlying voxel database. Instead they are stored in a temporary location and can be commited or discared as you wish. For example,
	 * in play mode it may make sense to load an original volume, have it undergo changes in-game, and then discard these changes (see DiscardChanges())
	 * on shutdown so that it reverts to it's original state for the next game. On the other hand, if you are writing an editor then you will probably want
	 * to commit the changes which have been made (see CommitChanges()). Alternatively you could use this functionality to implement an 'undo' system.
	 *
	 * \sa TerrainVolumeData, ColoredCubesVolumeData
	 */

    [System.Serializable]
    public abstract class VolumeData : ScriptableObject
    {
        private enum VoxelDatabasePaths { Streaming, Temporary, Root };

        /// Specifies the differnt permissions with which a file can be opened.
        /*
		 * This is primarily used when opening voxel databases.
		 * \sa VolumeData
		 */

        public enum WritePermissions
        {
            /// Allow only reading.
            ReadOnly,

            /// Allow both reading an writing.
            ReadWrite
        };

        /// Gets the dimensions of the VolumeData.
        /**
		 * %Cubiquity voxel databases (and by extension the VolumeData) have a fixed size which is specified on creation. You should not attempt to access
		 * and location outside of this range.
		 *
		 * \return The dimensions of the volume. The values are inclusive, so you can safely access the voxel at the positions given by Region.lowerCorner
		 * and Region.upperCorner.
		 */
        public Region enclosingRegion
        {
            get
            {
                // The initialization can fail (bad filename, database locked, etc), so the volume handle could still be null.
                Region result = new Region(0, 0, 0, 0, 0, 0);
                if (volumeHandle != null)
                {
                    CubiquityDLL.GetEnclosingRegion(volumeHandle.Value,
                        out result.lowerCorner.x, out result.lowerCorner.y, out result.lowerCorner.z,
                        out result.upperCorner.x, out result.upperCorner.y, out result.upperCorner.z);
                }
                return result;
            }
        }

        [SerializeField]
        private VoxelDatabasePaths basePath;

        [SerializeField]
        private string relativePathToVoxelDatabase;

        /// Gets the full path to the voxel database which backs this instance.
        /**
		 * The full path to the voxel database is derived from the relative path which you can optionally specify at creation time, and the base path
		 * which depends on the way the instance was created. See CreateEmptyVolumeData<VolumeDataType>() and CreateFromVoxelDatabase<VolumeDataType>()
		 * for more information about how the base path is chosen.
		 *
		 * This property is provided mostly for informational and debugging purposes as you are unlikely to directly make use of it.
		 *
		 * \return The full path to the voxel database
		 */
        public string fullPathToVoxelDatabase
        {
            get
            {
                if (String.IsNullOrEmpty(relativePathToVoxelDatabase))
                {
                    throw new System.ArgumentException(@"The relative path to the voxel database should always exist.
						Perhaps you created the VolumeData with ScriptableObject.CreateInstance(), rather than with
						CreateEmptyVolumeData() or CreateFromVoxelDatabase()?");
                }

                string basePathString = null;
                switch (basePath)
                {
                    case VoxelDatabasePaths.Streaming:
                        basePathString = Paths.voxelDatabases + '/';
                        break;
                    case VoxelDatabasePaths.Temporary:
                        basePathString = Application.temporaryCachePath + '/';
                        break;
                    case VoxelDatabasePaths.Root:
                        basePathString = "";
                        break;
                }
                return basePathString + relativePathToVoxelDatabase;
            }
        }

        [SerializeField]
        private WritePermissions mWritePermissions = WritePermissions.ReadOnly;

        /// Controls whether the underlying voxel database can be written to, or only read from.
        /**
		 * If you specify read-only access then it is safe to have multiple VolumeData instances referenceing the same underlying
		 * voxel database, otherwise this situation should be avoided. Please see the \ref pageCubiquityVsCubiquityForUnity "user manual"
		 * for more information on this.
		 *
		 * Note that you cannot change the write permissions while the voxel database is open.
		 *
		 * \return Write permissions for the underlying voxel database.
		 */
        public WritePermissions writePermissions
        {
            get
            {
                return mWritePermissions;
            }
            set
            {
                if (writePermissions != value)
                {
                    // Don't try and change permissions if the voxel database is already open. Although we could close and reopen it,
                    // we would probably lose the non-commited data. Or perhaps it can survive in the temporary tables? Should test this.
                    if (mVolumeHandle == null)
                    {
                        // Although the voxel database is not currently open, the path could still be registered because this is
                        // based on whether the VolumeData is configured to use a certain .vdb, rather than whether it currently
                        // has it open. Therefore we unregister and re-register it to recognise the change in permissions.
                        UnregisterPath();
                        mWritePermissions = value;
                        RegisterPath();
                    }
                    else
                    {
                        Debug.LogError("Cannot change write permissions on open volume data");
                    }
                }
            }
        }

        // If set, this identifies the volume to the Cubiquity DLL. It can
        // be tested against null to find if the volume is currently valid.
        /// \cond
        protected uint? mVolumeHandle = null;

        internal uint? volumeHandle
        {
            get
            {
                // We open the database connection the first time that any attempt is made to access the data. See the
                // comments in the 'VolumeData.OnEnable()' function for more information about why we don't do it there.
                if (mVolumeHandle == null)
                {
                    InitializeExistingCubiquityVolume();
                }

                return mVolumeHandle;
            }
            set { mVolumeHandle = value; }
        }

        // Accessing the volumeHandle property will cause it to try to be initialized,
        // therfore it should not be tested against null. This method can be used instead.
        public bool IsVolumeHandleNull()
        {
            return mVolumeHandle == null;
        }
        /// \endcond

        // Don't really like having this defined here. The base node size should be a rendering property rather than a
        // property of the actual volume data. Need to make this change in the underlying Cubiquity library as well though.
        /// \cond
        protected static uint DefaultBaseNodeSize = 32;

        /// \endcond

        // Used to stop too much spamming of error messages
        /// \cond
        protected bool initializeAlreadyFailed = false;

        /// \endcond

        // We wish to enforce the idea that each voxel database (.vdb) should only be referenced by a single instance of VolumeData.
        // This avoids potential problems with multiple writes to the same database. However, Unity makes it difficult to enforce this
        // because it allows users to copy VolumeData with Object.Instantiate(), and more problematically it allows users to duplicate
        // VolumeData assets through the Unity editor. Therefore we use this dictionary to track whether a given filename is already
        // in use and warn the user if this is the case.
        private static Dictionary<string, int> pathsAndAssets = new Dictionary<string, int>();

        /**
		 * It is possible for %Cubiquity voxel database files to be created outside of the %Cubiquity for Unity3D ecosystem (see the \ref pageCubiquityVsCubiquityForUnity
		 * "user manual" if you are not clear on the difference between 'Cubiquity and 'Cubiquity for Unity3D'). For example, the %Cubiquity SDK contains
		 * importers for converting a variety of external file formats into voxel databases. This function provides a way for you to create volume data
		 * which is linked to such a user provided voxel database.
		 *
		 * \param pathToVoxelDatabase The path to the .vdb files containing the data. You should provide one of the following:
		 *   - <b>An absolute path:</b> In this case the provided path will be used directly to reference the desired .vdb file.
		 *   - <b>A relative path:</b> If you provide a relative path then is it assumed to be relative to the streaming assets folder,
		 *   because the contents of this folder are included in the build and can therefore be accessed in the editor, during play
		 *   mode, and also in standalone builds.
		 * \param writePermissions The initial write permissions for the voxel database.
		 */
        public static VolumeDataType CreateFromVoxelDatabase<VolumeDataType>(string pathToVoxelDatabase, WritePermissions writePermissions = WritePermissions.ReadOnly) where VolumeDataType : VolumeData
        {
            VolumeDataType volumeData = ScriptableObject.CreateInstance<VolumeDataType>();
            if (Path.IsPathRooted(pathToVoxelDatabase))
            {
                volumeData.basePath = VoxelDatabasePaths.Root;
            }
            else
            {
                volumeData.basePath = VoxelDatabasePaths.Streaming;
            }
            volumeData.relativePathToVoxelDatabase = pathToVoxelDatabase;
            volumeData.writePermissions = writePermissions;

            volumeData.InitializeExistingCubiquityVolume();

            volumeData.RegisterPath();

            return volumeData;
        }

        /**
		 * Creates an empty volume data instance with a new voxel database (.vdb) being created at the location specified by the optional path.
		 *
		 * \param region A Region instance specifying the dimensions of the volume data. You should not later try to access voxels outside of this range.
		 * \param pathToVoxelDatabase The path where the voxel database should be created. You should provide one of the following:
		 *   - <b>A null or empty string:</b> In this case a temporary filename will be used and the .vdb will reside in a temporary folder.
		 *   You will be unable to access the data after the volume asset is destroyed. This usage is appropriate if you want to create
		 *   a volume at run time and then fill it with data generated by your own means (e.g. procedurally).
		 *   - <b>An absolute path:</b> In this case the provided path will be used directly to reference the desired .vdb file. Keep in mind that
		 *   such a path may be specific to the system on which it was created. Therefore you are unlikely to want to use this approach in
		 *   editor code as the file will not be present when building and distrubuting your application, but you may wish to use it in
		 *   play mode from a users machine to create a volume that can be saved between play sessions.
		 *   - <b>A relative path:</b> If you provide a relative path then is it assumed to be relative to the streaming assets folder, where the
		 *   .vdb will then be created. The contents of the streaming assets folder are distributed with your application, and so this
		 *   variant is probably most appropriate if you are creating volume through code in the editor. However, be aware that you should not
		 *   use it in play mode because the streaming assets folder may be read only.
		 */
        public static VolumeDataType CreateEmptyVolumeData<VolumeDataType>(Region region, string pathToVoxelDatabase = null) where VolumeDataType : VolumeData
        {
            VolumeDataType volumeData = ScriptableObject.CreateInstance<VolumeDataType>();

            if (String.IsNullOrEmpty(pathToVoxelDatabase))
            {
                // No path was provided, so create a temporary path and the created .vdb file cannot be used after the current session.
                string pathToCreateVoxelDatabase = Cubiquity.Impl.Utility.GenerateRandomVoxelDatabaseName();
                volumeData.basePath = VoxelDatabasePaths.Temporary;
                volumeData.relativePathToVoxelDatabase = pathToCreateVoxelDatabase;
                volumeData.writePermissions = WritePermissions.ReadWrite;
                volumeData.hideFlags = HideFlags.DontSave; //Don't serialize this instance as it uses a temporary file for the voxel database.
            }
            else if (Path.IsPathRooted(pathToVoxelDatabase))
            {
                // The user provided a rooted (non-relative) path and so we use the details directly.
                volumeData.basePath = VoxelDatabasePaths.Root;
                volumeData.relativePathToVoxelDatabase = pathToVoxelDatabase;
                volumeData.writePermissions = WritePermissions.ReadWrite;
            }
            else
            {
                // The user provided a relative path, which we then assume to be relative to the streaming assets folder.
                // This should only be done in edit more (not in play mode) as stated below.
                if (Application.isPlaying)
                {
                    Debug.LogWarning("You should not provide a relative path when creating empty volume " +
                        "data in play mode, because the streaming assets folder might not have write access.");
                }

                // As the user is providing a name for the voxel database then it follows that they want to make use of it later.
                // In this case it should not be in the temp folder so we put it in streaming assets.
                volumeData.basePath = VoxelDatabasePaths.Streaming;
                volumeData.relativePathToVoxelDatabase = pathToVoxelDatabase;
                volumeData.writePermissions = WritePermissions.ReadWrite;
            }

            volumeData.InitializeEmptyCubiquityVolume(region);

            volumeData.RegisterPath();

            return volumeData;
        }

        /**
		 * Writes the current state of the voxels into the voxel database.
		 *
		 *
		 */
        public abstract void CommitChanges();

        public abstract void DiscardChanges();

        private void Awake()
        {
            // Make sure the Cubiquity library is installed.
            Installation.ValidateAndFix();

            RegisterPath();
        }

        private void OnEnable()
        {
            // We should reset this flag from time-to-time incase the user has fixed the issue. This
            // seems like an appropriate place as the user may fix the issue aand then reload the scene.
            initializeAlreadyFailed = false;

            // It would seem intuitive to open and initialise the voxel database from this function. However, there seem to be
            // some problems with this approach.
            //		1. OnEnable() is (sometimes?) called when simply clicking on the asset in the project window. In this scenario
            // 		we don't really want/need to connect to the database.
            //		2. OnEnable() does not seem to be called when a volume asset is dragged onto an existing volume, and this is
            // 		the main way of setting which data a volume should use.
        }

        private void OnDisable()
        {
            // Note: For some reason this function is not called when transitioning between edit/play mode if this scriptable
            // object has been turned into an asset. Therefore we also call Initialize...()/Shutdown...() from the Volume class.
            // Aditional: Would we rather do this in OnDestoy()? It would give better behaviour with read-only volumes as these
            // can still have temporary changes which are lost when the volume is shutdown. It may be that the user would prefer
            // such temporary changes to survive a disable/enable cycle.
            ShutdownCubiquityVolume();
        }

        private void OnDestroy()
        {
            // If the voxel database was created in the temporary location
            // then we can be sure the user has no further use for it.
            if (basePath == VoxelDatabasePaths.Temporary)
            {
                File.Delete(fullPathToVoxelDatabase);

                if (File.Exists(fullPathToVoxelDatabase))
                {
                    Debug.LogWarning("Failed to delete voxel database from temporary cache");
                }
            }

            UnregisterPath();
        }

        private void RegisterPath()
        {
            // This function may be called before the object is poroperly initialised, in which case
            // the path may be empty. There's no point in checking for duplicate entries of an empty path.
            if (!String.IsNullOrEmpty(relativePathToVoxelDatabase))
            {
                int instanceID = GetInstanceID();

                // Find out wherther the path is aready being used by an instance of VolumeData.
                int existingInstanceID;
                if (pathsAndAssets.TryGetValue(fullPathToVoxelDatabase, out existingInstanceID))
                {
                    // It is being used, but this function could be called multiple tiomes so maybe it's being used by us?
                    if (existingInstanceID != instanceID)
                    {
                        // It's being used by a different instance, so warn the user.
                        // In play mode the best we can do is give the user the instance IDs.
                        string assetName = "Instance ID = " + instanceID;
                        string existingAssetName = "Instance ID = " + existingInstanceID;

                        // But in the editor we can try and find assets for them.
#if UNITY_EDITOR
                        assetName = AssetDatabase.GetAssetPath(instanceID);
                        existingAssetName = AssetDatabase.GetAssetPath(existingInstanceID);
#endif

                        // Let the user know what has gone wrong.
                        string warningMessage = "Duplicate volume data detected! Did you attempt to duplicate or clone an existing asset? " +
                            "If you want multiple volume data instances to share a voxel database then they must all be set to read-only. " +
                            "Please see the Cubiquity for Unity3D user manual and API documentation for more information. " +
                            "\nBoth '" + existingAssetName + "' and '" + assetName + "' reference the voxel database called '" + fullPathToVoxelDatabase + "'." +
                            "\nIt is recommended that you delete/destroy '" + assetName + "'." +
                            "\nNote: If you see this message regarding an asset which you have already deleted then you may need to close the scene and/or restart Unity.";
                        Debug.LogWarning(warningMessage);
                    }
                }
                else
                {
                    // No VolumeData instance is using this path yet, so register it for ourselves. However, we only need to register if the volume data
                    // has write permissions, because multiple volume datas can safely share a single voxel database in read-only mode. Note that this
                    // logic is not bulletproof because, for example, a volume data could open a .vdb in read-only mode (hence not registering it) and
                    // another could then open it in read-write mode. But it would be caught if the volume datas were created in the other order. In
                    // general we are mostly concerned with the user duplicating in the Unity editor, for which this logic should be sufficient.
                    if (writePermissions == WritePermissions.ReadWrite)
                    {
                        pathsAndAssets.Add(fullPathToVoxelDatabase, instanceID);
                    }
                }
            }
        }

        private void UnregisterPath()
        {
            // Remove the path entry from our duplicate-checking dictionary.
            // This could fail, e.g. if the user does indeed create two instance with the same filename
            // then deleting the first will remove the entry which then won't exist when deleting the second.
            pathsAndAssets.Remove(fullPathToVoxelDatabase);
        }

        /// \cond
        protected abstract void InitializeEmptyCubiquityVolume(Region region);
        protected abstract void InitializeExistingCubiquityVolume();
        public abstract void ShutdownCubiquityVolume();
        /// \endcond
    }
}