using UnityEngine;
using Uniblocks;

public class OverworldTerrainGenerator : TerrainGenerator
{
    TileMap _overworldTileMap;


    void Awake()
    {
        InitOverworldTileMap();
    }

    void InitOverworldTileMap()
    {
        _overworldTileMap = (Engine.EngineInstance as OverworldTerrainEngine).TileMap;
        if (_overworldTileMap != null)
        {
            _overworldTileMap.LoadMap();
        }
    }


    public override void GenerateVoxelData ()
    {
        if (_overworldTileMap == null)
        {
            InitOverworldTileMap();
            if (_overworldTileMap == null)
                return;
        }

        int tileMapWidth_WOF = ZeldaVRSettings.Instance.tileMapWidthInTiles_WithoutFiller;

        int SideLength = Engine.ChunkSideLength;

        int chunkXX = SideLength * chunk.ChunkIndex.x;
        int chunkYY = SideLength * chunk.ChunkIndex.y;
        int chunkZZ = SideLength * chunk.ChunkIndex.z;

        for (int x = 0; x < SideLength; x++)
        {
            int xx = x + chunkXX;
            if (xx < 0 || xx > _overworldTileMap.TilesWide - 1) { continue; }

            for (int z = 0; z < SideLength; z++)
            {
                int zz = z + chunkZZ;
                if (zz < 0 || zz > _overworldTileMap.TilesHigh - 1) { continue; }

                // Get tileCode at OW tile position
                int tileCode = _overworldTileMap.Tile(xx, zz);
                Index tileMapIndex = _overworldTileMap.TileTexture.IndexForTileCode(tileCode);
                ushort data = (ushort)(tileMapIndex.y * tileMapWidth_WOF + tileMapIndex.x + 1);

                float blockHeight = _overworldTileMap.GetBlockHeightForTileCode(tileCode);

                for (int y = 0; y < SideLength; y++)
                {
                    int yy = y + chunkYY;
                    if (yy > blockHeight - 1) { continue; }

                    chunk.SetVoxelSimple(x, y, z, data);
                }
            }
		}
	}
}