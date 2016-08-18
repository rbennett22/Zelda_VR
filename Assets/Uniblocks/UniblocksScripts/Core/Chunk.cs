#pragma warning disable 0618 // variable is obsolete

using System.Collections;
using UnityEngine;
using Immersio.Utility;

namespace Uniblocks
{
    public class Chunk : MonoBehaviour
    {
        public ushort[] voxelData; 

        public Index chunkIndex;    // Corresponds to the position of the chunk
        public Chunk[] neighborChunks;
        public bool empty;

        public bool fresh = true;

        public bool enableTimeout;
        public bool disableMesh;    // for chunks spawned from UniblocksServer; if true, the chunk will not build a mesh
        public float lifetime;      // how long since the chunk has been spawned

        // update queue
        public bool flaggedToUpdate;
        public bool inUpdateQueue;
        public bool voxelsDone;     // true when this chunk has finished generating or loading voxel data

        public GameObject meshContainer, chunkCollider;

        public int sizeX, sizeY, sizeZ;
        public Index3 Size {
            get { return new Index3(sizeX, sizeY, sizeZ); }
            set {
                sizeX = value.x;
                sizeY = value.y;
                sizeZ = value.z;
            }
        }


        ChunkMeshCreator MeshCreator;
        bool _flaggedToRemove;


        // ==== maintenance ===========================================================================================

        public void Awake()
        { 
            chunkIndex = new Index(transform.position);

            Size = Engine.ChunkSize;

            neighborChunks = new Chunk[6]; // 0 = up, 1 = down, 2 = right, 3 = left, 4 = forward, 5 = back
            MeshCreator = GetComponent<ChunkMeshCreator>();
            fresh = true;

            // Register chunk
            ChunkManager.RegisterChunk(this);

            // Clear the voxel data
            voxelData = new ushort[sizeX * sizeY * sizeZ];

            // Set actual position
            Vector3 idx = chunkIndex.ToVector3();
            Vector3 s = transform.localScale;
            transform.position = new Vector3(
                idx.x * sizeX * s.x, 
                idx.y * sizeY * s.y, 
                idx.z * sizeZ * s.z);

            // Grab voxel data
            if (Engine.EnableMultiplayer && !Network.isServer)
            {
                StartCoroutine(RequestVoxelData()); // if multiplayer, get data from server
            }
            else if (Engine.SaveVoxelData && TryLoadVoxelData())
            {
                // data is loaded through TryLoadVoxelData()
            }
            else
            {
                GenerateVoxelData();
            }
        }

        public bool TryLoadVoxelData()
        { 
            // returns true if data was loaded successfully, false if data was not found
            return GetComponent<ChunkDataFiles>().LoadData();
        }

        public void GenerateVoxelData()
        {
            GetComponent<TerrainGenerator>().InitializeGenerator();
        }

        public void AddToQueueWhenReady()
        { 
            // adds chunk to the UpdateQueue when this chunk and all known neighbors have their data ready
            StartCoroutine(DoAddToQueueWhenReady());
        }
        IEnumerator DoAddToQueueWhenReady()
        {
            while (voxelsDone == false || AllNeighborsHaveData() == false)
            {
                if (ChunkManager.StopSpawning)
                { 
                    // interrupt if the chunk spawn sequence is stopped. This will be restarted in the correct order from ChunkManager
                    yield break;
                }
                yield return new WaitForEndOfFrame();
            }
            ChunkManager.AddChunkToUpdateQueue(this);
        }

        bool AllNeighborsHaveData()
        {
            // returns false if at least one neighbor is known but doesn't have data ready yet
            foreach (Chunk neighbor in neighborChunks)
            {
                if (neighbor != null)
                {
                    if (neighbor.voxelsDone == false)
                    {
                        return false;
                    }
                }
            }
            return true;
        }


        void OnDestroy()
        {
            ChunkManager.UnregisterChunk(this);
        }


        // ==== data =======================================================================================

        public void ClearVoxelData()
        {
            voxelData = new ushort[sizeX * sizeY * sizeZ];
        }

        public int GetDataLength()
        {
            return voxelData.Length;
        }


        int GetRawIndex(int x, int y, int z) { return (z * sizeY * sizeX) + (y * sizeX) + x; }
        int GetRawIndex(Index index) { return GetRawIndex(index.x, index.y, index.z); }


        // == set voxel

        public void SetVoxelSimple(int rawIndex, ushort data)
        {
            voxelData[rawIndex] = data;
        }
        public void SetVoxelSimple(int x, int y, int z, ushort data)
        {
            SetVoxelSimple(GetRawIndex(x, y, z), data);
        }
        public void SetVoxelSimple(Index index, ushort data)
        {
            SetVoxelSimple(GetRawIndex(index), data);
        }

        public void SetVoxel(int x, int y, int z, ushort data, bool updateMesh)
        {
            // If outside of this chunk, change in neighbor instead (if possible)
            if (x < 0)
            {
                SetVoxelForNeighbor(Direction.left, x + sizeX, y, z, data, updateMesh);
            }
            else if (x >= sizeX)
            {
                SetVoxelForNeighbor(Direction.right, x - sizeX, y, z, data, updateMesh);
            }
            else if (y < 0)
            {
                SetVoxelForNeighbor(Direction.down, x, y + sizeY, z, data, updateMesh);
            }
            else if (y >= sizeY)
            {
                SetVoxelForNeighbor(Direction.down, x, y - sizeY, z, data, updateMesh);
            }
            else if (z < 0)
            {
                SetVoxelForNeighbor(Direction.back, x, y, z + sizeZ, data, updateMesh);
            }
            else if (z >= sizeZ)
            {
                SetVoxelForNeighbor(Direction.forward, x, y, z - sizeZ, data, updateMesh);
            }
            else
            {
                SetVoxelSimple(x, y, z, data);

                if (updateMesh)
                {
                    UpdateNeighborsIfNeeded(x, y, z);
                    FlagToUpdate();
                }
            }
        }
        public void SetVoxel(Index index, ushort data, bool updateMesh)
        {
            SetVoxel(index.x, index.y, index.z, data, updateMesh);
        }

        void SetVoxelForNeighbor(Direction dir, int x, int y, int z, ushort data, bool updateMesh)
        {
            Chunk n = neighborChunks[(int)dir];
            if (n != null)
            {
                n.SetVoxel(x, y, z, data, updateMesh);
            }
        }


        // == get voxel

        public ushort GetVoxelSimple(int rawIndex)
        {
            return voxelData[rawIndex];
        }
        public ushort GetVoxelSimple(int x, int y, int z)
        {
            return GetVoxelSimple(GetRawIndex(x, y, z));
        }
        public ushort GetVoxelSimple(Index index)
        {
            return GetVoxelSimple(GetRawIndex(index));
        }

        public ushort GetVoxel(int x, int y, int z)
        {
            if (x < 0)
            {
                return GetVoxelForNeighbor(Direction.left, x + sizeX, y, z);
            }
            else if (x >= sizeX)
            {
                return GetVoxelForNeighbor(Direction.right, x - sizeX, y, z);
            }
            else if (y < 0)
            {
                return GetVoxelForNeighbor(Direction.down, x, y + sizeY, z);
            }
            else if (y >= sizeY)
            {
                return GetVoxelForNeighbor(Direction.up, x, y - sizeY, z);
            }
            else if (z < 0)
            {
                return GetVoxelForNeighbor(Direction.back, x, y, z + sizeZ);
            }
            else if (z >= sizeZ)
            {
                return GetVoxelForNeighbor(Direction.forward, x, y, z - sizeZ);
            }
            
            return GetVoxelSimple(x, y, z);
        }
        public ushort GetVoxel(Index index)
        {
            return GetVoxel(index.x, index.y, index.z);
        }

        ushort GetVoxelForNeighbor(Direction dir, int x, int y, int z)
        {
            Chunk n = neighborChunks[(int)dir];
            if (n == null)
            {
                return ushort.MaxValue;
            }
            return n.GetVoxel(x, y, z);
        }


        // ==== Flags =======================================================================================

        public void FlagToRemove()
        {
            _flaggedToRemove = true;
        }
        public void FlagToUpdate()
        {
            flaggedToUpdate = true;
        }


        // ==== Update ====

        public void Update()
        {
            ChunkManager.SavesThisFrame = 0;
        }

        public void LateUpdate()
        {
            // timeout
            if (Engine.EnableChunkTimeout && enableTimeout)
            {
                lifetime += Time.deltaTime;
                if (lifetime > Engine.ChunkTimeout)
                {
                    _flaggedToRemove = true;
                }
            }

            // Update the mesh?
            if (flaggedToUpdate && voxelsDone && !disableMesh && Engine.GenerateMeshes)
            { 
                flaggedToUpdate = false;
                RebuildMesh();
            }

            if (_flaggedToRemove)
            {
                if (Engine.SaveVoxelData)
                { 
                    // save data over time, destroy chunk when done
                    if (ChunkDataFiles.SavingChunks == false)
                    { 
                        // only destroy chunks if they are not being saved currently
                        if (ChunkManager.SavesThisFrame < Engine.MaxChunkSaves)
                        {
                            ChunkManager.SavesThisFrame++;
                            SaveData();
                            Destroy(this.gameObject);
                        }
                    }
                }
                else
                { 
                    // if saving is disabled, destroy immediately
                    Destroy(this.gameObject);
                }
            }
        }

        public void RebuildMesh()
        {
            MeshCreator.RebuildMesh();
            ConnectNeighbors();
        }


        void SaveData()
        {
            if (Engine.SaveVoxelData == false)
            {
                Debug.LogWarning("Uniblocks: Saving is disabled. You can enable it in the Engine Settings.");
                return;
            }

            if (Application.isWebPlayer == false)
            {
                GetComponent<ChunkDataFiles>().SaveData();
            }
        }


        // ==== Neighbors =======================================================================================

        public void ConnectNeighbors()
        { 
            // update the mesh on all neighbors that have a mesh but don't know about this chunk yet, and also pass them the reference to this chunk
            int loop = 0;
            int i = loop;

            while (loop < 6)
            {
                // for even indexes, add one; for odd, subtract one (because the neighbors are in opposite direction to this chunk)
                i = (loop % 2 == 0) ? (loop + 1) : (loop - 1);

                Chunk ch = neighborChunks[loop];
                if (ch != null && ch.gameObject.GetComponent<MeshFilter>().sharedMesh != null)
                {
                    if (ch.neighborChunks[i] == null)
                    {
                        ch.AddToQueueWhenReady();
                        ch.neighborChunks[i] = this;
                    }
                }

                loop++;
            }
        }

        public void AssignNeighbors()
        { 
            // assign the neighbor chunk gameobjects to the NeighborChunks array
            int x = chunkIndex.x;
            int y = chunkIndex.y;
            int z = chunkIndex.z;

            if (neighborChunks[0] == null) neighborChunks[0] = ChunkManager.GetChunkComponent(x, y + 1, z);
            if (neighborChunks[1] == null) neighborChunks[1] = ChunkManager.GetChunkComponent(x, y - 1, z);
            if (neighborChunks[2] == null) neighborChunks[2] = ChunkManager.GetChunkComponent(x + 1, y, z);
            if (neighborChunks[3] == null) neighborChunks[3] = ChunkManager.GetChunkComponent(x - 1, y, z);
            if (neighborChunks[4] == null) neighborChunks[4] = ChunkManager.GetChunkComponent(x, y, z + 1);
            if (neighborChunks[5] == null) neighborChunks[5] = ChunkManager.GetChunkComponent(x, y, z - 1);
        }

        public Chunk GetChunkDirectlyBelowThisChunk()       // TODO
        {
            if (neighborChunks == null || neighborChunks.Length == 0)
            {
                return null;
            }
            return neighborChunks[1];
        }

        public Index GetAdjacentIndex(Index index, Direction direction)
        {
            return GetAdjacentIndex(index.x, index.y, index.z, direction);
        }

        public Index GetAdjacentIndex(int x, int y, int z, Direction direction)
        { 
            // converts x,y,z, direction into a specific index
            if (direction == Direction.down) return new Index(x, y - 1, z);
            else if (direction == Direction.up) return new Index(x, y + 1, z);
            else if (direction == Direction.left) return new Index(x - 1, y, z);
            else if (direction == Direction.right) return new Index(x + 1, y, z);
            else if (direction == Direction.back) return new Index(x, y, z - 1);
            else if (direction == Direction.forward) return new Index(x, y, z + 1);
            else
            {
                Debug.LogError("Chunk.GetAdjacentIndex failed! Returning default index.");
                return new Index(x, y, z);
            }
        }

        public void UpdateNeighborsIfNeeded(int x, int y, int z)
        { 
            // if the index lies at the border of a chunk, FlagToUpdate the neighbor at that border
            if (x == 0)
            {
                FlagNeighborToUpdate(Direction.left);
            }
            else if (x == sizeX - 1)
            {
                FlagNeighborToUpdate(Direction.right);
            }

            if (y == 0)
            {
                FlagNeighborToUpdate(Direction.down);
            }
            else if (y == sizeY - 1)
            {
                FlagNeighborToUpdate(Direction.up);
            }

            if (z == 0)
            {
                FlagNeighborToUpdate(Direction.back);
            }
            else if (z == sizeZ - 1)
            {
                FlagNeighborToUpdate(Direction.forward);
            }
        }
        void FlagNeighborToUpdate(Direction dir)
        {
            Chunk n = neighborChunks[(int)dir];
            if (n != null)
            {
                n.FlagToUpdate();
            }
        }


        // ==== position / voxel index =======================================================================================

        public Index PositionToVoxelIndex(Vector3 position)
        {
            Vector3 point = transform.InverseTransformPoint(position);

            // round it to get an int which we can convert to the voxel index
            Index index = new Index(0, 0, 0);
            index.x = Mathf.RoundToInt(point.x);
            index.y = Mathf.RoundToInt(point.y);
            index.z = Mathf.RoundToInt(point.z);

            return index;
        }

        public Vector3 VoxelIndexToPosition(Index index)
        {
            Vector3 localPoint = index.ToVector3();         // convert index to chunk's local position
            return transform.TransformPoint(localPoint);    // convert local position to world space
        }

        public Vector3 VoxelIndexToPosition(int x, int y, int z)
        {
            Vector3 localPoint = new Vector3(x, y, z);      // convert index to chunk's local positio
            return transform.TransformPoint(localPoint);    // convert local position to world space
        }

        public Index PositionToVoxelIndex(Vector3 position, Vector3 normal, bool returnAdjacent)
        {
            // converts the absolute position to the index of the voxel
            Vector3 offset = (normal * 0.25f);
            int sign = returnAdjacent ? 1 : -1;     // push the hit point outside of the cube or into the cube?
            position += sign * offset;

            // convert world position to chunk's local position
            Vector3 point = transform.InverseTransformPoint(position);

            // round it to get an int which we can convert to the voxel index
            Index index = new Index(0, 0, 0);
            index.x = Mathf.RoundToInt(point.x);
            index.y = Mathf.RoundToInt(point.y);
            index.z = Mathf.RoundToInt(point.z);

            return index;
        }


        // ==== network ==============

        // How many chunk requests are currently queued in the server for this client. 
        // Increased by 1 every time a chunk requests data, and reduced by 1 when a chunk receives data.
        public static int CurrentChunkDataRequests; 

        IEnumerator RequestVoxelData()
        { 
            // waits until we're connected to a server and then sends a request for voxel data for this chunk to the server
            while (!Network.isClient)
            {
                CurrentChunkDataRequests = 0;   // reset the counter if we're not connected
                yield return new WaitForEndOfFrame();
            }
            while (Engine.MaxChunkDataRequests != 0 && CurrentChunkDataRequests >= Engine.MaxChunkDataRequests)
            {
                yield return new WaitForEndOfFrame();
            }

            CurrentChunkDataRequests++;
            Engine.UniblocksNetwork.GetComponent<NetworkView>().RPC("SendVoxelData", RPCMode.Server, Network.player, chunkIndex.x, chunkIndex.y, chunkIndex.z);
        }
    }
}