using UnityEngine;
using System.Collections;

// inherit from this class if you want to use the default events as well as custom ones

namespace Uniblocks {

public class DefaultVoxelEvents : VoxelEvents {

	public override void OnMouseDown ( int mouseButton, VoxelInfo voxelInfo ) {
		
		if ( mouseButton == 0 ) { // destroy a block with LMB
			Voxel.DestroyBlock (voxelInfo); 
		}
		else if ( mouseButton == 1 ) { // place a block with RMB
		
			if ( voxelInfo.GetVoxel() == 8 ) { // if we're looking at a tall grass block, replace it with the held block
				Voxel.PlaceBlock (voxelInfo, ExampleInventory.HeldBlock);
			}
			else { // else put the block next to the one we're looking at
				VoxelInfo newInfo = new VoxelInfo (voxelInfo.adjacentIndex, voxelInfo.chunk); // use adjacentIndex to place the block
				Voxel.PlaceBlock (newInfo, ExampleInventory.HeldBlock);
			}
		}
		
	}

	public override void OnLook ( VoxelInfo voxelInfo ) {
		
		// move the selected block ui to the block that's being looked at (convert the index of the hit voxel to absolute world position)
		GameObject blockSelection = GameObject.Find ("selected block graphics");
		if (blockSelection != null) {
			blockSelection.transform.position = voxelInfo.chunk.VoxelIndexToPosition (voxelInfo.index);
			blockSelection.GetComponent<Renderer>().enabled = true;
			blockSelection.transform.rotation = voxelInfo.chunk.transform.rotation;
		}
				
	}

	public override void OnBlockPlace ( VoxelInfo voxelInfo ) {
		
		// if the block below is grass, change it to dirt
		Index indexBelow = new Index (voxelInfo.index.x, voxelInfo.index.y-1, voxelInfo.index.z);	
			
		if ( voxelInfo.GetVoxelType ().VTransparency == Transparency.solid 
	    && voxelInfo.chunk.GetVoxel(indexBelow) == 2) {	    	    
			voxelInfo.chunk.SetVoxel(indexBelow, 1, true);
		}			
	}
	
	public override void OnBlockDestroy ( VoxelInfo voxelInfo ) {
		
		// if the block above is tall grass, destroy it
		Index indexAbove = new Index (voxelInfo.index.x, voxelInfo.index.y+1, voxelInfo.index.z);
		
		if ( voxelInfo.chunk.GetVoxel(indexAbove) == 8 ) {
			voxelInfo.chunk.SetVoxel(indexAbove, 0, true);
		}
		
	}
	
	public override void OnBlockEnter ( GameObject enteringObject, VoxelInfo voxelInfo ) {
	
		Debug.Log ("OnBlockEnter at " + voxelInfo.chunk.ChunkIndex.ToString() + " / " + voxelInfo.index.ToString());
	
	}

}

}

