using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;


namespace Uniblocks {

public class ChunkDataFiles : MonoBehaviour {
		
	public static bool SavingChunks;
	public static Dictionary<string, string> TempChunkData; // stores chunk's data to write into a region file later
	public static Dictionary<string, string[]> LoadedRegions; // data of currently loaded regions
	

	public bool LoadData () { // attempts to load data from file, returns false if data is not found
		
		Chunk chunk = GetComponent<Chunk>();
		string chunkData = GetChunkData (chunk.ChunkIndex);
		
		if (chunkData != "") {
			
			ChunkDataFiles.DecompressData (chunk, GetChunkData(chunk.ChunkIndex));
			chunk.VoxelsDone = true;
			return true;
		}
		
		else {
			return false;
		}
	}

	public void SaveData () {
	
		Chunk chunk = GetComponent<Chunk>();
		string compressedData = ChunkDataFiles.CompressData (chunk);
		
		WriteChunkData (chunk.ChunkIndex, compressedData);
	}
	
	public static void DecompressData ( Chunk chunk, string data ) { // decompresses voxel data and loads it into the VoxelData array
		
		// check if chunk is empty
		if (data.Length == 2 && data[1] == (char)0) {
			chunk.Empty = true;
		}
		
		StringReader reader = new StringReader (data);
		
		int i = 0;
		int length = chunk.GetDataLength(); // length of VoxelData array
		
		try {
		while ( i < length ) { // this loop will stop once the VoxelData array has been populated completely. Iterates once per count-data block.
		
			ushort currentCount =  (ushort) reader.Read(); // read the count
			ushort currentData = (ushort) reader.Read(); // read the data
			
			int ii = 0;
				
			while ( ii < currentCount ) {
				chunk.SetVoxelSimple (i, (ushort)currentData);// write a single voxel for every currentCount
				ii ++;
				i ++;
			}
		}
		}
		catch (System.Exception) {
			Debug.LogError ("Uniblocks: Corrupt chunk data for chunk: " + chunk.ChunkIndex.ToString() + ". Has the data been saved using a different chunk size?");
			reader.Close();
			return;
		}
		
		reader.Close();
		
	}
	
	public static string CompressData ( Chunk chunk ) { // returns the data of chunk in compressed string format
		
		StringWriter writer = new StringWriter();
		
		int i = 0;
		int length = chunk.GetDataLength(); // length of VoxelData array
		
		ushort currentCount = 0; // count of consecutive voxels of the same type
		ushort currentData = 0; // data of the current voxel
		
		for (i=0; i<length; i++) { // for each voxel
		
			ushort thisData = chunk.GetVoxelSimple(i); // read raw data at i
			
			if (thisData != currentData) { // if the data is different from the previous data, write the last block and start a new one
			
				// write previous block 
				if (i != 0) { // (don't write in the first loop iteration, because count would be 0 (no previous blocks))
					writer.Write ((char)currentCount);
					writer.Write ((char)currentData);
				}
				// start new block
				currentCount = 1;
				currentData = thisData;
			}
			
			else { // if the data is the same as the last data, simply add to the count
				currentCount ++;
			}
			
			if (i == length-1) { // if this is the last iteration of the loop, close and write the current block
				writer.Write ((char)currentCount);
				writer.Write ((char)currentData);
			}
	
		}
		
		string compressedData = writer.ToString();
		writer.Flush ();
		writer.Close ();
		return compressedData;
		
	}
	
	private string GetChunkData (Index index) { // returns the chunk data (from memory or from file), or an empty string if data can't be found
		
		// try to load from TempChunkData
		string indexString = index.ToString();
		if (TempChunkData.ContainsKey (indexString)) {
			return TempChunkData[indexString];
		}
		
		// try to load from region, return empty if not found
		int regionIndex = GetChunkRegionIndex (index);
		string[] regionData = GetRegionData (GetParentRegion(index));
		if (regionData == null) {
			return "";
		}
		return regionData[regionIndex];
		
	}
	
	private void WriteChunkData (Index index, string data) { // writes the chunk data to the TempChunkData dictionary
		TempChunkData[index.ToString()] = data;
	}
	
	private static int GetChunkRegionIndex ( Index index ) { // returns the 1d index of a chunk's data in the region file
		
		Index newIndex = new Index (index.x, index.y, index.z);
		if (newIndex.x < 0) newIndex.x = -newIndex.x - 1;
		if (newIndex.y < 0) newIndex.y = -newIndex.y - 1;
		if (newIndex.z < 0) newIndex.z = -newIndex.z - 1;
		
		int flatIndex =  (newIndex.z * 100) + (newIndex.y * 10) + newIndex.x;
		
		while (flatIndex > 999) {
			flatIndex -= 1000;
		}
		
		return flatIndex;
	}
	
	private static string[] GetRegionData (Index regionIndex) { // loads region data and from file returns it, or returns null if region file is not found
		
		if (LoadRegionData(regionIndex) == true) {
			return LoadedRegions[regionIndex.ToString()];
		}
		else {
			return null;
		}
	}
	
	private static bool LoadRegionData (Index regionIndex) { // loads the region data into memory if file exists and it's not already loaded, returns true if data exists (and is loaded), else false
	
		string indexString = regionIndex.ToString();
		if (LoadedRegions.ContainsKey(indexString) == false) { // if not loaded
		
			// load data if region file exists
			string regionPath = GetRegionPath (regionIndex);
			if (File.Exists (regionPath)) {
		
				StreamReader reader = new StreamReader (regionPath);
				string[] regionData = reader.ReadToEnd().Split((char)ushort.MaxValue);
				reader.Close();
				LoadedRegions[indexString] = regionData;
				
				return true;
				
			}
			
			else {
				return false; // return false if region file doesn't exist
			}
		}
			
		return true; // return true if data is already loaded
	}
	
	private static string GetRegionPath (Index regionIndex) {
		
		return Engine.WorldPath + ( regionIndex.ToString() + ",.region");
	}
	
	private static Index GetParentRegion ( Index index ) { // returns the index of the region containing a specific chunk
		
		Index newIndex = new Index(index.x, index.y, index.z);
		
		if (index.x < 0) newIndex.x -= 9;
		if (index.y < 0) newIndex.y -= 9;
		if (index.z < 0) newIndex.z -= 9;
		
		int x = newIndex.x / 10;
		int y = newIndex.y / 10;
		int z = newIndex.z / 10;
		
		return new Index (x,y,z);
	}
	
	private static void CreateRegionFile (Index index) { // creates an empty region file
	
		Directory.CreateDirectory (Engine.WorldPath);
		StreamWriter writer = new StreamWriter (GetRegionPath(index));
		
		for (int i=0; i<999; i++) {
			writer.Write ((char)ushort.MaxValue);
		}
		
		writer.Flush();
		writer.Close();
	}
	
	public static IEnumerator SaveAllChunks () {
		
		if (!Engine.SaveVoxelData) {
			Debug.LogWarning ("Uniblocks: Saving is disabled. You can enable it in the Engine Settings.");
			yield break;
		}
	
		while (ChunkDataFiles.SavingChunks) {
			yield return new WaitForEndOfFrame();
		}	
		ChunkDataFiles.SavingChunks = true;
	
		// for each chunk object, save data to memory
		int count = 0;
		List<Chunk> chunksToSave = new List<Chunk> (ChunkManager.Chunks.Values);
		
		
		foreach (Chunk chunk in chunksToSave) {
			chunk.gameObject.GetComponent<ChunkDataFiles>().SaveData();
			count++;
			if (count > Engine.MaxChunkSaves) {
				yield return new WaitForEndOfFrame();
				count = 0;
			}
		}	
		
		// write data to disk
		ChunkDataFiles.WriteLoadedChunks ();
		ChunkDataFiles.SavingChunks = false;
		
		Debug.Log ("Uniblocks: World saved successfully.");
	}
	
	public static void SaveAllChunksInstant () { // writes data from TempChunkData into region files
	
		if (!Engine.SaveVoxelData) {
			Debug.LogWarning ("Uniblocks: Saving is disabled. You can enable it in the Engine Settings.");
			return;
		}
		
		// for each chunk object, save data to memory
		foreach (Chunk chunk in ChunkManager.Chunks.Values) {
			chunk.gameObject.GetComponent<ChunkDataFiles>().SaveData();
		}
	
		// write data to disk
		ChunkDataFiles.WriteLoadedChunks ();
		
		Debug.Log ("Uniblocks: World saved successfully. (Instant)");
	
	}
	
	public static void WriteLoadedChunks () { // writes all chunk data from memory to disk, and clears memory
		
		// for every chunk loaded in dictionary
		foreach (string chunkIndex in TempChunkData.Keys) {
			
			Index index = Index.FromString (chunkIndex);
			string region = GetParentRegion (index).ToString();
			
			// check if region is loaded, and load it if it's not
			if ( LoadRegionData ( GetParentRegion (index) ) == false) {
				CreateRegionFile ( GetParentRegion (index) );
				LoadRegionData (GetParentRegion(index));
			}
			
			// write chunk data into region dictionary
			int chunkRegionIndex = GetChunkRegionIndex (index);
			LoadedRegions[region][chunkRegionIndex] = TempChunkData[chunkIndex];
		}
		TempChunkData.Clear();
		
		
		// for every region loaded in dictionary
		foreach (string regionIndex in LoadedRegions.Keys) {
			WriteRegionFile (regionIndex);
		}
		LoadedRegions.Clear();
		
	}
	
	private static void WriteRegionFile ( string regionIndex ) {
		
		string[] regionData = LoadedRegions [regionIndex];
		
		StreamWriter writer = new StreamWriter (GetRegionPath( Index.FromString(regionIndex) ));
		int count = 0;
		foreach (string chunk in regionData) {
			writer.Write(chunk);
			if (count != regionData.Length-1) {
				writer.Write((char)ushort.MaxValue);
			}
			count ++;
		}
		
		writer.Flush();
		writer.Close();
	}
}

}