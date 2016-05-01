using UnityEngine;
using System.Collections;

namespace Uniblocks {

public class ColliderEventsSender : MonoBehaviour {
		
	private Index LastIndex;
	private Chunk LastChunk;
		
	public void Update () {
	
		// check if chunk is not null
		GameObject chunkObject = Engine.PositionToChunk (transform.position);
		if (chunkObject == null) return;
		
		// get the voxelInfo from the transform's position
		Chunk chunk = chunkObject.GetComponent<Chunk>();
		Index voxelIndex = chunk.PositionToVoxelIndex (transform.position);
		VoxelInfo voxelInfo = new VoxelInfo (voxelIndex, chunk);
		
		// create a local copy of the collision voxel so we can call functions on it
		GameObject voxelObject = Instantiate ( Engine.GetVoxelGameObject (voxelInfo.GetVoxel()) ) as GameObject;
	
		VoxelEvents events = voxelObject.GetComponent<VoxelEvents>();
		if (events != null ) {
			
			// OnEnter
			if ( chunk != LastChunk || voxelIndex.IsEqual(LastIndex) == false) {
				events.OnBlockEnter (this.gameObject, voxelInfo);
			}
			
			// OnStay
			else {
				events.OnBlockStay(this.gameObject, voxelInfo);
			}
		}
		
		LastChunk = chunk;
		LastIndex = voxelIndex;
		
		Destroy(voxelObject);
		
	}
	
}

}

























