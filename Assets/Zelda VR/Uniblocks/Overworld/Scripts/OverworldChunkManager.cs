using Uniblocks;

public class OverworldChunkManager : ChunkManager
{
    /*void OnEnabled()
    {
    }*/
    void OnDisabled()
    {
        StopAllCoroutines();
    }
}