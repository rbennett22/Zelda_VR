using UnityEngine;

namespace Uniblocks
{
    public class TerrainGenerator : MonoBehaviour
    {
        protected Chunk _chunk;
        protected int _seed;


        public void InitializeGenerator()
        {
            // load seed if it's not loaded yet
            while (Engine.WorldSeed == 0)
            {
                Engine.GetSeed();
            }
            _seed = Engine.WorldSeed;

            _chunk = GetComponent<Chunk>();

            GenerateVoxelData();

            _chunk.empty = true;
            foreach (ushort voxel in _chunk.voxelData)
            {
                if (voxel != 0)
                {
                    _chunk.empty = false;
                    break;
                }
            }

            _chunk.voxelsDone = true;
        }

        public virtual void GenerateVoxelData() { }
    }
}