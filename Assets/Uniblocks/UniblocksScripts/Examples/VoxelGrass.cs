using UnityEngine;
using System.Collections;

namespace Uniblocks {

public class VoxelGrass : DefaultVoxelEvents  {

	public override void OnBlockPlace ( VoxelInfo voxelInfo ) {
		
		// switch to dirt if the block above isn't 0
		Index adjacentIndex = voxelInfo.chunk.GetAdjacentIndex (voxelInfo.index, Direction.up);
		if ( voxelInfo.chunk.GetVoxel(adjacentIndex) != 0 ) {
			voxelInfo.chunk.SetVoxel(voxelInfo.index, 1, true); 
		}
		
		// if the block below is grass, change it to dirt
		Index indexBelow = new Index (voxelInfo.index.x, voxelInfo.index.y-1, voxelInfo.index.z);
			
		if ( voxelInfo.GetVoxelType ().VTransparency == Transparency.solid 
	    && voxelInfo.chunk.GetVoxel(indexBelow) == 2) {	 	    
			voxelInfo.chunk.SetVoxel(indexBelow, 1, true);
		}	
		
	}
}

}