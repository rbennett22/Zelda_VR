using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Uniblocks {
[RequireComponent(typeof(UniblocksClient))]
public class UniblocksServer : MonoBehaviour {
	
	public bool EnableDebugLog;
	public float AutosaveTime; // how often to autosave voxel data; 0 is never
	private float autosaveTimer;
	
	private Dictionary<NetworkPlayer,Index> PlayerPositions; // stores the index of each player's origin chunk. Changes will only be sent if the change is within their radius
	private Dictionary<NetworkPlayer,int> PlayerChunkSpawnDistances; // chunk spawn distance for each player
	
		
	
	void Awake () {
		if (Engine.EnableMultiplayer == false) 	Debug.LogWarning ("Uniblocks: Multiplayer is disabled. Unexpected behavior may occur.");
		
		Engine.UniblocksNetwork = this.gameObject;
		
		ResetPlayerData();
		if (Network.isServer) {
			Debug.Log ("UniblocksServer: Server initialized.");
		}
		
	}
	
	
	void ResetPlayerData () {
		PlayerPositions = new Dictionary<NetworkPlayer,Index>();// reset/initialize player origins
		PlayerChunkSpawnDistances = new Dictionary<NetworkPlayer,int>(); // reset/initialize player chunk spawn distances
	}
	
	void OnPlayerConnected () {
		if (EnableDebugLog) Debug.Log ("UniblocksServer: Player connected to server.");
	}
	
	void OnPlayerDisconnected (NetworkPlayer player) {
		if (Engine.MultiplayerTrackPosition) {
			PlayerPositions.Remove (player);
			PlayerChunkSpawnDistances.Remove (player);
		}
		if (EnableDebugLog) Debug.Log ("UniblocksServer: Player disconnected from server.");
	}
	
	
	// ===== send chunk data
	
	[RPC]
	public void SendVoxelData ( NetworkPlayer player, int chunkx, int chunky, int chunkz ) {
	
		// >> You can check whether the request for voxel data is valid here <<
		// if (true) {
			Chunk chunk = ChunkManager.SpawnChunkFromServer (chunkx,chunky,chunkz).GetComponent<Chunk>(); // get the chunk (spawn it if it's not spawned already)
			chunk.Lifetime = 0f; // refresh the chunk's lifetime
			string data = ChunkDataFiles.CompressData (chunk); // get data from the chunk and compress it
			byte[] dataBytes = GetBytes (data); // convert to byte array (sending strings over RPC doesn't work too well)
			GetComponent<NetworkView>().RPC ("ReceiveVoxelData", player, chunkx, chunky, chunkz, dataBytes); // send compressed data to the player who requested it
		// }
	}
	
	
	// ===== receive / distribute voxel changes
	
	[RPC]
	public void ServerPlaceBlock ( NetworkPlayer sender, int x, int y, int z, int chunkx, int chunky, int chunkz, int data ) {
		
		if (EnableDebugLog) Debug.Log ("UniblocksServer: Received PlaceBlock from player " + sender.ToString());
		// You can check whether the change sent by the client is valid here
		// if (true) {
			DistributeChange (sender, x,y,z, chunkx, chunky,chunkz, data, false);
		// }
	}
	
	[RPC]
	public void ServerChangeBlock ( NetworkPlayer sender, int x, int y, int z, int chunkx, int chunky, int chunkz, int data ) {
		
		if (EnableDebugLog) Debug.Log ("UniblocksServer: Received ChangeBlock from player " + sender.ToString());
		// You can check whether the change sent by the client is valid here
		// if (true) {
			DistributeChange (sender, x,y,z, chunkx, chunky,chunkz, data, true);
		// }
	}
	
	void DistributeChange ( NetworkPlayer sender, int x, int y, int z, int chunkx, int chunky, int chunkz, int data, bool isChangeBlock ) { // sends a change in the voxel data to all clients
		
		// update server
		ApplyOnServer (x,y,z, chunkx, chunky, chunkz, data, isChangeBlock); // the server can't send RPCs to itself so we'll need to call them directly
		
		// send to every client
		foreach (NetworkPlayer player in Network.connections) {
			if (player != Network.player) { // skip server
			if (Engine.MultiplayerTrackPosition==false || IsWithinRange (player, new Index (chunkx,chunky,chunkz))) { // check if the change is within range of each player
				
				if (EnableDebugLog)  Debug.Log ("UniblocksServer: Sending voxel change to player " + player.ToString());
				
				if (isChangeBlock) {
					GetComponent<NetworkView>().RPC ("ReceiveChangeBlock", player, sender, x,y,z, chunkx, chunky,chunkz, data);
				}
				else {
					GetComponent<NetworkView>().RPC ("ReceivePlaceBlock", player, sender, x,y,z, chunkx, chunky,chunkz, data);
				}
			}
			}
		}
	}
	
	bool IsWithinRange ( NetworkPlayer player, Index chunkIndex ) { // checks if the player is within the range of the chunk
		
		if ( Mathf.Abs ( PlayerPositions [player].x - chunkIndex.x ) > PlayerChunkSpawnDistances[player] ) {
			return false;
		}
		if ( Mathf.Abs ( PlayerPositions [player].y - chunkIndex.y ) > PlayerChunkSpawnDistances[player] ) {
			return false;
		}
		if ( Mathf.Abs ( PlayerPositions [player].z - chunkIndex.z ) > PlayerChunkSpawnDistances[player] ) {
			return false;
		}
		
		return true;
	}
	
	
	void ApplyOnServer ( int x, int y, int z, int chunkx, int chunky, int chunkz, int data, bool isChangeBlock ) { // updates the voxel data stored on the server with the change sent by client
		Chunk chunk = ChunkManager.SpawnChunkFromServer (chunkx,chunky,chunkz).GetComponent<Chunk>();// if chunk is not loaded, load it
		chunk.Lifetime = 0f; // refresh the chunk's lifetime
		if (isChangeBlock) {
			GetComponent<UniblocksClient>().ReceiveChangeBlock (Network.player, x,y,z, chunkx,chunky,chunkz, data);
		}
		else {
			GetComponent<UniblocksClient>().ReceivePlaceBlock (Network.player, x,y,z, chunkx,chunky,chunkz, data);
		}
	}
	
	
	[RPC]
	public void UpdatePlayerPosition ( NetworkPlayer player, int chunkx, int chunky, int chunkz ) { // sent by client
		PlayerPositions [player] = new Index (chunkx, chunky, chunkz);
		if (EnableDebugLog) Debug.Log ("UniblocksServer: Updated player position. Player "+player.ToString() + ", position: " + new Vector3(chunkx,chunky,chunkz).ToString());
	}
	
	[RPC]
	public void UpdatePlayerRange ( NetworkPlayer player, int range ) { // sent by client
		PlayerChunkSpawnDistances [player] = range;
		if (EnableDebugLog) Debug.Log ("UniblocksServer: Updated player range. Player: "+player.ToString() + ", range: " + range.ToString());
	}
	
	// ===== save data
	
	void Update () {
	
		if (AutosaveTime > 0.0001f) {
			if (autosaveTimer >= AutosaveTime) {
				autosaveTimer = 0;
				Engine.SaveWorld();
			}
			else {
				autosaveTimer += Time.deltaTime;
			}
		}
	}
	
	
	// convert string to byte array
	static byte[] GetBytes(string str)
	{
	    byte[] bytes = new byte[str.Length * sizeof(char)];
	    System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
	    return bytes;
	}


	
}

}