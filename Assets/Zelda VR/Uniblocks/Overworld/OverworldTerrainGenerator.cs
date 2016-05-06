using UnityEngine;
using Uniblocks;
using Immersio.Utility;

public class OverworldTerrainGenerator : TerrainGenerator
{
    const ushort INVISIBLE_COLLIDER_VOXEL = 145;
    const ushort FLAT_GROUND_SAND_VOXEL = 146;

    const int ENTRANCE_TILE_BLOCK_HEIGHT = 2;
    const int ENTRANCE_TILE_Y_OFFSET = 3;


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

        int[,] tiles = _overworldTileMap.TileMapData._tiles;

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

                // Handle special cases involving entrance tiles (ie. grottos and dungeons)
                bool isRegularTile = true;
                float blockHeight = 1;
                int yOffset = 0;

                // Entrance?
                if (TileInfo.IsTileAnEntrance(tileCode))
                {
                    int tileCodeAbove = tiles[worldZ + 1, worldX];
                    tileCode = tileCodeAbove;

                    blockHeight = ENTRANCE_TILE_BLOCK_HEIGHT;
                    yOffset = ENTRANCE_TILE_Y_OFFSET;

                    isRegularTile = false;
                }
                else
                {
                    if (worldZ > 0)
                    {
                        // One Above Entrance?
                        int tileCodeBelow = tiles[worldZ - 1, worldX];
                        if (TileInfo.IsTileAnEntrance(tileCodeBelow))
                        {
                            blockHeight = GetBlockHeightForTileCode(tileCode);
                            yOffset = ENTRANCE_TILE_Y_OFFSET - 1;

                            isRegularTile = false;
                        }
                        else if (worldZ > 1)
                        {
                            // Two Above Entrance?
                            int tileCode2xBelow = tiles[worldZ - 2, worldX];
                            if (TileInfo.IsTileAnEntrance(tileCode2xBelow))
                            {
                                if (TileInfo.IsTileFlat(tileCode))
                                {
                                    tileCode = tileCodeBelow;
                                }

                                if (TileInfo.IsTileFlat(tileCode) || TileInfo.IsTileShort(tileCode) || TileInfo.IsTileUnitHeight(tileCode))
                                {
                                    blockHeight = 1;
                                }
                                else
                                {
                                    blockHeight = GetRandomHeight();
                                }

                                yOffset = ENTRANCE_TILE_Y_OFFSET - 2;

                                isRegularTile = false;
                            }
                        }
                    }
                }
                if(isRegularTile)
                {
                    blockHeight = GetBlockHeightForTileCode(tileCode);
                }

                Index2 tileMapIndex = _overworldTileMap.TileMapTexture.IndexForTileCode(tileCode);
                ushort data = (ushort)(tileMapIndex.y * tileMapWidth_WithoutFiller + tileMapIndex.x + 1);

                if (tileCode == 2)
                {
                    data = FLAT_GROUND_SAND_VOXEL;
                }

                // TODO

                for (int y = 0; y < SideLength; y++)
                {
                    int worldY = y + chunkY - yOffset;
                    if (worldY < -1)
                    {
                        continue;
                    }
                    if (worldY > blockHeight - 1)
                    {
                        if(TileInfo.IsTileFlatImpassable(tileCode) && worldY == blockHeight)
                        {
                            chunk.SetVoxelSimple(x, y, z, INVISIBLE_COLLIDER_VOXEL);
                        }
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
            return _flatBlockHeight;
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