using UnityEditor;
using UnityEngine;

namespace Cubiquity
{
    public class VolumeDataInspector : Editor
    {
        protected void OnInspectorGUIImpl()
        {
            VolumeData data = target as VolumeData;

            EditorGUILayout.LabelField("Full path to voxel database:", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(data.fullPathToVoxelDatabase, MessageType.None);
            EditorGUILayout.Space();

            bool vdbAlreadyOpen = !data.IsVolumeHandleNull();
            EditorGUILayout.BeginHorizontal();
            GUI.enabled = !vdbAlreadyOpen;
            EditorGUILayout.LabelField("Open as read-only:", EditorStyles.boldLabel, GUILayout.Width(150));
            data.writePermissions = GUILayout.Toggle(data.writePermissions == VolumeData.WritePermissions.ReadOnly, "")
                ? VolumeData.WritePermissions.ReadOnly : VolumeData.WritePermissions.ReadWrite;
            GUI.enabled = true;
            if (vdbAlreadyOpen)
            {
                EditorGUILayout.LabelField("(voxel database is open)", GUILayout.Width(150));
            }
            EditorGUILayout.EndHorizontal();

            if (data.writePermissions == VolumeData.WritePermissions.ReadOnly)
            {
                EditorGUILayout.HelpBox("Opening a voxel database in read-only mode allows multiple VolumeData instances " +
                    "to make use of it at the same time. You will still be able to modify the volume data in the editor or " +
                    "in play mode, but you will not be able to save the changes back into the voxel database.", MessageType.None);
            }
            else
            {
                EditorGUILayout.HelpBox("Opening a voxel database in read-write mode (not read-only)" +
                    "allows you to save any changes back to disk. However, in this case only a single " +
                    "VolumeData instance can make use of the voxel database.", MessageType.None);
            }

            if (vdbAlreadyOpen)
            {
                EditorGUILayout.HelpBox("You can't change the write permissions while the voxel database is open. If you want to do this you " +
                    "will need to close anything using the asset, and probably need to close the scene and restart Unity.", MessageType.Info);
            }

            EditorUtility.SetDirty(data);
        }
    }
}