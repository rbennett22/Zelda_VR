using UnityEngine;
using Uniblocks;

public class TerrainGeneratorTest : TerrainGenerator
{ 
	public override void GenerateVoxelData ()
    {
		int chunkY = chunk.ChunkIndex.y;
		int SideLength = Engine.ChunkSideLength;
		
		for (int x = 0; x < SideLength; x++)
        {
			for (int y = 0; y < SideLength; y++)
            {
				for (int z = 0; z < SideLength; z++)
                {
                    int currentHeight = y + (SideLength * chunkY); // get absolute height for the voxel
					if (currentHeight < 8)
                    {
                        ushort n = (ushort)((z % 5) + 1);
                        if (n == 5) { n = 18; }
                        chunk.SetVoxelSimple(x, y, z, n);
					}
                }
			}
		}
	}
}