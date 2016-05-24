using UnityEngine;

namespace Uniblocks
{
    public class TerrainGenerator : MonoBehaviour
    {
        protected Chunk chunk;
        protected int seed;

        public void InitializeGenerator()
        {
            // load seed if it's not loaded yet
            while (Engine.WorldSeed == 0)
            {
                Engine.GetSeed();
            }
            seed = Engine.WorldSeed;

            // get chunk component
            chunk = GetComponent<Chunk>();

            // generate data
            GenerateVoxelData();

            // set empty
            chunk.empty = true;
            foreach (ushort voxel in chunk.voxelData)
            {
                if (voxel != 0)
                {
                    chunk.empty = false;
                    break;
                }
            }

            // flag as done
            chunk.voxelsDone = true;
        }

        public virtual void GenerateVoxelData()
        {
        }
    }
}