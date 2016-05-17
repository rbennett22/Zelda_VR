using UnityEditor;
using UnityEngine;

namespace Cubiquity
{
    [CustomEditor(typeof(ColoredCubesVolumeRenderer))]
    public class ColoredCubesVolumeRendererInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            ColoredCubesVolumeRenderer renderer = target as ColoredCubesVolumeRenderer;

            float labelWidth = 120.0f;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Receive Shadows:", GUILayout.Width(labelWidth));
            renderer.receiveShadows = EditorGUILayout.Toggle(renderer.receiveShadows);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Cast Shadows:", GUILayout.Width(labelWidth));
            renderer.castShadows = EditorGUILayout.Toggle(renderer.castShadows);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Show Wireframe:", GUILayout.Width(labelWidth));
            renderer.showWireframe = EditorGUILayout.Toggle(renderer.showWireframe);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            renderer.material = EditorGUILayout.ObjectField("Material: ", renderer.material, typeof(Material), true) as Material;
            EditorGUILayout.EndHorizontal();

            // If any of the above caused a change then we need to update
            // the volume, so that the new properties can be synced with it.
            if (renderer.hasChanged)
            {
                Volume volume = renderer.gameObject.GetComponent<Volume>();
                if (volume != null)
                {
                    volume.ForceUpdate();
                }
            }
        }
    }
}