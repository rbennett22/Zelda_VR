using Immersio.Utility;
using Uniblocks;
using UnityEngine;

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
    int _tileMapWidth_WithoutFiller;


    TileMap _overworldTileMap;


    void InitFromSettings(ZeldaVRSettings s)
    {
        _blockHeight = s.blockHeight;
        _blockHeightVariance = s.blockHeightVariance;
        _shortBlockHeight = s.shortBlockHeight;
        _flatBlockHeight = s.flatBlockHeight;
        _tileMapWidth_WithoutFiller = s.tileMapWidthInTiles_WithoutFiller;
    }


    void InitOverworldTileMap()
    {
        _overworldTileMap = CommonObjects.OverworldTileMap;
        if (_overworldTileMap != null)
        {
            _overworldTileMap.InitFromSettings(ZeldaVRSettings.Instance);
            _overworldTileMap.LoadMap();
        }
    }

    int[,] _tiles;
    public override void GenerateVoxelData()
    {
        if (_overworldTileMap == null)
        {
            InitFromSettings(ZeldaVRSettings.Instance);
            InitOverworldTileMap();
            if (_overworldTileMap == null)
                return;
        }

        int chunkSizeX = Engine.chunkSizeX;
        int chunkSizeY = Engine.chunkSizeY;
        int chunkSizeZ = Engine.chunkSizeZ;

        int chunkX = chunkSizeX * chunk.chunkIndex.x;
        int chunkY = chunkSizeY * chunk.chunkIndex.y;
        int chunkZ = chunkSizeZ * chunk.chunkIndex.z;

        _tiles = _overworldTileMap.TileMapData._tiles;

        Index2 lostWoodsSector = WorldInfo.Instance.LostWoodsSector;

        for (int vX = 0; vX < chunkSizeX; vX++)
        {
            int x = vX + chunkX;
            if (x < 0 || x > _overworldTileMap.TilesWide - 1)
            {
                continue;
            }

            for (int vZ = 0; vZ < chunkSizeZ; vZ++)
            {
                int z = vZ + chunkZ;
                if (z < 0 || z > _overworldTileMap.TilesHigh - 1)
                {
                    continue;
                }

                if (LostWoods.IsWarpedToDuplicate)
                {
                    Index2 sector;
                    Index2 tileIdx_S = _overworldTileMap.TileIndex_WorldToSector(x, z, out sector);
                    if (!sector.IsEqual(lostWoodsSector))
                    {
                        Index2 tileIdx = _overworldTileMap.TileIndex_SectorToWorld(tileIdx_S.x, tileIdx_S.y, lostWoodsSector);
                        x = tileIdx.x;
                        z = tileIdx.y;
                    }
                }

                if (_overworldTileMap.IsTileSpecial(x, z))
                {
                    continue;
                }


                int tileCode;
                float blockHeight;
                int yOffset;
                if (IsRegularTile(x, z, out tileCode, out blockHeight, out yOffset))
                {
                    blockHeight = GetBlockHeightForTileCode(tileCode);
                }

                ushort data = GetDataForTileCode(tileCode);

                if (tileCode == 2)
                {
                    data = FLAT_GROUND_SAND_VOXEL;
                }

                // TODO

                for (int vY = 0; vY < chunkSizeY; vY++)
                {
                    int y = vY + chunkY - yOffset;
                    if (y < -1)
                    {
                        continue;
                    }
                    if (y > blockHeight - 1)
                    {
                        if (TileInfo.IsTileFlatImpassable(tileCode) && y == blockHeight)
                        {
                            chunk.SetVoxelSimple(vX, vY, vZ, INVISIBLE_COLLIDER_VOXEL);
                        }
                        else
                        {
                            chunk.SetVoxelSimple(vX, vY, vZ, 0);    //// Currently necessary for removing blocks during LostWoods transition
                        }
                        continue;
                        ////break;
                    }

                    chunk.SetVoxelSimple(vX, vY, vZ, data);
                }
            }
        }
    }

    ushort GetDataForTileCode(int tileCode)
    {
        Index2 tileMapIndex = _overworldTileMap.TileMapTexture.IndexForTileCode(tileCode);
        return (ushort)(tileMapIndex.y * _tileMapWidth_WithoutFiller + tileMapIndex.x + 1);
    }

    bool IsRegularTile(int x, int z, out int tileCode, out float blockHeight, out int yOffset)
    {
        bool isRegularTile = true;

        // Get tileCode at OW tile position
        tileCode = _tiles[z, x];

        // Handle special cases involving entrance tiles (ie. grottos and dungeons)
        blockHeight = 1;
        yOffset = 0;

        // Entrance?
        if (TileInfo.IsTileAnEntrance(tileCode))
        {
            int tileCodeAbove = _tiles[z + 1, x];
            tileCode = tileCodeAbove;

            blockHeight = ENTRANCE_TILE_BLOCK_HEIGHT;
            yOffset = ENTRANCE_TILE_Y_OFFSET;

            isRegularTile = false;
        }
        else
        {
            if (z > 0)
            {
                // One Above Entrance?
                int tileCodeBelow = _tiles[z - 1, x];
                if (TileInfo.IsTileAnEntrance(tileCodeBelow))
                {
                    blockHeight = GetBlockHeightForTileCode(tileCode);
                    yOffset = ENTRANCE_TILE_Y_OFFSET - 1;

                    isRegularTile = false;
                }
                else if (z > 1)
                {
                    // Two Above Entrance?
                    int tileCode2xBelow = _tiles[z - 2, x];
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

        return isRegularTile;
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
            return GetRandomHeight();
        }
    }

    int GetRandomHeight()
    {
        int min = (int)(_blockHeight - _blockHeightVariance);
        int max = (int)(_blockHeight + _blockHeightVariance);
        return Random.Range(min, max + 1);
    }
}