namespace Uniblocks
{
    public class FlatTerrainGenerator : TerrainGenerator
    { // generates a flat terrain
        public override void GenerateVoxelData()
        {
            int chunkSizeX = Engine.chunkSizeX;
            int chunkSizeY = Engine.chunkSizeY;
            int chunkSizeZ = Engine.chunkSizeZ;

            int chunky = chunk.chunkIndex.y;

            for (int x = 0; x < chunkSizeX; x++)
            {
                for (int y = 0; y < chunkSizeY; y++)
                {
                    for (int z = 0; z < chunkSizeZ; z++)
                    { // for all voxels in the chunk
                        int currentHeight = y + (chunkSizeY * chunky); // get absolute height for the voxel

                        if (currentHeight < 8)
                        {
                            chunk.SetVoxelSimple(x, y, z, 1); // set dirt
                        }
                    }
                }
            }
        }
    }
}