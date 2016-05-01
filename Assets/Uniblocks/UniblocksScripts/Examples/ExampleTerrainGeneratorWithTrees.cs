using UnityEngine;
using System.Collections;

namespace Uniblocks {

public class ExampleTerrainGeneratorWithTrees : TerrainGenerator {

	public override void GenerateVoxelData () {
	
		int chunky = chunk.ChunkIndex.y;
		int SideLength = Engine.ChunkSideLength;
		
		for (int x=0; x<SideLength; x++) {
			for (int y=0; y<SideLength; y++) {
				for (int z=0; z<SideLength; z++) { // for all voxels in the chunk
					
					Vector3 voxelPos = chunk.VoxelIndexToPosition(x,y,z); // get absolute position for the voxel
					voxelPos = new Vector3 (voxelPos.x+seed, voxelPos.y, voxelPos.z+seed); // offset by seed
					
					float perlin1 = Mathf.PerlinNoise( voxelPos.x * 0.010f, voxelPos.z * 0.010f ) * 70.1f; // major (mountains & big hills)
					float perlin2 = Mathf.PerlinNoise( voxelPos.x * 0.085f, voxelPos.z * 0.085f ) * 9.1f; // minor (fine detail)
					
					
					int currentHeight = y+(SideLength*chunky); // get absolute height for the voxel
					bool setToGrass = false;
				
					// grass pass
					if (perlin1 > currentHeight) {
						if (perlin1 > perlin2 + currentHeight) {
							chunk.SetVoxelSimple(x,y,z, 2);	// set grass
							setToGrass = true;
						}
					}
				
					// dirt pass
					currentHeight = currentHeight + 1; // offset dirt by 1 (since we want grass 1 block higher)
					if (perlin1 > currentHeight) {
						if (perlin1 > perlin2 + currentHeight) {
							chunk.SetVoxelSimple(x,y,z, 1); // set dirt
							setToGrass = false;
						}
					}
					
					// tree pass
					if (setToGrass && TreeCanFit(x,y,z)) { // only add a tree if the current block has been set to grass and if there is room for the tree in the chunk
						if (Random.Range (0.0f, 1.0f) < 0.01f) { // 1% chance to add a tree
							AddTree (x,y+1,z);
						}
					}
					
				}
			}
		}
	}
	
	
	bool TreeCanFit ( int x, int y, int z ) {
		if (x > 0 && x < Engine.ChunkSideLength-1 && z > 0 && z < Engine.ChunkSideLength-1 && y+5 < Engine.ChunkSideLength) {
			return true;
		}
		else {
			return false;
		}
	}
	
	void AddTree ( int x, int y, int z ) {
		
		// first, create a trunk
		for (int trunkHeight = 0; trunkHeight < 4; trunkHeight++) {
			chunk.SetVoxelSimple (x, y + trunkHeight, z, 6); // set wood at y from 0 to 4
		}
		
		
		// then create leaves around the top
		for (int offsetY = 2; offsetY < 4; offsetY++) // leaves should start from y=2 (vertical coordinate)
		{
			for (int offsetX = -1; offsetX <= 1; offsetX++) {
				for (int offsetZ = -1; offsetZ <= 1; offsetZ++) {
					if ( (offsetX == 0 && offsetZ == 0) == false) {
						chunk.SetVoxelSimple (x + offsetX, y + offsetY, z + offsetZ, 9);
					}
				}
			}
		}
		
		// add one more leaf block on top
		chunk.SetVoxel (x, y+4, z, 9, false);
	}
}

}