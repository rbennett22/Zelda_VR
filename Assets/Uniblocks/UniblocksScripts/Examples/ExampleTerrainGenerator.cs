using UnityEngine;
using System.Collections;

namespace Uniblocks {

public class ExampleTerrainGenerator : TerrainGenerator {

	public override void GenerateVoxelData () {
	
		int chunky = chunk.ChunkIndex.y;
		int SideLength = Engine.ChunkSideLength;
		
		// debug
		int random = Random.Range(0,10);
		
		for (int x=0; x<SideLength; x++) {
			for (int y=0; y<SideLength; y++) {
				for (int z=0; z<SideLength; z++) { // for all voxels in the chunk
					
					Vector3 voxelPos = chunk.VoxelIndexToPosition(x,y,z); // get absolute position for the voxel
					voxelPos = new Vector3 (voxelPos.x+seed, voxelPos.y, voxelPos.z+seed); // offset by seed
					
					float perlin1 = Mathf.PerlinNoise( voxelPos.x * 0.010f, voxelPos.z * 0.010f ) * 70.1f; // major (mountains & big hills)
					float perlin2 = Mathf.PerlinNoise( voxelPos.x * 0.085f, voxelPos.z * 0.085f ) * 9.1f; // minor (fine detail)
					
					
					int currentHeight = y+(SideLength*chunky); // get absolute height for the voxel
				
					// grass pass
					if (perlin1 > currentHeight) {
						if (perlin1 > perlin2 + currentHeight) {
							chunk.SetVoxelSimple(x,y,z, 2);	// set grass

						}
					}
				
					// dirt pass
					currentHeight = currentHeight + 1; // offset dirt by 1 (since we want grass 1 block higher)
					if (perlin1 > currentHeight) {
						if (perlin1 > perlin2 + currentHeight) {
							chunk.SetVoxelSimple(x,y,z, 1); // set dirt
						}
					}
					
					// debug
					if (random == 1) {
						//chunk.SetVoxelSimple(x,y,z, 3); // set stone or whatever
					}

				}
			}
		}
	}
}

}