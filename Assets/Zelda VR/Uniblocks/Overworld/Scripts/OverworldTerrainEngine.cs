using UnityEngine;
using UnityEngine.SceneManagement;
using Uniblocks;
using System.Collections.Generic;
using Immersio.Utility;

public class OverworldTerrainEngine : Engine
{
    public static OverworldTerrainEngine Instance { get { return EngineInstance as OverworldTerrainEngine; } }


    public TileMap TileMap { get { return FindObjectOfType<TileMap>(); } }
    public OverworldChunkLoader ChunkLoader { get { return FindObjectOfType<OverworldChunkLoader>(); } }

    public Chunk GetChunkForSector(Index2 sector)
    {
        Vector3 pos = TileMap.GetCenterPositionOfSector(sector);
        return PositionToChunk(pos);
    }


    public GameObject GroundPlane { get { return GameObject.FindGameObjectWithTag("GroundPlane"); } }
    public bool GroundPlaneCollisionEnabled
    {
        get { return (GroundPlane == null) ? false : GroundPlane.GetComponent<Collider>().enabled; }
        set
        {
            if (GroundPlane == null) { return; }
            GroundPlane.GetComponent<Collider>().enabled = value;
        }
    }
    public bool GroundPlaneRenderingEnabled
    {
        get { return (GroundPlane == null) ? false : GroundPlane.GetComponent<Renderer>().enabled; }
        set
        {
            if (GroundPlane == null) { return; }
            GroundPlane.GetComponent<Renderer>().enabled = value;
        }
    }


    void Start()
    {
        RefreshActiveStatus();

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RefreshActiveStatus();
    }

    public void RefreshActiveStatus()
    {
        bool doActivate = WorldInfo.Instance.IsOverworld;

        if (ChunkLoader != null)
        {
            ChunkLoader.enabled = doActivate;
            if (doActivate)
            {
                ChunkLoader.DoSpawnChunks();
            }
        }

        ChunkManagerInstance.enabled = doActivate;
    }


    public void ForceRegenerateTerrain(Chunk chunk)
    {
        ForceRegenerateTerrain(new List<Chunk>() { chunk });
    }
    public void ForceRegenerateTerrain(List<Chunk> chunks = null)
    {
        if(!WorldInfo.Instance.IsOverworld)
        {
            return;
        }

        OverworldChunkManager cm = ChunkManagerInstance as OverworldChunkManager;
        cm.ForceRegenerateTerrain(chunks);
    }
}