using System.IO;
using UnityEditor;
using UnityEngine;

namespace Cubiquity
{
    public class VolumeDataAsset
    {
        public static VolumeDataType CreateFromVoxelDatabase<VolumeDataType>(string relativePathToVoxelDatabase) where VolumeDataType : VolumeData
        {
            VolumeDataType data = VolumeData.CreateFromVoxelDatabase<VolumeDataType>(relativePathToVoxelDatabase);
            string assetName = Path.GetFileNameWithoutExtension(relativePathToVoxelDatabase);
            CreateAssetFromInstance<VolumeDataType>(data, assetName);
            return data;
        }

        public static VolumeDataType CreateEmptyVolumeData<VolumeDataType>(Region region) where VolumeDataType : VolumeData
        {
            VolumeDataType data = VolumeData.CreateEmptyVolumeData<VolumeDataType>(region, Impl.Utility.GenerateRandomVoxelDatabaseName());
            CreateAssetFromInstance<VolumeDataType>(data);
            return data;
        }

        // The contents of this method are taken/derived from here:
        // http://wiki.unity3d.com/index.php?title=CreateScriptableObjectAsset
        protected static void CreateAssetFromInstance<T>(T instance, string assetName = "") where T : ScriptableObject
        {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (path == "")
            {
                path = "Assets";
            }
            else if (Path.GetExtension(path) != "")
            {
                path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
            }

            if (assetName == "")
            {
                assetName = "New " + typeof(T).Name;
            }

            string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "/" + assetName + ".asset");

            AssetDatabase.CreateAsset(instance, assetPathAndName);

            AssetDatabase.SaveAssets();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = instance;
        }
    }
}