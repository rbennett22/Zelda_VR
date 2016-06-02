using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;

namespace ScionGUI
{
	public static class ScionGUIUtil
	{				
		private const string relativeInternalPath = "Internal/GUI/";

		public static Texture2D LoadPNG(string name)
		{
			return Resources.Load(relativeInternalPath + name) as Texture2D;

//			string path = relativeTexturePath + name + ".png";
//
//			TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
//			if (importer == null) return null;
//
//			if (importer.textureType != TextureImporterType.GUI)
//			{
//				importer.textureType = TextureImporterType.GUI;
//				AssetDatabase.WriteImportSettingsIfDirty(path);
//				AssetDatabase.Refresh();
//			}
//
//			Texture2D tex = (Texture2D)AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D));
//			return tex;
		}
	}
}
#endif