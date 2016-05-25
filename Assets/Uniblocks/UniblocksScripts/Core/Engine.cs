using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Immersio.Utility;

namespace Uniblocks
{
    // enums
    public enum Facing
    {
        up, down, right, left, forward, back
    }

    public enum Direction
    {
        up, down, right, left, forward, back
    }

    public enum Transparency
    {
        solid, semiTransparent, transparent
    }

    public enum ColliderType
    {
        cube, mesh, none
    }


    public class Engine : MonoBehaviour
    {
        // file paths
        public static string WorldName, WorldPath, BlocksPath;

        public static int WorldSeed;
        public string lWorldName = "TestWorld";
        public string lBlocksPath;

        // voxels
        public static GameObject[] Blocks;

        public GameObject[] lBlocks;

        // chunk spawn settings
        public static int HeightRange, ChunkSpawnDistance, ChunkDespawnDistance;
        public int lHeightRange, lChunkSpawnDistance, lChunkDespawnDistance;

        public static Index3 ChunkSize;
        public int lChunkSizeX, lChunkSizeY, lChunkSizeZ;

        // texture settings
        public static float TextureUnit, TexturePadding;

        public float lTextureUnit, lTexturePadding;

        // performance settings
        public static int TargetFPS, MaxChunkSaves, MaxChunkDataRequests;

        public int lTargetFPS, lMaxChunkSaves, lMaxChunkDataRequests;

        // global settings

        public static bool ShowBorderFaces, GenerateColliders, SendCameraLookEvents,
        SendCursorEvents, EnableMultiplayer, MultiplayerTrackPosition, SaveVoxelData, GenerateMeshes;

        public bool lShowBorderFaces, lGenerateColliders, lSendCameraLookEvents,
        lSendCursorEvents, lEnableMultiplayer, lMultiplayerTrackPosition, lSaveVoxelData, lGenerateMeshes;

        public static float ChunkTimeout;
        public float lChunkTimeout;
        public static bool EnableChunkTimeout;

        // other
        public static int SquaredSideLength;

        public static GameObject UniblocksNetwork;
        public static Engine EngineInstance;
        public static ChunkManager ChunkManagerInstance;

        public static Vector3 ChunkScale;

        public static bool Initialized;



        // ==== initialization ====
        public void Awake()
        {
            EngineInstance = this;
            ChunkManagerInstance = GetComponent<ChunkManager>();

            WorldName = lWorldName;
            UpdateWorldPath();

            BlocksPath = lBlocksPath;
            Blocks = lBlocks;

            TargetFPS = lTargetFPS;
            MaxChunkSaves = lMaxChunkSaves;
            MaxChunkDataRequests = lMaxChunkDataRequests;

            TextureUnit = lTextureUnit;
            TexturePadding = lTexturePadding;
            GenerateColliders = lGenerateColliders;
            ShowBorderFaces = lShowBorderFaces;
            EnableMultiplayer = lEnableMultiplayer;
            MultiplayerTrackPosition = lMultiplayerTrackPosition;
            SaveVoxelData = lSaveVoxelData;
            GenerateMeshes = lGenerateMeshes;

            ChunkSpawnDistance = lChunkSpawnDistance;
            HeightRange = lHeightRange;
            ChunkDespawnDistance = lChunkDespawnDistance;

            SendCameraLookEvents = lSendCameraLookEvents;
            SendCursorEvents = lSendCursorEvents;

            ChunkSize = new Index3(lChunkSizeX, lChunkSizeY, lChunkSizeZ);

            ChunkDataFiles.LoadedRegions = new Dictionary<string, string[]>();
            ChunkDataFiles.TempChunkData = new Dictionary<string, string>();

            if (lChunkTimeout <= 0.00001f)
            {
                EnableChunkTimeout = false;
            }
            else {
                EnableChunkTimeout = true;
                ChunkTimeout = lChunkTimeout;
            }

            if (Application.isWebPlayer)
            {
                lSaveVoxelData = false;
                SaveVoxelData = false;
            }


            // set layer
            if (LayerMask.LayerToName(26) != "" && LayerMask.LayerToName(26) != "UniblocksNoCollide")
            {
                Debug.LogWarning("Uniblocks: Layer 26 is reserved for Uniblocks; it is automatically set to ignore collision with all layers.");
            }
            for (int i = 0; i < 31; i++)
            {
                Physics.IgnoreLayerCollision(i, 26);
            }


            // check block array
            if (Blocks.Length < 1)
            {
                Debug.LogError("Uniblocks: The blocks array is empty! Use the Block Editor to update the blocks array.");
                Debug.Break();
            }

            if (Blocks[0] == null)
            {
                Debug.LogError("Uniblocks: Cannot find the empty block prefab (id 0)!");
                Debug.Break();
            }
            else if (Blocks[0].GetComponent<Voxel>() == null)
            {
                Debug.LogError("Uniblocks: Voxel id 0 does not have the Voxel component attached!");
                Debug.Break();
            }

            // check settings
            if (ChunkSize.x < 1 || ChunkSize.y < 1 || ChunkSize.z < 1)
            {
                Debug.LogError("Uniblocks: Chunk side length must be greater than 0!");
                Debug.Break();
            }

            if (ChunkSpawnDistance < 1)
            {
                ChunkSpawnDistance = 0;
                Debug.LogWarning("Uniblocks: Chunk spawn distance is 0. No chunks will spawn!");
            }

            if (HeightRange < 0)
            {
                HeightRange = 0;
                Debug.LogWarning("Uniblocks: Chunk height range can't be a negative number! Setting chunk height range to 0.");
            }

            if (MaxChunkDataRequests < 0)
            {
                MaxChunkDataRequests = 0;
                Debug.LogWarning("Uniblocks: Max chunk data requests can't be a negative number! Setting max chunk data requests to 0.");
            }

            // check materials
            GameObject chunkPrefab = GetComponent<ChunkManager>().ChunkObject;
            int materialCount = chunkPrefab.GetComponent<Renderer>().sharedMaterials.Length - 1;

            for (ushort i = 0; i < Blocks.Length; i++)
            {
                if (Blocks[i] != null)
                {
                    Voxel voxel = Blocks[i].GetComponent<Voxel>();

                    if (voxel.VSubmeshIndex < 0)
                    {
                        Debug.LogError("Uniblocks: Voxel " + i + " has a material index lower than 0! Material index must be 0 or greater.");
                        Debug.Break();
                    }

                    if (voxel.VSubmeshIndex > materialCount)
                    {
                        Debug.LogError("Uniblocks: Voxel " + i + " uses material index " + voxel.VSubmeshIndex + ", but the chunk prefab only has " + (materialCount + 1) + " material(s) attached. Set a lower material index or attach more materials to the chunk prefab.");
                        Debug.Break();
                    }
                }
            }

            // check anti-aliasing
            if (QualitySettings.antiAliasing > 0)
            {
                Debug.LogWarning("Uniblocks: Anti-aliasing is enabled. This may cause seam lines to appear between blocks. If you see lines between blocks, try disabling anti-aliasing, switching to deferred rendering path, or adding some texture padding in the engine settings.");
            }


            Initialized = true;
        }

        // ==== world data ====

        private static void UpdateWorldPath()
        {
            WorldPath = Application.dataPath + "/../Worlds/" + WorldName + "/"; // you can set World Path here
                                                                                       //WorldPath = "/mnt/sdcard/UniblocksWorlds/" + WorldName + "/"; // example mobile path for Android
        }

        public static void SetWorldName(string worldName)
        {
            WorldName = worldName;
            WorldSeed = 0;
            UpdateWorldPath();
        }

        public static void GetSeed()
        { // reads the world seed from file if it exists, else creates a new seed and saves it to file
            if (Application.isWebPlayer)
            { // don't save to file if webplayer
                WorldSeed = Random.Range(ushort.MinValue, ushort.MaxValue);
                return;
            }

            if (File.Exists(WorldPath + "seed"))
            {
                StreamReader reader = new StreamReader(WorldPath + "seed");
                WorldSeed = int.Parse(reader.ReadToEnd());
                reader.Close();
            }
            else {
                while (WorldSeed == 0)
                {
                    WorldSeed = Random.Range(ushort.MinValue, ushort.MaxValue);
                }
                System.IO.Directory.CreateDirectory(WorldPath);
                StreamWriter writer = new StreamWriter(WorldPath + "seed");
                writer.Write(WorldSeed.ToString());
                writer.Flush();
                writer.Close();
            }
        }

        public static void SaveWorld()
        { // saves the data over multiple frames
            EngineInstance.StartCoroutine(ChunkDataFiles.SaveAllChunks());
        }

        public static void SaveWorldInstant()
        {
            ChunkDataFiles.SaveAllChunksInstant();
        }

        // ==== other ====


        public static GameObject GetVoxelGameObject(ushort voxelId)
        {
            try
            {
                if (voxelId == ushort.MaxValue) voxelId = 0;
                GameObject voxelObject = Blocks[voxelId];
                if (voxelObject.GetComponent<Voxel>() == null)
                {
                    Debug.LogError("Uniblocks: Voxel id " + voxelId + " does not have the Voxel component attached!");
                    return Blocks[0];
                }
                else {
                    return voxelObject;
                }
            }
            catch (System.Exception)
            {
                Debug.LogError("Uniblocks: Invalid voxel id: " + voxelId);
                return Blocks[0];
            }
        }

        public static Voxel GetVoxelType(ushort voxelId)
        {
            try
            {
                if (voxelId == ushort.MaxValue) voxelId = 0;
                Voxel voxel = Blocks[(int)voxelId].GetComponent<Voxel>();
                if (voxel == null)
                {
                    Debug.LogError("Uniblocks: Voxel id " + voxelId + " does not have the Voxel component attached!");
                    return null;
                }
                else {
                    return voxel;
                }
            }
            catch (System.Exception)
            {
                Debug.LogError("Uniblocks: Invalid voxel id: " + voxelId);
                return null;
            }
        }



        // a raycast which returns the index of the hit voxel and the gameobject of the hit chunk
        public static VoxelInfo VoxelRaycast(Vector3 origin, Vector3 direction, float range, bool ignoreTransparent)
        {
            RaycastHit hit = new RaycastHit();

            if (Physics.Raycast(origin, direction, out hit, range))
            {
                if (hit.collider.GetComponent<Chunk>() != null
                    || hit.collider.GetComponent<ChunkExtension>() != null)
                { // check if we're actually hitting a chunk
                    GameObject hitObject = hit.collider.gameObject;

                    if (hitObject.GetComponent<ChunkExtension>() != null)
                    { // if we hit a mesh container instead of a chunk
                        hitObject = hitObject.transform.parent.gameObject; // swap the mesh container for the actual chunk object
                    }

                    Index hitIndex = hitObject.GetComponent<Chunk>().PositionToVoxelIndex(hit.point, hit.normal, false);

                    if (ignoreTransparent)
                    { // punch through transparent voxels by raycasting again when a transparent voxel is hit
                        ushort hitVoxel = hitObject.GetComponent<Chunk>().GetVoxel(hitIndex.x, hitIndex.y, hitIndex.z);
                        if (GetVoxelType(hitVoxel).VTransparency != Transparency.solid)
                        { // if the hit voxel is transparent
                            Vector3 newOrigin = hit.point;
                            newOrigin.y -= 0.5f; // push the new raycast down a bit
                            return VoxelRaycast(newOrigin, Vector3.down, range - hit.distance, true);
                        }
                    }


                    return new VoxelInfo(
                                         hitObject.GetComponent<Chunk>().PositionToVoxelIndex(hit.point, hit.normal, false), // get hit voxel index
                                         hitObject.GetComponent<Chunk>().PositionToVoxelIndex(hit.point, hit.normal, true), // get adjacent voxel index
                                         hitObject.GetComponent<Chunk>()); // get chunk
                }
            }

            // else
            return null;
        }

        public static VoxelInfo VoxelRaycast(Ray ray, float range, bool ignoreTransparent)
        {
            return VoxelRaycast(ray.origin, ray.direction, range, ignoreTransparent);
        }




        public static Index PositionToChunkIndex(Vector3 position)
        {
            Index chunkIndex = new Index(Mathf.RoundToInt(position.x / ChunkScale.x) / ChunkSize.x,
                                          Mathf.RoundToInt(position.y / ChunkScale.y) / ChunkSize.y,
                                          Mathf.RoundToInt(position.z / ChunkScale.z) / ChunkSize.z);
            return chunkIndex;
        }

        public static GameObject PositionToChunk(Vector3 position)
        {
            Index chunkIndex = new Index(Mathf.RoundToInt(position.x / ChunkScale.x) / ChunkSize.x,
                                          Mathf.RoundToInt(position.y / ChunkScale.y) / ChunkSize.y,
                                          Mathf.RoundToInt(position.z / ChunkScale.z) / ChunkSize.z);
            return ChunkManager.GetChunk(chunkIndex);
        }

        public static VoxelInfo PositionToVoxelInfo(Vector3 position)
        {
            GameObject chunkObject = PositionToChunk(position);
            if (chunkObject != null)
            {
                Chunk chunk = chunkObject.GetComponent<Chunk>();
                Index voxelIndex = chunk.PositionToVoxelIndex(position);
                return new VoxelInfo(voxelIndex, chunk);
            }
            else {
                return null;
            }
        }

        public static Vector3 VoxelInfoToPosition(VoxelInfo voxelInfo)
        {
            return voxelInfo.chunk.GetComponent<Chunk>().VoxelIndexToPosition(voxelInfo.index);
        }




        // ==== mesh creator ====

        public static Vector2 GetTextureOffset(ushort voxel, Facing facing)
        {
            Voxel voxelType = GetVoxelType(voxel);
            Vector2[] textureArray = voxelType.VTexture;

            if (textureArray.Length == 0)
            { // in case there are no textures defined, return a default texture
                Debug.LogWarning("Uniblocks: Block " + voxel.ToString() + " has no defined textures! Using default texture.");
                return new Vector2(0, 0);
            }
            else if (voxelType.VCustomSides == false)
            { // if this voxel isn't using custom side textures, return the Up texture.
                return textureArray[0];
            }
            else if ((int)facing > textureArray.Length - 1)
            { // if we're asking for a texture that's not defined, grab the last defined texture instead
                return textureArray[textureArray.Length - 1];
            }
            else {
                return textureArray[(int)facing];
            }
        }
    }
}