using UnityEngine;
using System.Collections;

namespace Uniblocks {

public class Voxel : MonoBehaviour {
	
	public string VName;
	public Mesh VMesh;
	public bool VCustomMesh;
	public bool VCustomSides;
	public Vector2[] VTexture; // index of the texture. Array index specifies face (VTexture[0] is the up-facing texture, for example)
	public Transparency VTransparency;
	public ColliderType VColliderType;
	public int VSubmeshIndex;
	public MeshRotation VRotation;

	
	public static void DestroyBlock ( VoxelInfo voxelInfo ) {
	
		// multiplayer - send change to server
		if (Engine.EnableMultiplayer) {
			Engine.UniblocksNetwork.GetComponent<UniblocksClient>().SendPlaceBlock ( voxelInfo, 0 );
		}
		
		// single player - apply change locally
		else {
			GameObject voxelObject = Instantiate ( Engine.GetVoxelGameObject (voxelInfo.GetVoxel()) ) as GameObject;
			if (voxelObject.GetComponent<VoxelEvents>() != null) {
				voxelObject.GetComponent<VoxelEvents>().OnBlockDestroy(voxelInfo);
			}
			voxelInfo.chunk.SetVoxel (voxelInfo.index, 0, true);
			Destroy (voxelObject);
		}
	}
	
	public static void PlaceBlock ( VoxelInfo voxelInfo, ushort data) {
		
		// multiplayer - send change to server
		if (Engine.EnableMultiplayer) {
			Engine.UniblocksNetwork.GetComponent<UniblocksClient>().SendPlaceBlock ( voxelInfo, data );
		}
		
		// single player - apply change locally
		else {
			voxelInfo.chunk.SetVoxel (voxelInfo.index, data, true);
		
			GameObject voxelObject = Instantiate ( Engine.GetVoxelGameObject (data) ) as GameObject;
			if (voxelObject.GetComponent<VoxelEvents>() != null) {
				voxelObject.GetComponent<VoxelEvents>().OnBlockPlace(voxelInfo);
			}
			Destroy (voxelObject);
		}
	}
	
	public static void ChangeBlock ( VoxelInfo voxelInfo, ushort data ) {
	
		// multiplayer - send change to server
		if (Engine.EnableMultiplayer) {
			Engine.UniblocksNetwork.GetComponent<UniblocksClient>().SendChangeBlock ( voxelInfo, data );
		}
		
		// single player - apply change locally
		else {
			voxelInfo.chunk.SetVoxel (voxelInfo.index, data, true);
		
			GameObject voxelObject = Instantiate ( Engine.GetVoxelGameObject (data) ) as GameObject;
			if (voxelObject.GetComponent<VoxelEvents>() != null) {
				voxelObject.GetComponent<VoxelEvents>().OnBlockChange(voxelInfo);
			}
			Destroy (voxelObject);
		}
	}
	
	// multiplayer
	
	public static void DestroyBlockMultiplayer ( VoxelInfo voxelInfo, NetworkPlayer sender ) { // received from server, don't use directly
		
		GameObject voxelObject = Instantiate ( Engine.GetVoxelGameObject (voxelInfo.GetVoxel()) ) as GameObject;
		VoxelEvents events = voxelObject.GetComponent<VoxelEvents>();
		if (events != null) {
			events.OnBlockDestroy(voxelInfo);
			events.OnBlockDestroyMultiplayer(voxelInfo, sender);
		}
		voxelInfo.chunk.SetVoxel (voxelInfo.index, 0, true);
		Destroy(voxelObject);
	}
	
	public static void PlaceBlockMultiplayer ( VoxelInfo voxelInfo, ushort data, NetworkPlayer sender ) { // received from server, don't use directly
		
		voxelInfo.chunk.SetVoxel (voxelInfo.index, data, true);
		
		GameObject voxelObject = Instantiate ( Engine.GetVoxelGameObject (data) ) as GameObject;
		VoxelEvents events = voxelObject.GetComponent<VoxelEvents>();
		if (events != null) {
			events.OnBlockPlace(voxelInfo);
			events.OnBlockPlaceMultiplayer(voxelInfo, sender);
		}
		Destroy (voxelObject);
	}
	
	public static void ChangeBlockMultiplayer ( VoxelInfo voxelInfo, ushort data, NetworkPlayer sender ) { // received from server, don't use directly
		
		voxelInfo.chunk.SetVoxel (voxelInfo.index, data, true);
		
		GameObject voxelObject = Instantiate ( Engine.GetVoxelGameObject (data) ) as GameObject;
		VoxelEvents events = voxelObject.GetComponent<VoxelEvents>();
		if (events != null) {
			events.OnBlockChange(voxelInfo);
			events.OnBlockChangeMultiplayer(voxelInfo, sender);
		}
		Destroy (voxelObject);
	}


	// block editor functions
	public ushort GetID () {
		return ushort.Parse(this.gameObject.name.Split('_')[1]);
		
	}
	
	public void SetID ( ushort id ) {
		this.gameObject.name = "block_" + id.ToString();
	}

}

}