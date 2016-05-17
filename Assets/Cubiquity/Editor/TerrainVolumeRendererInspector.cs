using UnityEditor;
using UnityEngine;

namespace Cubiquity
{
    [CustomEditor(typeof(TerrainVolumeRenderer))]
    public class TerrainVolumeRendererInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            TerrainVolumeRenderer renderer = target as TerrainVolumeRenderer;

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

            // Although the LOD system is partially functional I don't feel it's ready
            // for release yet. Therefore the LOD GUI components below are disabled.
            /*EditorGUILayout.LabelField("*Experimental* LOD support:", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Minimum LOD level:", GUILayout.Width(labelWidth));
            renderer.minimumLOD = EditorGUILayout.IntSlider(renderer.minimumLOD, 0, 2);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("LOD Threshold:", GUILayout.Width(labelWidth));
            renderer.lodThreshold = EditorGUILayout.Slider(renderer.lodThreshold, 0.5f, 2.0f);
            EditorGUILayout.EndHorizontal();*/

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