using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace Uniblocks {

public class BlockEditor : EditorWindow {
	
	private GameObject SelectedBlock; // in the scene
	private GameObject SelectedPrefab; // prefab of the selected block
	private GameObject[] Blocks; // prefabs of blocks. null if prefab doesn't exist.
	private ushort LastId;
	private bool BlocksGotten, ReplaceBlockDialog;
	
	private Vector2 BlockListScroll, BlockEditorScroll;
	private GameObject EngineInstance;
	
	// textures
	private Texture TextureSheet;
	
	
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
	
	// ==== Editor window ====
	
	[MenuItem ("Window/Uniblocks: Block Editor")]
	static void Init () {
		
		BlockEditor window = (BlockEditor)EditorWindow.GetWindow (typeof (BlockEditor));
		window.Show();
	}
	
	// ==== GUI ====
	
	public void OnGUI () {
		
		if ( EngineInstance == null  ) {
		
			if ( FindEngine () == false ) {
				EditorGUILayout.LabelField ("Cannot find an Engine game object in the scene!");
				return;
			}
		}		
		
			if (!BlocksGotten) {
				GetBlocks();
			}	
			
			
			GUILayout.Space (10);
			Engine engine = EngineInstance.GetComponent<Engine>();
			engine.lBlocksPath = EditorGUILayout.TextField ( "Blocks path", engine.lBlocksPath );
			if (GUI.changed) {
				UnityEditor.PrefabUtility.ReplacePrefab(engine.gameObject, UnityEditor.PrefabUtility.GetPrefabParent(engine.gameObject), ReplacePrefabOptions.ConnectToPrefab);
			}
			GUILayout.Space (5);	
			EditorGUILayout.BeginHorizontal();
			
			
			// block list
			EditorGUILayout.BeginVertical(GUILayout.Width(190));
			
			GUILayout.Space(10);
			// new block
			if (GUILayout.Button ( "New block", GUILayout.Width(145), GUILayout.Height(30) )) {
				CreateBlock();
			}
			GUILayout.Space(10);
			
			BlockListScroll = EditorGUILayout.BeginScrollView(BlockListScroll);			
			int i = 0;
			int lastbutton = 0;
			foreach (GameObject block in Blocks) {
				if (block != null) {
					
					// block button
					
					if (i-1 != lastbutton) { // block space
						GUILayout.Space(10);
					}
					lastbutton = i;
					
					EditorGUILayout.BeginHorizontal();
					GUILayout.Label (i.ToString());
					
					Voxel voxel = block.GetComponent<Voxel>();
					
					// selected button
					if (SelectedPrefab != null && block.name == SelectedPrefab.name) {
						GUILayout.Box( voxel.VName, GUILayout.Width(140));
					}
					
					// unselected button
					else if (GUILayout.Button( voxel.VName, GUILayout.Width(140) )) {
						SelectBlock (block);
					}
					EditorGUILayout.EndHorizontal();
				}				
				i++;
				
			}
			
			EditorGUILayout.EndScrollView();
			EditorGUILayout.EndVertical();
			
			
			
			
			
			
			// block editor		
			EditorGUILayout.BeginVertical();
			BlockEditorScroll = EditorGUILayout.BeginScrollView(BlockEditorScroll);
			GUILayout.Space(20);
			if (SelectedBlock == null) {
				EditorGUILayout.LabelField("Select a block...");
			}	
			
			
			
			
			
			
			if (SelectedBlock != null) {
				Voxel selectedVoxel = SelectedBlock.GetComponent<Voxel>();
				
				// name
				selectedVoxel.VName = EditorGUILayout.TextField("Name", selectedVoxel.VName);
				
				// id
				selectedVoxel.SetID ((ushort) EditorGUILayout.IntField("ID", selectedVoxel.GetID()));
				
				GUILayout.Space(10);
				
				// mesh
				selectedVoxel.VCustomMesh = EditorGUILayout.Toggle("Custom mesh", selectedVoxel.VCustomMesh);
				selectedVoxel.VMesh = (Mesh)EditorGUILayout.ObjectField ( "Mesh", selectedVoxel.VMesh, typeof(Mesh), false);
				
				
				// texture
				if (selectedVoxel.VCustomMesh == false) {
					if (selectedVoxel.VTexture.Length < 6) {
						selectedVoxel.VTexture = new Vector2[6];
					}
					
					selectedVoxel.VCustomSides = EditorGUILayout.Toggle("Define side textures", selectedVoxel.VCustomSides);
					
					if (selectedVoxel.VCustomSides) {					
						selectedVoxel.VTexture[0] = EditorGUILayout.Vector2Field ("Top ", 		selectedVoxel.VTexture[0] );
						selectedVoxel.VTexture[1] = EditorGUILayout.Vector2Field ("Bottom ", 	selectedVoxel.VTexture[1] );
						selectedVoxel.VTexture[2] = EditorGUILayout.Vector2Field ("Right ", 	selectedVoxel.VTexture[2] );	
						selectedVoxel.VTexture[3] = EditorGUILayout.Vector2Field ("Left ", 	selectedVoxel.VTexture[3]);	
						selectedVoxel.VTexture[4] = EditorGUILayout.Vector2Field ("Forward ",  selectedVoxel.VTexture[4]);
						selectedVoxel.VTexture[5] = EditorGUILayout.Vector2Field ("Back ", 	selectedVoxel.VTexture[5]);
					}
					else {
						selectedVoxel.VTexture[0] = EditorGUILayout.Vector2Field ("Texture ", selectedVoxel.VTexture[0]);	
					}
				}
				
				// rotation
				else {
					selectedVoxel.VRotation = (MeshRotation)EditorGUILayout.EnumPopup ("Mesh rotation", selectedVoxel.VRotation);
				}
				
				GUILayout.Space(10);
				
				// material index
				selectedVoxel.VSubmeshIndex = EditorGUILayout.IntField ("Material index", selectedVoxel.VSubmeshIndex);
				if (selectedVoxel.VSubmeshIndex <0) selectedVoxel.VSubmeshIndex = 0;
				
				// transparency
				selectedVoxel.VTransparency = (Transparency)EditorGUILayout.EnumPopup("Transparency", selectedVoxel.VTransparency);
				
				// collision
				selectedVoxel.VColliderType = (ColliderType)EditorGUILayout.EnumPopup("Collider", selectedVoxel.VColliderType);
				
				
				GUILayout.Space(10);
				
				
				
				// components
				
				
				GUILayout.Label ("Components");
				foreach (Object component in SelectedBlock.GetComponents<Component>()) {
					if (component is Transform == false && component is Voxel == false) {
						GUILayout.Label (component.GetType().ToString());
					}
					
				}
				
				
				GUILayout.Space(20);
				
				// apply
				if (GUILayout.Button("Apply", GUILayout.Height(80))) {
					
					if (SelectedPrefab != null 
					    && SelectedPrefab.GetComponent<Voxel>().GetID() != selectedVoxel.GetID() // if id was changed
					    && GetBlock(selectedVoxel.GetID()) != null ){ // and there is already a block with this id
						
						ReplaceBlockDialog = true;	
						
					}
					else {
						ReplaceBlockDialog = false;
						UpdateBlock();
						ApplyBlocks();
						GetBlocks();
					}
					
				}
				
				
				
				
				if (ReplaceBlockDialog) {
					GUILayout.Label ("A block with this ID already exists!"+SelectedPrefab.GetComponent<Voxel>().GetID()+selectedVoxel.GetID());
				}
				
				
				
			}
			
			
			
			
			EditorGUILayout.EndScrollView();
			EditorGUILayout.EndVertical();
			
			EditorGUILayout.EndHorizontal();
		
		
	}
	
	public void OnDestroy () {		
		DestroyImmediate(SelectedBlock);		
	}
	
	
	// ==== block logic ====
	
	private void SelectBlock ( GameObject block ) {
		
		DestroyImmediate(SelectedBlock); // destroy previously selected block		
		try {
			SelectedBlock = Instantiate (block, new Vector3(0,0,0), Quaternion.identity) as GameObject; // instantiate the newly selected block so we can work on it
		}
		catch (System.Exception) {
			Debug.LogError ("Uniblocks: Cannot find the block prefab! Have you deleted the block 0 prefab?");
		}
		SelectedPrefab = block;
		SelectedBlock.name = block.name;
		
		Selection.objects = new Object[] {SelectedBlock};
		
		//Debug.Log ("Selected block ID is:" + block.GetComponent<Voxel>().GetID().ToString());
	}
	
	private void CreateBlock ( ) { // instantiates a new block with a free name
		
		SelectBlock ( GetBlock(0) ); // select the empty block
		SelectedBlock.name = "block_" + (LastId+1); // set name
		
		SelectedPrefab = null; // there is no selected prefab yet!
		ReplaceBlockDialog = false;
		
		UpdateBlock();
		ApplyBlocks();
		GetBlocks();
	}
	
	private void UpdateBlock ( ) { // replaces selected block prefab with the scene instance (also sets the prefab name)

		SelectedBlock.name = "block_" + SelectedBlock.GetComponent<Voxel>().GetID();
		GameObject newPrefab = PrefabUtility.CreatePrefab(GetPrefabPath(SelectedBlock), SelectedBlock);
		SelectedPrefab = newPrefab;

	}
	
	void Update () {
		if (Input.GetKeyDown ("g")) {
			CreateBlock();
			UpdateBlock();
			ApplyBlocks();
		}
	}
	
	
	// ==== get, apply ====

	private string GetPath () {
		try {
			return EngineInstance.GetComponent<Engine>().lBlocksPath;
		}
		catch (System.Exception) {
			Debug.LogError ("Engine prefab not found!");
			return null;
		}
	}	
	private string GetBlockPath ( ushort data ) { // converts block id to prefab path		
		return GetPath() + "block_" + data + ".prefab";
	}
	private string GetPrefabPath ( GameObject block ) {
		return GetPath() + block.name.Split("("[0])[0] + ".prefab";
	}
	
	private void GetBlocks () { // populates the Blocks array		
			
		Blocks = new GameObject[ushort.MaxValue];		
		for (ushort i=0; i<ushort.MaxValue; i++) {
			GameObject block = GetBlock(i);
			if (block != null) {
				Blocks[i] = block;
				LastId = i;
			}
		}
			
		BlocksGotten = true;
	}
	
	private GameObject GetBlock ( ushort data ) { // returns the prefab of the block with a given index
	
		Object blockObject =  AssetDatabase.LoadAssetAtPath( GetBlockPath(data),typeof(Object) );
		GameObject block = null;
			
		if (blockObject != null) {
			block = (GameObject)blockObject;
		}
		else {
			return null;
		}
			
		if (block != null && block.GetComponent<Voxel>() != null) {	
			return block;			
		}
		else {
			return null;
		}
	}
		
	private void ApplyBlocks () { // gets all valid voxel prefabs and applies them to the lBlocks array in the Engine GameObject
		
		List<GameObject> voxels = new List<GameObject>();
		int empty = 0; // count of empty items between non-empty items
		
		for (ushort i=0; i<ushort.MaxValue; i++) {
			
			Object voxel = AssetDatabase.LoadAssetAtPath( GetBlockPath(i), typeof(Object) );
			
			if (voxel != null) { 
				while (empty > 0) { // add empty spaces
					voxels.Add(null);
					empty --;
				}				
				voxels.Add((GameObject)voxel); // add item
			}
			else {
				empty ++;
			}
		}		
		
		Engine engine = EngineInstance.GetComponent<Engine>();
		engine.lBlocks = voxels.ToArray();
		UnityEditor.PrefabUtility.ReplacePrefab(engine.gameObject, UnityEditor.PrefabUtility.GetPrefabParent(engine.gameObject), ReplacePrefabOptions.ConnectToPrefab);
	}
	
	private void SaveBlocks () {
		UpdateBlock();
		ApplyBlocks();
	}
		
}
	
}
