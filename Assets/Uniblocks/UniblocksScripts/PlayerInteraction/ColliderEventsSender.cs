using UnityEngine;

namespace Uniblocks
{
    public class ColliderEventsSender : MonoBehaviour
    {
        private Index LastIndex;
        private Chunk LastChunk;

        public void Update()
        {
            Chunk chunk = Engine.PositionToChunk(transform.position);
            if (chunk == null)
            {
                return;
            }

            Index voxelIndex = chunk.PositionToVoxelIndex(transform.position);
            VoxelInfo voxelInfo = new VoxelInfo(voxelIndex, chunk);

            // create a local copy of the collision voxel so we can call functions on it
            GameObject voxelObject = Instantiate(Engine.GetVoxelGameObject(voxelInfo.GetVoxel())) as GameObject;

            VoxelEvents events = voxelObject.GetComponent<VoxelEvents>();
            if (events != null)
            {
                // OnEnter
                if (chunk != LastChunk || voxelIndex.IsEqual(LastIndex) == false)
                {
                    events.OnBlockEnter(gameObject, voxelInfo);
                }
                else // OnStay
                {
                    events.OnBlockStay(gameObject, voxelInfo);
                }
            }

            LastChunk = chunk;
            LastIndex = voxelIndex;

            Destroy(voxelObject);
        }
    }
}