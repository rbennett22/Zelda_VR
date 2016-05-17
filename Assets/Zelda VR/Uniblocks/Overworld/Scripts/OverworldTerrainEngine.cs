public class OverworldTerrainEngine : Uniblocks.Engine
{
    public TileMap TileMap { get { return FindObjectOfType<TileMap>(); } }
    public OverworldChunkLoader ChunkLoader { get { return FindObjectOfType<OverworldChunkLoader>(); } }


    void Start()
    {
        RefreshActiveStatus();
    }
    void OnLevelWasLoaded(int level)
    {
        RefreshActiveStatus();
    }

    void RefreshActiveStatus()
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
}