using UnityEditor;
using UnityEngine;

namespace Cubiquity
{
    public class VolumeColliderInspector : Editor
    {
        protected void OnInspectorGUIImpl()
        {
            VolumeCollider volumeCollider = target as VolumeCollider;

            float labelWidth = 120.0f;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Use In Edit Mode:", GUILayout.Width(labelWidth));
            volumeCollider.useInEditMode = EditorGUILayout.Toggle(volumeCollider.useInEditMode);
            EditorGUILayout.EndHorizontal();
        }
    }
}