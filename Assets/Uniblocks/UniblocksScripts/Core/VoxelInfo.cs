namespace Uniblocks
{
    public class VoxelInfo
    {
        public Index index;
        public Index adjacentIndex;

        public Chunk chunk;


        public VoxelInfo(int setX, int setY, int setZ, Chunk setChunk)
        {
            index.x = setX;
            index.y = setY;
            index.z = setZ;

            chunk = setChunk;
        }

        public VoxelInfo(int setX, int setY, int setZ, int setXa, int setYa, int setZa, Chunk setChunk)
        {
            index.x = setX;
            index.y = setY;
            index.z = setZ;

            adjacentIndex.x = setXa;
            adjacentIndex.y = setYa;
            adjacentIndex.z = setZa;

            chunk = setChunk;
        }

        public VoxelInfo(Index setIndex, Chunk setChunk)
        {
            index = setIndex;

            chunk = setChunk;
        }

        public VoxelInfo(Index setIndex, Index setAdjacentIndex, Chunk setChunk)
        {
            index = setIndex;
            adjacentIndex = setAdjacentIndex;

            chunk = setChunk;
        }


        public ushort GetVoxel()
        {
            return chunk.GetVoxel(index);
        }

        public Voxel GetVoxelType()
        {
            return Engine.GetVoxelType(chunk.GetVoxel(index));
        }

        public ushort GetAdjacentVoxel()
        {
            return chunk.GetVoxel(adjacentIndex);
        }

        public Voxel GetAdjacentVoxelType()
        {
            return Engine.GetVoxelType(chunk.GetVoxel(adjacentIndex));
        }

        public void SetVoxel(ushort data, bool updateMesh)
        {
            chunk.SetVoxel(index, data, updateMesh);
        }
    }
}