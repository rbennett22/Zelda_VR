using Immersio.Utility;
using Uniblocks;
using UnityEngine;

public class OverworldTerrainGenerator : TerrainGenerator
{
    const ushort INVISIBLE_COLLIDER_VOXEL = 145;
    const ushort FLAT_GROUND_SAND_VOXEL = 146;
    const ushort FLAT_GROUND_GRAY_VOXEL = 147;

    const int ENTRANCE_TILE_BLOCK_HEIGHT = 2;
    const int ENTRANCE_TILE_Y_OFFSET = 3;


    int _blockHeight = 1;
    float _blockHeightVariance = 0;
    float _shortBlockHeight = 1;
    float _flatBlockHeight = 0;
    int _tileMapWidth_WithoutFiller;


    TileMap _overworldTileMap;
    int[,] _tiles;


    public OverworldChunk ChunkOW { get { return chunk as OverworldChunk; } }

    public Index3 ChunkPosition
    {
        get
        {
            Index3 size = Engine.ChunkSize;
            Index idx = ChunkOW.chunkIndex;

            Index3 P = new Index3();
            P.x = size.x * idx.x;
            P.y = size.y * idx.y;
            P.z = size.z * idx.z;

            return P;
        }
    }


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


    public override void GenerateVoxelData()
    {
        if (_overworldTileMap == null)
        {
            InitFromSettings(ZeldaVRSettings.Instance);
            InitOverworldTileMap();

            if (_overworldTileMap == null)
            {
                return;
            }
        }

        Index3 chunkSize = Engine.ChunkSize;
        Index3 chunkPos = ChunkPosition;

        _tiles = _overworldTileMap.TileMapData._tiles;
        int tilesWide = _overworldTileMap.TilesWide;
        int tilesHigh = _overworldTileMap.TilesHigh;

        for (int vX = 0; vX < chunkSize.x; vX++)
        {
            int x = vX + chunkPos.x;
            if (x < 0 || x > tilesWide - 1)
            {
                if (!ChunkOW.useOverridingSector)
                {
                    continue;
                }
            }

            for (int vZ = 0; vZ < chunkSize.z; vZ++)
            {
                int z = vZ + chunkPos.z;
                if (z < 0 || z > tilesHigh - 1)
                {
                    if (!ChunkOW.useOverridingSector)
                    {
                        continue;
                    }
                }

                // Clear All first
                for (int vY = 0; vY < chunkSize.y; vY++)
                {
                    ChunkOW.SetVoxelSimple(vX, vY, vZ, 0);
                }


                if (ChunkOW.useOverridingSector)
                {
                    Index2 sector;
                    Index2 tileIdx_S = _overworldTileMap.TileIndex_WorldToSector(x, z, out sector);
                    if (sector != ChunkOW.overridingSector)
                    {
                        Index2 tileIdx = _overworldTileMap.TileIndex_SectorToWorld(tileIdx_S, ChunkOW.overridingSector);
                        x = tileIdx.x;
                        z = tileIdx.y;
                    }
                }

                if (_overworldTileMap.IsTileSpecial(x, z))
                {
                    continue;
                }


                int tileCode;
                float blockStackHeight;
                int yOffset;
                if (IsRegularTile(x, z, out tileCode, out blockStackHeight, out yOffset))
                {
                    blockStackHeight = GetBlockHeightForTileCode(tileCode, x, z);
                }

                ushort data = GetDataForTileCode(tileCode);

                if (tileCode == 2)
                {
                    data = FLAT_GROUND_SAND_VOXEL;
                }
                else if (tileCode == 14)
                {
                    data = FLAT_GROUND_GRAY_VOXEL;
                }

                bool isTileFlat = TileInfo.IsTileFlat(tileCode);
                bool isTileFlatImpassable = TileInfo.IsTileFlatImpassable(tileCode);

                // TODO

                for (int vY = 0; vY < chunkSize.y; vY++)
                {
                    // TODO: Make clearer/more generic

                    int y = vY + chunkPos.y - yOffset;
                    if (y < -1)
                    {
                        continue;
                    }
                    if (y < 0)
                    {
                        if (!isTileFlat)
                        {
                            continue;
                        }
                    }
                    if (y > blockStackHeight - 1)
                    {
                        bool isFlatTop = isTileFlatImpassable && (y == blockStackHeight);
                        if (isFlatTop)
                        {
                            ChunkOW.SetVoxelSimple(vX, vY, vZ, INVISIBLE_COLLIDER_VOXEL);
                        }

                        /*// Clear previous
                        if (!isFlatTop)
                        {
                            ChunkOW.SetVoxelSimple(vX, vY, vZ, 0);
                        }*/
                        continue;
                    }

                    ChunkOW.SetVoxelSimple(vX, vY, vZ, data);
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

            return false;
        }

        if (z == 0)
        {
            return true;
        }

        // One Above Entrance?
        int tileCodeBelow = _tiles[z - 1, x];
        if (TileInfo.IsTileAnEntrance(tileCodeBelow))
        {
            blockHeight = GetBlockHeightForTileCode(tileCode, x, z - 1);
            yOffset = ENTRANCE_TILE_Y_OFFSET - 1;

            return false;
        }

        if (z == 1)
        {
            return true;
        }

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
                blockHeight = GetRandomBlockHeightForTile(x, z - 2);
            }

            yOffset = ENTRANCE_TILE_Y_OFFSET - 2;

            return false;
        }

        return true;
    }

    float GetBlockHeightForTileCode(int tileCode, int x, int z)
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
            return GetRandomBlockHeightForTile(x, z);
        }
    }


    // TODO
    int GetRandomBlockHeightForTile(Index2 tile)
    {
        return GetRandomBlockHeightForTile(tile.x, tile.y);
    }
    int GetRandomBlockHeightForTile(int x, int z)
    {
        int min = (int)(_blockHeight - _blockHeightVariance);
        int max = (int)(_blockHeight + _blockHeightVariance);

        int pRand = (x + 3) ^ (z + 5);  // pseudo-random number
        return min + (pRand % (max + 1 - min));
    }

    int GetRandomBlockHeight()
    {
        int min = (int)(_blockHeight - _blockHeightVariance);
        int max = (int)(_blockHeight + _blockHeightVariance);
        return Random.Range(min, max + 1);
    }
}