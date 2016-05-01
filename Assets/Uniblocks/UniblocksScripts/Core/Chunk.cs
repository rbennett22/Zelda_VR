using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Uniblocks {

public class Chunk : MonoBehaviour {
	
	// Chunk data
	public ushort[] VoxelData; // main voxel data array
	public Index ChunkIndex; // corresponds to the position of the chunk
	public Chunk[] NeighborChunks; // references to GameObjects of neighbor chunks
	public bool Empty;
	
	// Settings & flags
	public bool Fresh = true; 
	public bool EnableTimeout;
	public bool DisableMesh; // for chunks spawned from UniblocksServer; if true, the chunk will not build a mesh
	private bool FlaggedToRemove;
	public float Lifetime; // how long since the chunk has been spawned
	
	// update queue
	public bool	FlaggedToUpdate,
	InUpdateQueue,
	VoxelsDone; // true when this chunk has finished generating or loading voxel data

	
	// Semi-constants
	public int SideLength;
	private int SquaredSideLength;

	private ChunkMeshCreator MeshCreator;
	
	// object prefabs
	public GameObject MeshContainer, ChunkCollider;
	
	
	
	// ==== maintenance ===========================================================================================
	
	public void Awake () { // chunk initialization (load/generate data, set position, etc.)
		
		// Set variables
		ChunkIndex = new Index (transform.position);
		SideLength = Engine.ChunkSideLength;
		SquaredSideLength = SideLength * SideLength;
		NeighborChunks = new Chunk [6]; // 0 = up, 1 = down, 2 = right, 3 = left, 4 = forward, 5 = back
		MeshCreator = GetComponent<ChunkMeshCreator>();
		Fresh = true;
		
		// Register chunk
		ChunkManager.RegisterChunk (this);
		
		// Clear the voxel data
		VoxelData = new ushort[SideLength*SideLength*SideLength];
			
		// Set actual position
		transform.position = ChunkIndex.ToVector3() * SideLength;
		
		// multiply by scale
		transform.position = new Vector3 (transform.position.x * transform.localScale.x, transform.position.y * transform.localScale.y, transform.position.z * transform.localScale.z);
		
		// Grab voxel data
		if (Engine.EnableMultiplayer && !Network.isServer) {
			StartCoroutine (RequestVoxelData());	// if multiplayer, get data from server
		}
		else if (Engine.SaveVoxelData && TryLoadVoxelData() == true ) {
			// data is loaded through TryLoadVoxelData()
		}
		else {
			GenerateVoxelData();
		}
		
	}
	
	public bool TryLoadVoxelData () { // returns true if data was loaded successfully, false if data was not found
		return GetComponent<ChunkDataFiles>().LoadData();
	}
	
	public void GenerateVoxelData () {
		GetComponent<TerrainGenerator>().InitializeGenerator();
	}
	
	public void AddToQueueWhenReady () { // adds chunk to the UpdateQueue when this chunk and all known neighbors have their data ready
		StartCoroutine (DoAddToQueueWhenReady());
	}
	private IEnumerator DoAddToQueueWhenReady () {
		while (VoxelsDone == false || AllNeighborsHaveData() == false) {
			if (ChunkManager.StopSpawning) { // interrupt if the chunk spawn sequence is stopped. This will be restarted in the correct order from ChunkManager
				yield break;
			}
			yield return new WaitForEndOfFrame ();
			
		}
		ChunkManager.AddChunkToUpdateQueue (this);
	}
	
	private bool AllNeighborsHaveData () { // returns false if at least one neighbor is known but doesn't have data ready yet
		foreach (Chunk neighbor in NeighborChunks) {
			if (neighbor != null) {
				if (neighbor.VoxelsDone == false) {
					return false;
				}
			}
		}
		return true;
	}
	
	private void OnDestroy () {
		ChunkManager.UnregisterChunk (this);
	}
	
	
	// ==== data =======================================================================================
	
	public void ClearVoxelData () {
		VoxelData = new ushort[SideLength*SideLength*SideLength];
	}
	
	public int GetDataLength () {
		return VoxelData.Length;
	}
	

	// == set voxel
	public void SetVoxelSimple ( int rawIndex, ushort data ) {
		VoxelData [rawIndex] = data;
	}
	public void SetVoxelSimple ( int x, int y, int z, ushort data ) {
		VoxelData [(z * SquaredSideLength) + (y * SideLength) + x] = data;
	}
	public void SetVoxelSimple ( Index index, ushort data ) {
		VoxelData [(index.z * SquaredSideLength) + (index.y * SideLength) + index.x] = data;
	}
	public void SetVoxel ( int x, int y, int z, ushort data, bool updateMesh ) {
	
		// if outside of this chunk, change in neighbor instead (if possible)
		if (x < 0) {
			if (NeighborChunks[(int)Direction.left] != null)
				NeighborChunks[(int)Direction.left].SetVoxel(x+SideLength, y, z, data, updateMesh); return;
		}
		else if (x >= SideLength) {
			if (NeighborChunks[(int)Direction.right] != null)
				NeighborChunks[(int)Direction.right].SetVoxel(x-SideLength, y, z, data, updateMesh); return; 
		}		
		else if (y < 0 ) {
			if (NeighborChunks[(int)Direction.down] != null)
				NeighborChunks[(int)Direction.down].SetVoxel(x, y+SideLength, z, data, updateMesh); return; 
		}		
		else if (y >= SideLength) {
			if (NeighborChunks[(int)Direction.up] != null)
				NeighborChunks[(int)Direction.up].SetVoxel(x, y-SideLength, z, data, updateMesh); return; 
		}		
		else if (z < 0) {
			if (NeighborChunks[(int)Direction.back] != null)
				NeighborChunks[(int)Direction.back].SetVoxel(x, y, z+SideLength, data, updateMesh); return; 
		}		
		else if (z >= SideLength) {
			if (NeighborChunks[(int)Direction.forward] != null)
				NeighborChunks[(int)Direction.forward].SetVoxel(x, y, z-SideLength, data, updateMesh); return; 
		}
		
		VoxelData [(z * SquaredSideLength) + (y * SideLength) + x] = data;
		
		if (updateMesh) {
			UpdateNeighborsIfNeeded(x,y,z);
			FlagToUpdate();
		}
	}
	public void SetVoxel ( Index index, ushort data, bool updateMesh ) {
		SetVoxel (index.x, index.y, index.z, data, updateMesh);
	}
	
	// == get voxel
	public ushort GetVoxelSimple ( int rawIndex ) {
		return VoxelData[rawIndex];
	}
	public ushort GetVoxelSimple ( int x, int y, int z ) {
		return VoxelData [(z * SquaredSideLength) + (y * SideLength) + x];
	}
	public ushort GetVoxelSimple ( Index index ) {
		return VoxelData [(index.z * SquaredSideLength) + (index.y * SideLength) + index.x];
	}
	public ushort GetVoxel ( int x, int y, int z )  {
	
		if (x < 0) {
			if (NeighborChunks[(int)Direction.left] != null)	{
				return NeighborChunks[(int)Direction.left].GetVoxel(x+SideLength, y, z);
			} 
			else return ushort.MaxValue;
		}
		else if (x >= SideLength) {
			if (NeighborChunks[(int)Direction.right] != null) {
				return NeighborChunks[(int)Direction.right].GetVoxel(x-SideLength, y, z);
			}
			else return ushort.MaxValue;
		}
		else if (y < 0) {
			if (NeighborChunks[(int)Direction.down] != null) {
				return NeighborChunks[(int)Direction.down].GetVoxel(x, y+SideLength, z);
			}
			else return ushort.MaxValue;
		}
		else if (y >= SideLength) {
			if (NeighborChunks[(int)Direction.up] != null) {
				return NeighborChunks[(int)Direction.up].GetVoxel(x, y-SideLength, z);
			}
			else return ushort.MaxValue;
		}
		else if (z < 0) {
			if (NeighborChunks[(int)Direction.back] != null) {
				return NeighborChunks[(int)Direction.back].GetVoxel(x, y, z+SideLength);
			}
			else return ushort.MaxValue;
		}
		else if (z >= SideLength) {
			if (NeighborChunks[(int)Direction.forward] != null) {
				return NeighborChunks[(int)Direction.forward].GetVoxel(x, y, z-SideLength);
			}
			else return ushort.MaxValue;
		}
		
		
		else {
			return VoxelData [(z * SquaredSideLength) + (y * SideLength) + x];
		}
	}
	public ushort GetVoxel ( Index index ) {
		return GetVoxel (index.x, index.y, index.z);
	}

	
	// ==== Flags =======================================================================================
	
	public void FlagToRemove () {
		FlaggedToRemove = true;
	}
	public void FlagToUpdate () {
		FlaggedToUpdate = true;
	}
	

	// ==== Update ====
	
	public void Update () {
		ChunkManager.SavesThisFrame = 0;
	}
	
	public void LateUpdate () {
	
		// timeout
		if (Engine.EnableChunkTimeout && EnableTimeout) {
			Lifetime += Time.deltaTime;
			if (Lifetime > Engine.ChunkTimeout) {
				FlaggedToRemove = true;
			}
		}
	
		if (FlaggedToUpdate && VoxelsDone && !DisableMesh && Engine.GenerateMeshes) { // check if we should update the mesh
			FlaggedToUpdate = false;
			RebuildMesh();	
		}
		
		if (FlaggedToRemove) {
			
			if (Engine.SaveVoxelData) { // save data over time, destroy chunk when done
				if (ChunkDataFiles.SavingChunks == false) { // only destroy chunks if they are not being saved currently
					if (ChunkManager.SavesThisFrame < Engine.MaxChunkSaves) {
						ChunkManager.SavesThisFrame ++;
						SaveData();
						Destroy(this.gameObject);
					}
				}
			}
			
			else { // if saving is disabled, destroy immediately
				Destroy(this.gameObject);
			}
			
		}
	}
	
	public void RebuildMesh () {
		MeshCreator.RebuildMesh();
		ConnectNeighbors ();
	}

	
	private void SaveData () {	
	
		if (Engine.SaveVoxelData == false) {
			Debug.LogWarning ("Uniblocks: Saving is disabled. You can enable it in the Engine Settings.");
			return;
		}
	
		if (Application.isWebPlayer == false) {	
			GetComponent<ChunkDataFiles>().SaveData();		
		}
	}	
	
	
	
	// ==== Neighbors =======================================================================================
	
	public void ConnectNeighbors () { // update the mesh on all neighbors that have a mesh but don't know about this chunk yet, and also pass them the reference to this chunk
		
		int loop = 0;
		int i = loop;
		
		while (loop < 6) {
			if (loop % 2 == 0) { // for even indexes, add one; for odd, subtract one (because the neighbors are in opposite direction to this chunk)
				i = loop+1;
			}
			else {
				i = loop-1;
			}
			
			if (NeighborChunks[loop] != null && NeighborChunks[loop].gameObject.GetComponent<MeshFilter>().sharedMesh != null) {
				if (NeighborChunks[loop].NeighborChunks[i] == null) {
					NeighborChunks[loop].AddToQueueWhenReady();
					NeighborChunks[loop].NeighborChunks[i] = this;
				}
			}	
			
			loop ++;
		}
	}
	
	public void GetNeighbors () { // assign the neighbor chunk gameobjects to the NeighborChunks array
	
		int x = ChunkIndex.x;
		int y = ChunkIndex.y;
		int z = ChunkIndex.z;
		
		if (NeighborChunks[0] == null) 		NeighborChunks[0] = ChunkManager.GetChunkComponent(x,y+1,z);
		if (NeighborChunks[1] == null) 		NeighborChunks[1] = ChunkManager.GetChunkComponent(x,y-1,z);
		if (NeighborChunks[2] == null) 		NeighborChunks[2] = ChunkManager.GetChunkComponent(x+1,y,z);
		if (NeighborChunks[3] == null) 		NeighborChunks[3] = ChunkManager.GetChunkComponent(x-1,y,z);
		if (NeighborChunks[4] == null) 		NeighborChunks[4] = ChunkManager.GetChunkComponent(x,y,z+1);
		if (NeighborChunks[5] == null) 		NeighborChunks[5] = ChunkManager.GetChunkComponent(x,y,z-1);
	
	}
	
	public Index GetAdjacentIndex (Index index, Direction direction) {
		return GetAdjacentIndex (index.x, index.y, index.z, direction);
	} 
	
	public Index GetAdjacentIndex (int x, int y, int z, Direction direction) { // converts x,y,z, direction into a specific index
		
		if (direction == Direction.down) 			return new Index(x,y-1,z);
		else if (direction == Direction.up)	 		return new Index(x,y+1,z);
		else if (direction == Direction.left) 		return new Index(x-1,y,z);
		else if (direction == Direction.right) 		return new Index(x+1,y,z);
		else if (direction == Direction.back) 		return new Index(x,y,z-1);
		else if (direction == Direction.forward) 	return new Index(x,y,z+1);
		
		
		else {
			Debug.LogError("Chunk.GetAdjacentIndex failed! Returning default index.");
			return new Index(x,y,z);	
		}
	}
	
	
	public void UpdateNeighborsIfNeeded (int x, int y, int z) { // if the index lies at the border of a chunk, FlagToUpdate the neighbor at that border
		
		if (x == 0 && NeighborChunks [(int)Direction.left] != null) { 
			NeighborChunks [(int)Direction.left].GetComponent<Chunk>().FlagToUpdate(); }
					
		else if (x == SideLength-1 && NeighborChunks [(int)Direction.right] != null) { 
			NeighborChunks [(int)Direction.right].GetComponent<Chunk>().FlagToUpdate(); }
			
		if (y == 0 && NeighborChunks [(int)Direction.down] != null) { 
			NeighborChunks [(int)Direction.down].GetComponent<Chunk>().FlagToUpdate(); }
			
		else if (y == SideLength-1 && NeighborChunks [(int)Direction.up] != null) { 
			NeighborChunks [(int)Direction.up].GetComponent<Chunk>().FlagToUpdate(); }
			
		if (z == 0 && NeighborChunks [(int)Direction.back] != null) { 
			NeighborChunks [(int)Direction.back].GetComponent<Chunk>().FlagToUpdate(); }
			
		else if (z == SideLength-1 && NeighborChunks [(int)Direction.forward] != null) { 
			NeighborChunks [(int)Direction.forward].GetComponent<Chunk>().FlagToUpdate(); }
	}
	
	
	// ==== position / voxel index =======================================================================================
	
	
	public Index PositionToVoxelIndex ( Vector3 position ) { 

		Vector3 point = transform.InverseTransformPoint (position);
		
		// round it to get an int which we can convert to the voxel index
		Index index = new Index(0,0,0);
		index.x = Mathf.RoundToInt(point.x);
		index.y = Mathf.RoundToInt(point.y);
		index.z = Mathf.RoundToInt(point.z);
		
		return index;
	}
	
	public Vector3 VoxelIndexToPosition ( Index index ) {
		
		Vector3 localPoint = index.ToVector3(); // convert index to chunk's local position
		return transform.TransformPoint(localPoint);// convert local position to world space
		
	}
	
	public Vector3 VoxelIndexToPosition ( int x, int y, int z ) {
		
		Vector3 localPoint = new Vector3 (x,y,z); // convert index to chunk's local positio
		return transform.TransformPoint(localPoint);// convert local position to world space
	}
	
	public Index PositionToVoxelIndex ( Vector3 position, Vector3 normal, bool returnAdjacent ) { // converts the absolute position to the index of the voxel
	
		if (returnAdjacent == false) {
			position = position - (normal*0.25f); // push the hit point into the cube
		} else {
			position = position + (normal*0.25f); // push the hit point outside of the cube
		}
		

		// convert world position to chunk's local position
		Vector3 point = transform.InverseTransformPoint (position);
								
																								
		// round it to get an int which we can convert to the voxel index
		Index index = new Index(0,0,0);
		index.x = Mathf.RoundToInt(point.x);
		index.y = Mathf.RoundToInt(point.y);
		index.z = Mathf.RoundToInt(point.z);
		
		return index;
	}
	
	
	// ==== network ==============
	public static int CurrentChunkDataRequests; // how many chunk requests are currently queued in the server for this client. Increased by 1 every time a chunk requests data, and reduced by 1 when a chunk receives data.
	IEnumerator RequestVoxelData () { // waits until we're connected to a server and then sends a request for voxel data for this chunk to the server
		while (!Network.isClient) { 
			Chunk.CurrentChunkDataRequests = 0; // reset the counter if we're not connected
			yield return new WaitForEndOfFrame();
		}
		while (Engine.MaxChunkDataRequests != 0 && Chunk.CurrentChunkDataRequests >= Engine.MaxChunkDataRequests) {
			yield return new WaitForEndOfFrame();
		}
		
		Chunk.CurrentChunkDataRequests ++;
		Engine.UniblocksNetwork.GetComponent<NetworkView>().RPC ("SendVoxelData", RPCMode.Server, Network.player, ChunkIndex.x, ChunkIndex.y, ChunkIndex.z);
	}

}

}
