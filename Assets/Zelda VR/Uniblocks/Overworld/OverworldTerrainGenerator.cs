using UnityEngine;
using Uniblocks;
using Immersio.Utility;

public class OverworldTerrainGenerator : TerrainGenerator
{
    int _blockHeight = 1;
    float _blockHeightVariance = 0;
    float _shortBlockHeight = 1;
    float _flatBlockHeight = 0;


    TileMap _overworldTileMap;


    void InitFromSettings(ZeldaVRSettings s)
    {
        _blockHeight = s.blockHeight;
        _blockHeightVariance = s.blockHeightVariance;
        _shortBlockHeight = s.shortBlockHeight;
        _flatBlockHeight = s.flatBlockHeight;
    }


    void InitOverworldTileMap()
    {
        _overworldTileMap = (Engine.EngineInstance as OverworldTerrainEngine).TileMap;
        if (_overworldTileMap != null)
        {
            _overworldTileMap.InitFromSettings(ZeldaVRSettings.Instance);
            _overworldTileMap.LoadMap();
        }
    }


    public override void GenerateVoxelData ()
    {
        if (_overworldTileMap == null)
        {
            InitFromSettings(ZeldaVRSettings.Instance);
            InitOverworldTileMap();
            if (_overworldTileMap == null)
                return;
        }

        int tileMapWidth_WithoutFiller = ZeldaVRSettings.Instance.tileMapWidthInTiles_WithoutFiller;

        int SideLength = Engine.ChunkSideLength;

        int chunkX = SideLength * chunk.ChunkIndex.x;
        int chunkY = SideLength * chunk.ChunkIndex.y;
        int chunkZ = SideLength * chunk.ChunkIndex.z;

        for (int x = 0; x < SideLength; x++)
        {
            int worldX = x + chunkX;
            if (worldX < 0 || worldX > _overworldTileMap.TilesWide - 1)
            {
                continue;
            }

            for (int z = 0; z < SideLength; z++)
            {
                int worldZ = z + chunkZ;
                if (worldZ < 0 || worldZ > _overworldTileMap.TilesHigh - 1)
                {
                    continue;
                }

                if (_overworldTileMap.IsTileSpecial(worldX, worldZ))
                {
                    continue;
                }

                // Get tileCode at OW tile position
                int tileCode = _overworldTileMap.Tile(worldX, worldZ);
                Index2 tileMapIndex = _overworldTileMap.TileMapTexture.IndexForTileCode(tileCode);
                ushort data = (ushort)(tileMapIndex.y * tileMapWidth_WithoutFiller + tileMapIndex.x + 1);

                float blockHeight = GetBlockHeightForTileCode(tileCode);

                for (int y = 0; y < SideLength; y++)
                {
                    int worldY = y + chunkY;
                    if (worldY > blockHeight - 1)
                    {
                        break;
                    }

                    chunk.SetVoxelSimple(x, y, z, data);
                }
            }
		}
	}

    float GetBlockHeightForTileCode(int tileCode)
    {
        if (TileInfo.IsTileFlat(tileCode))
        {
            return TileInfo.IsTileFlatImpassable(tileCode) ? _flatBlockHeight : 0;
        }
        else if (TileInfo.IsTileShort(tileCode))
        {
            return _shortBlockHeight;
        }
        else if (TileInfo.IsTileUnitHeight(tileCode))
        {
            return 1;
        }
        else
        {
            return  GetRandomHeight();
        }
    }

    int GetRandomHeight()
    {
        int min = (int)(_blockHeight - _blockHeightVariance);
        int max = (int)(_blockHeight + _blockHeightVariance);
        return Random.Range(min, max + 1);
    }
}