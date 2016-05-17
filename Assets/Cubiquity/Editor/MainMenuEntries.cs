using System.IO;
using UnityEditor;
using UnityEngine;

namespace Cubiquity
{
    public class MainMenuEntries : MonoBehaviour
    {
        [MenuItem("GameObject/Create Other/Terrain Volume")]
        static void CreateTerrainVolume()
        {
            int width = 128;
            int height = 32;
            int depth = 128;

            TerrainVolumeData data = VolumeDataAsset.CreateEmptyVolumeData<TerrainVolumeData>(new Region(0, 0, 0, width - 1, height - 1, depth - 1));

            // Create some ground in the terrain so it shows up in the editor.
            // Soil as a base (mat 1) and then a couple of layers of grass (mat 0).
            TerrainVolumeGenerator.GenerateFloor(data, 6, (uint)1, 8, (uint)0);

            // Now create the terrain game object from the data.
            GameObject terrain = TerrainVolume.CreateGameObject(data, true, false);

            // And select it, so the user can get straight on with editing.
            Selection.activeGameObject = terrain;
        }

        [MenuItem("Assets/Create/Terrain Volume Data/Empty Volume Data...")]
        static void CreateEmptyTerrainVolumeDataAsset()
        {
            ScriptableWizard.DisplayWizard<CreateEmptyTerrainVolumeDataAssetWizard>("Create Terrain Volume Data", "Create");
        }

        [MenuItem("Assets/Create/Terrain Volume Data/From Voxel Database...")]
        static void CreateTerrainVolumeDataAssetFromVoxelDatabase()
        {
            // Resulting path already contains UNIX-style seperators (even on Wondows).
            string pathToVoxelDatabase = EditorUtility.OpenFilePanel("Choose a Voxel Database (.vdb) file to load", Paths.voxelDatabases, "vdb");

            if (pathToVoxelDatabase.Length != 0)
            {
                // Check the user didn't navigate outside of the required folder.
                string folderContainingSelectedVDB = Path.GetDirectoryName(pathToVoxelDatabase);
                if (PathUtils.IsSameFolderOrSubfolder(folderContainingSelectedVDB, Paths.voxelDatabases) == false)
                {
                    Debug.LogError("The chosen .vdb file must be inside the '" + Paths.voxelDatabases + "' folder");
                    return;
                }

                string relativePathToVoxelDatabase = PathUtils.MakeRelativePath(Paths.voxelDatabases + '/', pathToVoxelDatabase);

                // Pass through to the other version of the method.
                VolumeDataAsset.CreateFromVoxelDatabase<TerrainVolumeData>(relativePathToVoxelDatabase);
            }
        }

        [MenuItem("GameObject/Create Other/Colored Cubes Volume")]
        static void CreateColoredCubesVolume()
        {
            int width = 256;
            int height = 32;
            int depth = 256;

            ColoredCubesVolumeData data = VolumeDataAsset.CreateEmptyVolumeData<ColoredCubesVolumeData>(new Region(0, 0, 0, width - 1, height - 1, depth - 1));

            GameObject coloredCubesGameObject = ColoredCubesVolume.CreateGameObject(data, true, false);

            // And select it, so the user can get straight on with editing.
            Selection.activeGameObject = coloredCubesGameObject;

            int floorThickness = 8;
            QuantizedColor floorColor = new QuantizedColor(192, 192, 192, 255);

            for (int z = 0; z <= depth - 1; z++)
            {
                for (int y = 0; y < floorThickness; y++)
                {
                    for (int x = 0; x <= width - 1; x++)
                    {
                        data.SetVoxel(x, y, z, floorColor);
                    }
                }
            }
        }

        [MenuItem("Assets/Create/Colored Cubes Volume Data/Empty Volume Data...")]
        static void CreateEmptyColoredCubesVolumeDataAsset()
        {
            ScriptableWizard.DisplayWizard<CreateEmptyColoredCubesVolumeDataAssetWizard>("Create Colored Cubes Volume Data", "Create");
        }

        [MenuItem("Assets/Create/Colored Cubes Volume Data/From Voxel Database...")]
        static void CreateColoredCubesVolumeDataAssetFromVoxelDatabase()
        {
            // Resulting path already contains UNIX-style seperators (even on Wondows).
            string pathToVoxelDatabase = EditorUtility.OpenFilePanel("Choose a Voxel Database (.vdb) file to load", Paths.voxelDatabases, "vdb");

            if (pathToVoxelDatabase.Length != 0)
            {
                // Check the user didn't navigate outside of the required folder.
                string folderContainingSelectedVDB = Path.GetDirectoryName(pathToVoxelDatabase);
                if (PathUtils.IsSameFolderOrSubfolder(folderContainingSelectedVDB, Paths.voxelDatabases) == false)
                {
                    Debug.LogError("The chosen .vdb file must be inside the '" + Paths.voxelDatabases + "' folder");
                    return;
                }

                string relativePathToVoxelDatabase = PathUtils.MakeRelativePath(Paths.voxelDatabases + '/', pathToVoxelDatabase);

                // Pass through to the other version of the method.
                VolumeDataAsset.CreateFromVoxelDatabase<ColoredCubesVolumeData>(relativePathToVoxelDatabase);
            }
        }
    }
}