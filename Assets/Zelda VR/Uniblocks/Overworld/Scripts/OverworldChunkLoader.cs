using Uniblocks;

public class OverworldChunkLoader : ChunkLoader
{
    public void DoSpawnChunks()
    {
        if (!Engine.Initialized || !ChunkManager.Initialized)
        {
            return;
        }

        ChunkManager.SpawnChunks(transform.position);
    }
}