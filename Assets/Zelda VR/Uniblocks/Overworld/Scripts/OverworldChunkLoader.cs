using Uniblocks;

public class OverworldChunkLoader : ChunkLoader
{
    public void DoSpawnChunks()
    {
        if (!Engine.Initialized || !ChunkManager.Initialized)
        {
            return;
        }

        Index pos = Engine.PositionToChunkIndex(transform.position);
        ChunkManager.SpawnChunks(pos.x, pos.y, pos.z);
    }
}