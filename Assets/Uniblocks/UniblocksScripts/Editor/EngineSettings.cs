using UnityEditor;
using UnityEngine;

namespace Uniblocks
{
    public class EngineSettings : EditorWindow
    {
        Engine _engineInstance;
        Engine EngineInstance { get { return _engineInstance ?? (_engineInstance = FindObjectOfType<Engine>()); } }


        [MenuItem("Window/Uniblocks: Engine Settings")]
        static void Init()
        {
            EngineSettings window = (EngineSettings)GetWindow(typeof(EngineSettings));
            window.Show();
        }


        void OnGUI()
        {
            if (EngineInstance == null)
            {
                EditorGUILayout.LabelField("Cannot find a Uniblocks Engine GameObject in the scene!");
                return;
            }
            if (EngineInstance.GetComponent<ChunkManager>() == null)
            {
                EditorGUILayout.LabelField("The Uniblocks Engine GameObject does not have a ChunkManager component!");
                return;
            }

            Engine n = EngineInstance;

            EditorGUILayout.BeginVertical();
            {
                GUILayout.Space(10);

                n.lWorldName = EditorGUILayout.TextField("World name", n.lWorldName);

                GUILayout.Space(20);

                GUILayout.Label("Chunk settings");

                n.lChunkSpawnDistance = EditorGUILayout.IntField("Chunk spawn distance", n.lChunkSpawnDistance);
                n.lChunkDespawnDistance = EditorGUILayout.IntField("Chunk despawn distance", n.lChunkDespawnDistance);
                n.lHeightRange = EditorGUILayout.IntField("Chunk height range", n.lHeightRange);
                n.lChunkSizeX = EditorGUILayout.IntField("Chunk size X", n.lChunkSizeX);
                n.lChunkSizeY = EditorGUILayout.IntField("Chunk size Y", n.lChunkSizeY);
                n.lChunkSizeZ = EditorGUILayout.IntField("Chunk size Z", n.lChunkSizeZ);
                n.lTextureUnit = EditorGUILayout.FloatField("Texture unit", n.lTextureUnit);
                n.lTexturePadding = EditorGUILayout.FloatField("Texture padding", n.lTexturePadding);
                n.lGenerateMeshes = EditorGUILayout.Toggle("Generate meshes", n.lGenerateMeshes);
                n.lGenerateColliders = EditorGUILayout.Toggle("Generate colliders", n.lGenerateColliders);
                n.lShowBorderFaces = EditorGUILayout.Toggle("Show border faces", n.lShowBorderFaces);

                GUILayout.Space(20);
                GUILayout.Label("Events settings");
                n.lSendCameraLookEvents = EditorGUILayout.Toggle("Send camera look events", n.lSendCameraLookEvents);
                n.lSendCursorEvents = EditorGUILayout.Toggle("Send cursor events", n.lSendCursorEvents);

                GUILayout.Space(20);
                GUILayout.Label("Data settings");
                n.lSaveVoxelData = EditorGUILayout.Toggle("Save/load voxel data", n.lSaveVoxelData);

                GUILayout.Space(20);
                GUILayout.Label("Multiplayer");
                n.lEnableMultiplayer = EditorGUILayout.Toggle("Enable multiplayer", n.lEnableMultiplayer);
                n.lMultiplayerTrackPosition = EditorGUILayout.Toggle("Track player position", n.lMultiplayerTrackPosition);
                n.lChunkTimeout = EditorGUILayout.FloatField("Chunk timeout (0=off)", n.lChunkTimeout);
                n.lMaxChunkDataRequests = EditorGUILayout.IntField("Max chunk data requests", n.lMaxChunkDataRequests);
                GUILayout.Label("(0=off)");

                GUILayout.Space(40);
                GUILayout.Label("Performance");
                n.lTargetFPS = EditorGUILayout.IntField("Target FPS", n.lTargetFPS);
                n.lMaxChunkSaves = EditorGUILayout.IntField("Chunk saves limit", n.lMaxChunkSaves);


                if (GUI.changed)
                {
                    PrefabUtility.ReplacePrefab(n.gameObject, PrefabUtility.GetPrefabParent(n.gameObject), ReplacePrefabOptions.ConnectToPrefab);
                }
            }
            EditorGUILayout.EndVertical();
        }
    }
}