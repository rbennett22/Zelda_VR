using UnityEngine;
using System.Collections;
namespace Uniblocks {
	

public class VoxelDoorOpenClose : DefaultVoxelEvents {

	public override void OnMouseDown (int mouseButton, VoxelInfo voxelInfo) {
	
		if (mouseButton == 0) {
			Voxel.DestroyBlock (voxelInfo);	// destroy with left click
		}
		
		else if (mouseButton == 1) { // open/close with right click
		
			if (voxelInfo.GetVoxel() == 70) { // if open door
				Voxel.ChangeBlock (voxelInfo, 7); // set to closed
			}
			
			else if (voxelInfo.GetVoxel() == 7) { // if closed door
				Voxel.ChangeBlock (voxelInfo, 70); // set to open
			}	

		}
	}

}

}