using Immersio.Utility;
using Uniblocks;
using UnityEngine;

public class OverworldTerrainGenerator : TerrainGenerator
{
    const ushort INVISIBLE_COLLIDER_VOXEL = 145;
    const ushort FLAT_GROUND_SAND_VOXEL = 146;

    const int ENTRANCE_TILE_BLOCK_HEIGHT = 2;
    const int ENTRANCE_TILE_Y_OFFSET = 3;


    public bool useOverridingSector;
    public Index2 overridingSector;


    int _blockHeight = 1;
    float _blockHeightVariance = 0;
    float _shortBlockHeight = 1;
    float _flatBlockHeight = 0;
    int _tileMapWidth_WithoutFiller;


    TileMap _overworldTileMap;
    int[,] _tiles;


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


    public Index3 ChunkPosition {
        get {
            Index3 size = Engine.ChunkSize;
            Index3 pos = new Index3();
            pos.x = size.x * chunk.chunkIndex.x;
            pos.y = size.y * chunk.chunkIndex.y;
            pos.z = size.z * chunk.chunkIndex.z;
            return pos;
        }
    }

    public override void GenerateVoxelData()
    {
        GenerateVoxelData(false);
    }
    public void GenerateVoxelData(bool clearPrevious)
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
                continue;
            }

            for (int vZ = 0; vZ < chunkSize.z; vZ++)
            {
                int z = vZ + chunkPos.z;
                if (z < 0 || z > tilesHigh - 1)
                {
                    continue;
                }

                if (useOverridingSector)
                {
                    Index2 sector;
                    Index2 tileIdx_S = _overworldTileMap.TileIndex_WorldToSector(x, z, out sector);
                    if (sector != overridingSector)
                    {
                        Index2 tileIdx = _overworldTileMap.TileIndex_SectorToWorld(tileIdx_S.x, tileIdx_S.y, overridingSector);
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
                    blockStackHeight = GetBlockHeightForTileCode(tileCode);
                }

                ushort data = GetDataForTileCode(tileCode);

                if (tileCode == 2)
                {
                    data = FLAT_GROUND_SAND_VOXEL;
                }

                // TODO

                for (int vY = 0; vY < chunkSize.y; vY++)
                {
                    // TODO: Make clearer/more generic

                    int y = vY + chunkPos.y - yOffset;
                    if (y < -1)
                    {
                        continue;
                    }
                    if (y > blockStackHeight - 1)
                    {
                        bool isTopmostBlockInStack = TileInfo.IsTileFlatImpassable(tileCode) && (y == blockStackHeight);
                        if (isTopmostBlockInStack)
                        {
                            chunk.SetVoxelSimple(vX, vY, vZ, INVISIBLE_COLLIDER_VOXEL);
                        }

                        if (clearPrevious)
                        {
                            if (!isTopmostBlockInStack)
                            {
                                chunk.SetVoxelSimple(vX, vY, vZ, 0);
                            }
                            continue;
                        }
                        else
                        {
                            break;
                        }
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