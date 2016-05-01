using UnityEngine;
using System.Collections;

namespace Uniblocks {

public class UniblocksClient : MonoBehaviour {
		
	void OnServerDisconnected () {
		Destroy(this.gameObject);
	}
	
	public void OnConnectedToServer () {
		Debug.Log ("UniblocksClient: Connected to server.");
		if (Engine.EnableMultiplayer == false) 	Debug.LogWarning ("Uniblocks: Multiplayer is disabled. Unexpected behavior may occur.");
		Engine.SaveVoxelData = false; // disable local saving for client
	}

	
	// ===== network communication ============
	
	public static void UpdatePlayerPosition (int x, int y, int z) {
		Engine.UniblocksNetwork.GetComponent<NetworkView>().RPC ("UpdatePlayerPosition", RPCMode.Server, Network.player, x, y, z);
	}
	public static void UpdatePlayerPosition (Index index) {
		Engine.UniblocksNetwork.GetComponent<NetworkView>().RPC ("UpdatePlayerPosition", RPCMode.Server, Network.player, index.x, index.y, index.z);
	}
	public static void UpdatePlayerRange (int range) {
		Engine.UniblocksNetwork.GetComponent<NetworkView>().RPC ("UpdatePlayerRange", RPCMode.Server, Network.player, range);
	}
	

	[RPC]
	public void ReceiveVoxelData ( int chunkx, int chunky, int chunkz, byte[] data ) {
		
		 
		GameObject chunkObject = ChunkManager.GetChunk (chunkx,chunky,chunkz); // find the chunk
		if (chunkObject == null) 	return; // abort if chunk isn't spawned anymore
		Chunk chunk = chunkObject.GetComponent<Chunk>();
		
		ChunkDataFiles.DecompressData (chunk, GetString(data)); // decompress data
//		ChunkManager.DataReceivedCount ++; // let ChunkManager know that we have received the data
		chunk.VoxelsDone = true; // let Chunk know that it can update it's mesh
		Chunk.CurrentChunkDataRequests --;
	}
	
	
	public void SendPlaceBlock ( VoxelInfo info, ushort data ) {	// sends a voxel change to the server, which then redistributes it to other clients
		
		// convert to ints
		int chunkx = info.chunk.ChunkIndex.x;
		int chunky = info.chunk.ChunkIndex.y;
		int chunkz = info.chunk.ChunkIndex.z;
		
		// send to server
		if (Network.isServer) {
			GetComponent<UniblocksServer>().ServerPlaceBlock (Network.player, info.index.x, info.index.y, info.index.z, chunkx,chunky,chunkz, (int)data);
		}
		else {
			GetComponent<NetworkView>().RPC ("ServerPlaceBlock", RPCMode.Server, Network.player, info.index.x, info.index.y, info.index.z, chunkx,chunky,chunkz, (int)data);
		}
	}
	
	public void SendChangeBlock ( VoxelInfo info, ushort data ) {
	
		// convert to ints
		int chunkx = info.chunk.ChunkIndex.x;
		int chunky = info.chunk.ChunkIndex.y;
		int chunkz = info.chunk.ChunkIndex.z;
		
		// send to server
		if (Network.isServer) {
			GetComponent<UniblocksServer>().ServerChangeBlock (Network.player, info.index.x, info.index.y, info.index.z, chunkx,chunky,chunkz, (int)data);
		}
		else {
			GetComponent<NetworkView>().RPC ("ServerChangeBlock", RPCMode.Server, Network.player, info.index.x, info.index.y, info.index.z, chunkx,chunky,chunkz, (int)data);
		}
	}
	
	[RPC]
	public void ReceivePlaceBlock ( NetworkPlayer sender, int x, int y, int z, int chunkx, int chunky, int chunkz, int data ) {	// receives a change sent by other client or server
		
		GameObject chunkObject = ChunkManager.GetChunk (chunkx,chunky,chunkz);
		if (chunkObject != null) {
		
			// convert back to VoxelInfo
			Index voxelIndex = new Index (x,y,z);
			VoxelInfo info = new VoxelInfo (voxelIndex, chunkObject.GetComponent<Chunk>());
			
			// apply change
			if (data == 0) {
				Voxel.DestroyBlockMultiplayer (info, sender);
			}
			else {
				Voxel.PlaceBlockMultiplayer (info, (ushort)data, sender);
			}
		}
	}
	
	[RPC]
	public void ReceiveChangeBlock ( NetworkPlayer sender, int x, int y, int z, int chunkx, int chunky, int chunkz, int data ) {	// receives a change sent by other client or server
		
		GameObject chunkObject = ChunkManager.GetChunk (chunkx,chunky,chunkz);
		if (chunkObject != null) {
		
			// convert back to VoxelInfo
			Index voxelIndex = new Index (x,y,z);
			VoxelInfo info = new VoxelInfo (voxelIndex, chunkObject.GetComponent<Chunk>());
			
			// apply change
			Voxel.ChangeBlockMultiplayer (info, (ushort)data, sender);
		}
	}
	
	// convert back to string
	static string GetString(byte[] bytes)
	{
	    char[] chars = new char[bytes.Length / sizeof(char)];
	    System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
	    return new string(chars);
	}
	
}

}