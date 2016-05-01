using UnityEngine;
using UnityEditor;
using System.Collections;

namespace Uniblocks {

public class EngineSettings : EditorWindow {

	private GameObject EngineInstance;

	// ==== Find Engine =====
	private bool FindEngine () { // returns false if engine not found, else true
		foreach (Object obj in Object.FindObjectsOfType<Engine>()) {
			if (obj != null) {
				Engine engine = obj as Engine;
				EngineInstance = engine.gameObject;
				return true;
			}
		}
		return false;
	}
	
	
	[MenuItem ("Window/Uniblocks: Engine Settings")]
	static void Init () {
		
		EngineSettings window = (EngineSettings)EditorWindow.GetWindow (typeof (EngineSettings));
		window.Show();
	}
	
	public void OnGUI () {
		
		if ( FindEngine() == false ) {
			EditorGUILayout.LabelField ("Cannot find an Engine game object in the scene!");
			return;
		}
		
		else if (EngineInstance.GetComponent<ChunkManager>() == null) {
			EditorGUILayout.LabelField ("The Engine game object does not have a ChunkManager component!");
			return;
		}
		
		else {
			
			Engine engine = EngineInstance.GetComponent<Engine>();
			
			EditorGUILayout.BeginVertical();
			
			GUILayout.Space (10);
			
			engine.lWorldName = EditorGUILayout.TextField ( "World name", engine.lWorldName );
			
			GUILayout.Space (20);
			
			GUILayout.Label ("Chunk settings");
			
			engine.lChunkSpawnDistance = EditorGUILayout.IntField ( "Chunk spawn distance", engine.lChunkSpawnDistance );
			engine.lChunkDespawnDistance = EditorGUILayout.IntField ( "Chunk despawn distance", engine.lChunkDespawnDistance );
			engine.lHeightRange = EditorGUILayout.IntField ( "Chunk height range", engine.lHeightRange );
			engine.lChunkSideLength = EditorGUILayout.IntField ( "Chunk side length", engine.lChunkSideLength );
			engine.lTextureUnit = EditorGUILayout.FloatField ( "Texture unit", engine.lTextureUnit );
			engine.lTexturePadding = EditorGUILayout.FloatField ( "Texture padding", engine.lTexturePadding );
			engine.lGenerateMeshes = EditorGUILayout.Toggle ( "Generate meshes", engine.lGenerateMeshes);
			engine.lGenerateColliders = EditorGUILayout.Toggle ( "Generate colliders", engine.lGenerateColliders );
			engine.lShowBorderFaces = EditorGUILayout.Toggle ( "Show border faces", engine.lShowBorderFaces );
			
			GUILayout.Space (20);
			GUILayout.Label ("Events settings");
			engine.lSendCameraLookEvents = EditorGUILayout.Toggle ( "Send camera look events", engine.lSendCameraLookEvents);
			engine.lSendCursorEvents = EditorGUILayout.Toggle ( "Send cursor events", engine.lSendCursorEvents );
			
			GUILayout.Space (20);
			GUILayout.Label ("Data settings");
			engine.lSaveVoxelData = EditorGUILayout.Toggle ( "Save/load voxel data", engine.lSaveVoxelData );
			
			GUILayout.Space (20);
			GUILayout.Label ("Multiplayer");
			engine.lEnableMultiplayer = EditorGUILayout.Toggle ( "Enable multiplayer", engine.lEnableMultiplayer );
			engine.lMultiplayerTrackPosition = EditorGUILayout.Toggle ( "Track player position", engine.lMultiplayerTrackPosition );
			engine.lChunkTimeout = EditorGUILayout.FloatField ( "Chunk timeout (0=off)", engine.lChunkTimeout );
			engine.lMaxChunkDataRequests = EditorGUILayout.IntField ("Max chunk data requests", engine.lMaxChunkDataRequests );
			GUILayout.Label ( "(0=off)" );
			
			GUILayout.Space (40);
			GUILayout.Label ("Performance");
			engine.lTargetFPS = EditorGUILayout.IntField ("Target FPS", engine.lTargetFPS);
			engine.lMaxChunkSaves = EditorGUILayout.IntField ("Chunk saves limit", engine.lMaxChunkSaves);
			
			
			if (GUI.changed) {
				UnityEditor.PrefabUtility.ReplacePrefab(engine.gameObject, UnityEditor.PrefabUtility.GetPrefabParent(engine.gameObject), ReplacePrefabOptions.ConnectToPrefab);
			}
		
			EditorGUILayout.EndVertical();
		}
	}	
}

}