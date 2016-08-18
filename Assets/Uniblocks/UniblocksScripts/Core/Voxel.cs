using UnityEngine;

namespace Uniblocks
{
    public class Voxel : MonoBehaviour
    {
        public string VName;
        public Mesh VMesh;
        public bool VCustomMesh;
        public bool VCustomSides;
        public Vector2[] VTexture; // index of the texture. Array index specifies face (VTexture[0] is the up-facing texture, for example)
        public Transparency VTransparency;
        public ColliderType VColliderType;
        public int VSubmeshIndex;
        public MeshRotation VRotation;


        public static void DestroyBlock(VoxelInfo voxelInfo)
        {
            // multiplayer - send change to server
            if (Engine.EnableMultiplayer)
            {
                Engine.UniblocksNetwork.GetComponent<UniblocksClient>().SendPlaceBlock(voxelInfo, 0);
            }

            // single player - apply change locally
            else
            {
                GameObject obj = Instantiate(Engine.GetVoxelGameObject(voxelInfo.GetVoxel())) as GameObject;

                OnBlockDestroy(voxelInfo, obj);

                RegisterWithChunk(voxelInfo, 0);
                Destroy(obj);
            }
        }
        static void OnBlockDestroy(VoxelInfo voxelInfo, GameObject voxelObject)
        {
            VoxelEvents ev = voxelObject.GetComponent<VoxelEvents>();
            if (ev != null)
            {
                ev.OnBlockDestroy(voxelInfo);
            }
        }

        public static void PlaceBlock(VoxelInfo voxelInfo, ushort data)
        {
            // multiplayer - send change to server
            if (Engine.EnableMultiplayer)
            {
                Engine.UniblocksNetwork.GetComponent<UniblocksClient>().SendPlaceBlock(voxelInfo, data);
            }

            // single player - apply change locally
            else
            {
                RegisterWithChunk(voxelInfo, data);

                GameObject obj = Instantiate(Engine.GetVoxelGameObject(data)) as GameObject;
                OnBlockPlace(voxelInfo, obj);

                Destroy(obj);
            }
        }
        static void OnBlockPlace(VoxelInfo voxelInfo, GameObject voxelObject)
        {
            VoxelEvents ev = voxelObject.GetComponent<VoxelEvents>();
            if (ev != null)
            {
                ev.OnBlockPlace(voxelInfo);
            }
        }

        public static void ChangeBlock(VoxelInfo voxelInfo, ushort data)
        {
            // multiplayer - send change to server
            if (Engine.EnableMultiplayer)
            {
                Engine.UniblocksNetwork.GetComponent<UniblocksClient>().SendChangeBlock(voxelInfo, data);
            }

            // single player - apply change locally
            else
            {
                RegisterWithChunk(voxelInfo, data);

                GameObject voxelObject = Instantiate(Engine.GetVoxelGameObject(data)) as GameObject;
                if (voxelObject.GetComponent<VoxelEvents>() != null)
                {
                    voxelObject.GetComponent<VoxelEvents>().OnBlockChange(voxelInfo);
                }
                Destroy(voxelObject);
            }
        }


        #region Multiplayer

        public static void DestroyBlockMultiplayer(VoxelInfo voxelInfo, NetworkPlayer sender)
        { 
            // received from server, don't use directly
            GameObject obj = Instantiate(Engine.GetVoxelGameObject(voxelInfo.GetVoxel())) as GameObject;

            OnBlockDestroyMultiplayer(voxelInfo, obj, sender);

            RegisterWithChunk(voxelInfo, 0);
            Destroy(obj);
        }
        static void OnBlockDestroyMultiplayer(VoxelInfo voxelInfo, GameObject voxelObject, NetworkPlayer sender)
        {
            VoxelEvents ev = voxelObject.GetComponent<VoxelEvents>();
            if (ev != null)
            {
                ev.OnBlockDestroy(voxelInfo);
                ev.OnBlockDestroyMultiplayer(voxelInfo, sender);
            }
        }

        public static void PlaceBlockMultiplayer(VoxelInfo voxelInfo, ushort data, NetworkPlayer sender)
        {
            // received from server, don't use directly
            RegisterWithChunk(voxelInfo, data);

            GameObject obj = Instantiate(Engine.GetVoxelGameObject(data)) as GameObject;
            OnBlockPlaceMultiplayer(voxelInfo, obj, sender);

            Destroy(obj);
        }
        static void OnBlockPlaceMultiplayer(VoxelInfo voxelInfo, GameObject voxelObject, NetworkPlayer sender)
        {
            VoxelEvents ev = voxelObject.GetComponent<VoxelEvents>();
            if (ev != null)
            {
                ev.OnBlockPlace(voxelInfo);
                ev.OnBlockPlaceMultiplayer(voxelInfo, sender);
            }
        }

        public static void ChangeBlockMultiplayer(VoxelInfo voxelInfo, ushort data, NetworkPlayer sender)
        { 
            // received from server, don't use directly
            RegisterWithChunk(voxelInfo, data);

            GameObject voxelObject = Instantiate(Engine.GetVoxelGameObject(data)) as GameObject;
            VoxelEvents events = voxelObject.GetComponent<VoxelEvents>();
            if (events != null)
            {
                events.OnBlockChange(voxelInfo);
                events.OnBlockChangeMultiplayer(voxelInfo, sender);
            }
            Destroy(voxelObject);
        }

        #endregion Multiplayer


        static void RegisterWithChunk(VoxelInfo voxelInfo, ushort data)
        {
            voxelInfo.chunk.SetVoxel(voxelInfo.index, data, true);
        }


        // Block Editor functions

        public ushort GetID()
        {
            return ushort.Parse(gameObject.name.Split('_')[1]);
        }
        public void SetID(ushort id)
        {
            gameObject.name = "block_" + id.ToString();
        }
    }
}