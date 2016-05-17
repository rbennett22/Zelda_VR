#pragma warning disable 0649 // variable is never assigned to

using Immersio.Utility;
using System.Collections.Generic;
using UnityEngine;

public class TileMap_OLD : MonoBehaviour
{
    public const float BLOCK_OFFSET_XZ = 0;        // Dependant upon WorldInfo.Instance.WorldOffset

    const int ENTRANCE_TILE_BLOCK_HEIGHT = 2;
    const int ENTRANCE_TILE_Y_OFFSET = 1;


    [SerializeField]
    TileMapData _tileMapData;

    public TileMapData TileMapData { get { return _tileMapData; } }

    [SerializeField]
    TileMapTexture _tileTexture;

    public TileMapTexture TileTexture { get { return _tileTexture; } }

    [SerializeField]
    GameObject _blockPrefab, _shortBlockPrefab, _invisibleBlockPrefab;

    [SerializeField]
    int _blockHeight = 1;

    [SerializeField]
    float _blockHeightVariance = 0;

    [SerializeField]
    float _shortBlockHeight = 1;

    [SerializeField]
    float _flatBlockHeight = 0;


    Transform _blockContainer, _specialBlocksContainer;

    float[,] _blockHeights;
    bool[,] _populationFlags;
    bool[,] _specialBlockPopulationFlags;


    public float WorldOffsetY { get { return WorldInfo.Instance.WorldOffset.y; } }

    public int SectorHeightInTiles { get { return _tileMapData.SectorHeightInTiles; } }
    public int SectorWidthInTiles { get { return _tileMapData.SectorWidthInTiles; } }
    public int SectorsWide { get { return _tileMapData.SectorsWide; } }
    public int SectorsHigh { get { return _tileMapData.SectorsHigh; } }
    public int TilesWide { get { return _tileMapData.TilesWide; } }
    public int TilesHigh { get { return _tileMapData.TilesHigh; } }


    public int Tile(Index2 n)
    {
        return _tileMapData.Tile(n);
    }
    public int Tile(int x, int y)
    {
        return _tileMapData.Tile(x, y);
    }

    public Index2 GetSectorForPosition(Vector3 pos)
    {
        return _tileMapData.GetSectorForPosition(pos);
    }


    void Awake()
    {
        InitFromSettings(ZeldaVRSettings.Instance);

        _blockHeights = new float[TilesHigh, TilesWide];
        _populationFlags = new bool[TilesHigh, TilesWide];

        // TODO

        _blockContainer = GameObject.Find("Blocks").transform;

        InitSpecialBlocks();

        if (Cheats.Instance.SecretDetectionModeIsEnabled)
        {
            HighlightAllSpecialBlocks();
        }
    }

    void InitFromSettings(ZeldaVRSettings s)
    {
        _blockHeight = s.blockHeight;
        _blockHeightVariance = s.blockHeightVariance;
        _shortBlockHeight = s.shortBlockHeight;
        _flatBlockHeight = s.flatBlockHeight;

        _tileMapData.InitFromSettings(s);
    }

    void InitSpecialBlocks()
    {
        _specialBlocksContainer = GameObject.Find("Special Blocks").transform;
        _specialBlockPopulationFlags = new bool[TilesHigh, TilesWide];

        foreach (Transform sb in _specialBlocksContainer)
        {
            if (!sb.gameObject.activeSelf) { continue; }

            int xLen = (int)(sb.lossyScale.x);
            int zLen = (int)(sb.lossyScale.z);
            int startX = (int)(sb.position.x - xLen * BLOCK_OFFSET_XZ);
            int startZ = (int)(sb.position.z - zLen * BLOCK_OFFSET_XZ);

            //print("startX: " + startX + ", startZ: " + startZ + ", xLen: " + xLen + ", zLen: " + zLen);

            float blockHeight = 0;
            Block b = sb.GetComponent<Block>();
            if (b != null)
            {
                blockHeight = b.isShortBlock ? _shortBlockHeight : 1;
                SetBlockHeight(b.gameObject, blockHeight);
                SetBlockTexture(b.gameObject, b.tileCode);
            }

            // Set Population Flags
            for (int z = startZ; z < startZ + zLen; z++)
            {
                for (int x = startX; x < startX + xLen; x++)
                {
                    if (x < 0 || x > TilesWide - 1) { continue; }
                    if (z < 0 || z > TilesHigh - 1) { continue; }

                    _blockHeights[z, x] = blockHeight;
                    _specialBlockPopulationFlags[z, x] = true;

                    // Create Overhead Block?
                    /*if (b != null && b.isBombable)
                    {
                        GameObject overheadBlock = CreateBlock(b.tileCode, x, z, blockPrefab, 3, 1);
                        overheadBlock.name = "overheadBlock_" + x + "_" + z;
                        overheadBlock.transform.parent = _specialBlocksContainer;
                        overheadBlock.transform.SetY(GroundPosY + 1.5f);
                    }*/
                }
            }
        }
    }

    public void LoadMap()
    {
        if (!_tileMapData.HasLoaded)
        {
            _tileMapData.LoadMap();
        }
    }


    public void PopulateWorld()
    {
        PopulateWorld(0, 0, TilesWide, TilesHigh);
    }

    public void PopulateWorld(int left, int top, int width, int length)
    {
        int right = left + width;
        int bottom = top + length;

        left = Mathf.Clamp(left, 0, TilesWide);
        right = Mathf.Clamp(right, 0, TilesWide);
        top = Mathf.Clamp(top, 0, TilesHigh);
        bottom = Mathf.Clamp(bottom, 0, TilesHigh);

        int[,] tiles = _tileMapData._tiles;

        for (int z = top; z < bottom; z++)
        {
            for (int x = left; x < right; x++)
            {
                if (_populationFlags[z, x] || _specialBlockPopulationFlags[z, x])
                {
                    continue;
                }

                int tileCode = tiles[z, x];

                // Armos
                if (TileInfo.IsArmosTile(tileCode))
                {
                    tileCode = TileInfo.GetReplacementTileForArmosTile(tileCode);
                }

                // Above Entrance
                int yOffset = 0;
                if (z > 0)
                {
                    int tileCodeBelow = tiles[z - 1, x];
                    if (TileInfo.IsTileAnEntrance(tileCodeBelow))
                    {
                        yOffset = 1;
                    }
                    else if (z > 1)
                    {
                        int tileCode2xBelow = tiles[z - 2, x];
                        if (TileInfo.IsTileAnEntrance(tileCode2xBelow))
                        {
                            HandleTwoAboveEntranceCase(tileCode, tileCodeBelow, x, z);
                            continue;
                        }
                    }
                }

                if (TileInfo.IsTileAnEntrance(tileCode))
                {
                    int tileCodeAbove = tiles[z + 1, x];
                    CreateBlock(tileCodeAbove, x, z, _blockPrefab, ENTRANCE_TILE_BLOCK_HEIGHT, ENTRANCE_TILE_Y_OFFSET);
                }
                else
                {
                    GameObject prefab = GetBlockPrefabForTileCode(tileCode);
                    if (prefab != null)
                    {
                        float actualBlockHeight = GetBlockHeightForTileCode(tileCode);
                        CreateBlock(tileCode, x, z, prefab, actualBlockHeight, yOffset);
                    }
                }
            }
        }
    }

    void HandleTwoAboveEntranceCase(int tileCode, int tileCodeBelow, int x, int z)
    {
        GameObject prefab = null;
        float actualBlockHeight = 1;

        if (TileInfo.IsTileFlat(tileCode))
        {
            tileCode = tileCodeBelow;
            prefab = _blockPrefab;
        }
        else if (TileInfo.IsTileShort(tileCode))
        {
            prefab = _shortBlockPrefab;
        }
        else if (TileInfo.IsTileUnitHeight(tileCode))
        {
            prefab = _blockPrefab;
        }
        else
        {
            prefab = _blockPrefab;
            actualBlockHeight = GetRandomHeight();
        }

        if (prefab != null)
        {
            CreateBlock(tileCode, x, z, prefab, actualBlockHeight);
        }
    }

    GameObject GetBlockPrefabForTileCode(int tileCode)
    {
        if (TileInfo.IsTileFlat(tileCode))
        {
            if (TileInfo.IsTileFlatImpassable(tileCode))
            {
                return _invisibleBlockPrefab;
            }
            else
            {
                return null;
            }
        }
        else if (TileInfo.IsTileShort(tileCode))
        {
            return _shortBlockPrefab;
        }
        else if (TileInfo.IsTileUnitHeight(tileCode))
        {
            return _blockPrefab;
        }
        else
        {
            return _blockPrefab;
        }
    }


    public float GetBlockHeightForTileCode(int tileCode)
    {
        if (TileInfo.IsTileFlat(tileCode))
        {
            if (TileInfo.IsTileFlatImpassable(tileCode))
            {
                return _flatBlockHeight;
            }
            else
            {
                return 0;   //
            }
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


    public void DePopulateWorld()
    {
        DePopulateWorld(0, 0, TilesWide, TilesHigh);
    }

    public void DePopulateWorld(int left, int top, int width, int height)
    {
        int right = left + width;
        int bottom = top + height;

        left = Mathf.Clamp(left, 0, TilesWide);
        right = Mathf.Clamp(right, 0, TilesWide);
        top = Mathf.Clamp(top, 0, TilesHigh);
        bottom = Mathf.Clamp(bottom, 0, TilesHigh);

        for (int z = top; z < bottom; z++)
        {
            for (int x = left; x < right; x++)
            {
                if (_populationFlags[z, x] == false)
                {
                    continue;
                }

                RemoveBlock(x, z);
            }
        }
    }

    public void DePopulateWorldExcluding(int left, int top, int width, int height, int exclusionDistance)
    {
        int outerLeft = left - exclusionDistance;
        int outerTop = top - exclusionDistance;
        int outerRight = left + width + exclusionDistance;
        int outerBottom = top + height + exclusionDistance;

        outerLeft = Mathf.Clamp(outerLeft, 0, TilesWide);
        outerRight = Mathf.Clamp(outerRight, 0, TilesWide);
        outerTop = Mathf.Clamp(outerTop, 0, TilesHigh);
        outerBottom = Mathf.Clamp(outerBottom, 0, TilesHigh);

        for (int z = outerTop; z < outerBottom; z++)
        {
            for (int x = outerLeft; x < outerRight; x++)
            {
                if (_populationFlags[z, x] == false)
                {
                    continue;
                }

                if ((z >= top) && (z < top + height) &&
                    (x >= left) && (x < left + width))
                {
                    continue;
                }

                RemoveBlock(x, z);
            }
        }
    }


    public GameObject CreateBlock(int tileCode, int tileX, int tileY, GameObject prefab, float actualBlockHeight = 1.0f, float yOffset = 0.0f)
    {
        Vector3 pos = new Vector3(tileX + BLOCK_OFFSET_XZ, WorldOffsetY, tileY + BLOCK_OFFSET_XZ);
        Quaternion rot = Quaternion.Euler(0, 180, 0);
        GameObject g = Instantiate(prefab, pos, rot) as GameObject;

        SetBlockHeight(g, actualBlockHeight, yOffset);

        g.name = NameForBlockAtTile(tileX, tileY);
        g.transform.parent = _blockContainer;

        SetBlockTexture(g, tileCode, prefab.GetComponent<Renderer>().sharedMaterial, actualBlockHeight);

        _blockHeights[tileY, tileX] = actualBlockHeight;
        _populationFlags[tileY, tileX] = true;

        return g;
    }


    void SetBlockHeight(GameObject block, float height, float yOffset = 0.0f)
    {
        Vector3 pos = block.transform.position;
        pos.y = WorldOffsetY + (height * 0.5f) + yOffset;
        block.transform.position = pos;

        Vector3 scale = block.transform.localScale;
        scale.y = height;
        block.transform.localScale = scale;
    }

    void SetBlockTexture(GameObject block, int tileCode, Material sourceMaterial = null, float actualBlockHeight = 1.0f)
    {
        Renderer r = block.GetComponent<Renderer>();

        Texture2D tex = _tileTexture.GetTexture(tileCode);
        if (sourceMaterial == null)
        {
            sourceMaterial = r.sharedMaterial;
        }
        Material mat = new Material(sourceMaterial);
        tex.filterMode = FilterMode.Point;
        mat.SetTexture("_MainTex", tex);

        Destroy(r.material);
        r.material = mat;
        r.material.mainTextureScale = new Vector2(1, actualBlockHeight);
    }

    int GetRandomHeight()
    {
        int min = (int)(_blockHeight - _blockHeightVariance);
        int max = (int)(_blockHeight + _blockHeightVariance);
        return Random.Range(min, max + 1);
    }


    public void RemoveBlock(int tileX, int tileY, GameObject block = null)
    {
        if (block == null)
        {
            string blockName = NameForBlockAtTile(tileX, tileY);
            block = GameObject.Find(blockName);
        }

        Destroy(block);

        _blockHeights[tileY, tileX] = 0;
        _populationFlags[tileY, tileX] = false;
    }


    public List<Index2> GetTilesInArea(Rect area, List<int> requisiteTileTypes = null)
    {
        return GetTilesInArea((int)area.xMin, (int)area.yMin, (int)area.width, (int)area.height, requisiteTileTypes);
    }
    public List<Index2> GetTilesInArea(int xMin, int yMin, int width, int height, List<int> requisiteTileTypes = null)
    {
        List<Index2> tileIndices = new List<Index2>();

        int right = xMin + width;
        int top = yMin + height;

        xMin = Mathf.Clamp(xMin, 0, TilesWide);
        right = Mathf.Clamp(right, 0, TilesWide);
        yMin = Mathf.Clamp(yMin, 0, TilesHigh);
        top = Mathf.Clamp(top, 0, TilesHigh);

        int[,] tiles = _tileMapData._tiles;

        for (int z = yMin; z < top; z++)
        {
            for (int x = xMin; x < right; x++)
            {
                int tileCode = tiles[z, x];
                if (requisiteTileTypes.Contains(tileCode))
                {
                    tileIndices.Add(new Index2(x, z));
                }
            }
        }

        return tileIndices;
    }


    public void HighlightAllSpecialBlocks(bool doHighlight = true)
    {
        foreach (Transform child in _specialBlocksContainer)
        {
            Block block = child.GetComponent<Block>();
            if (block == null) { continue; }

            if (doHighlight)
            {
                ColorBlockRed(block.gameObject);
            }
            else
            {
                block.GetComponent<Renderer>().material.color = Color.white;
            }
        }
    }

    void ColorBlockRed(GameObject block)
    {
        Color c = block.GetComponent<Renderer>().material.color;
        c.r = 1.0f;
        c.g = 0.5f;
        c.b = 0.5f;
        block.GetComponent<Renderer>().material.color = c;
    }


    string NameForBlockAtTile(int tileX, int tileY)
    {
        return "Block_" + tileX + "_" + tileY;
    }
}