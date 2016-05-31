using Uniblocks;
using System.Linq;
using System.Collections.Generic;

public class OverworldChunkManager : ChunkManager
{
    void OnDisabled()
    {
        StopAllCoroutines();
    }


    public void ForceRegenerateTerrain(List<Chunk> chunks = null)
    {
        if(chunks == null)
        {
            chunks = Chunks.Select(item => item.Value).ToList();
        }

        foreach (Chunk c in chunks)
        {
            if (c == null)
            {
                continue;
            }
            c.GenerateVoxelData();
            c.FlagToUpdate();
        }
    }
}