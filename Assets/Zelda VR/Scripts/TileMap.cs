using UnityEngine;
using System.Collections.Generic;
using Immersio.Utility;

public class TileMap : MonoBehaviour
{
    public const float BLOCK_OFFSET_XZ = 0;        // Dependant upon WorldInfo.Instance.WorldOffset

    static Color SPECIAL_BLOCK_HIGHLIGHT_COLOR = new Color(1, 0.5f, 0.5f);


    [SerializeField]
    TileMapData _tileMapData;
    public TileMapData TileMapData { get { return _tileMapData; } }

    [SerializeField]
    TileMapTexture _tileMapTexture;
    public TileMapTexture TileMapTexture { get { return _tileMapTexture; } }

    [SerializeField]
    GameObject _blockPrefab, _shortBlockPrefab, _invisibleBlockPrefab;


    Transform _specialBlocksContainer;

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

    public bool IsTileSpecial(int x, int y)
    {
        return _specialBlockPopulationFlags[y, x];
    }

    public Index2 GetSectorForPosition(Vector3 pos)
    {
        return _tileMapData.GetSectorForPosition(pos);
    }


    void Awake()
    {
        InitFromSettings(ZeldaVRSettings.Instance);
        LoadMap();
        InitSpecialBlocks();

        if (Cheats.Instance.SecretDetectionModeIsEnabled)
        {
            HighlightAllSpecialBlocks();
        }
    }

    public void InitFromSettings(ZeldaVRSettings s)
    {
        _tileMapData.InitFromSettings(ZeldaVRSettings.Instance);
        _tileMapTexture.InitFromSettings(ZeldaVRSettings.Instance);
    }

    void InitSpecialBlocks()
    {
        float shortBlockHeight = ZeldaVRSettings.Instance.shortBlockHeight;

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
                blockHeight = b.isShortBlock ? shortBlockHeight : 1;
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

                    _specialBlockPopulationFlags[z, x] = true;
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


    void SetBlockTexture(GameObject block, int tileCode, Material sourceMaterial = null, float actualBlockHeight = 1.0f)
    {
        Renderer r = block.GetComponent<Renderer>();

        Texture2D tex = _tileMapTexture.GetTexture(tileCode);
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

    void SetBlockHeight(GameObject block, float height, float yOffset = 0.0f)
    {
        Vector3 pos = block.transform.position;
        pos.y = WorldOffsetY + (height * 0.5f) + yOffset;
        block.transform.position = pos;

        Vector3 scale = block.transform.localScale;
        scale.y = height;
        block.transform.localScale = scale;
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
                if(requisiteTileTypes.Contains(tileCode))
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
            Block b = child.GetComponent<Block>();
            if (b != null)
            {
                Color c = doHighlight ? SPECIAL_BLOCK_HIGHLIGHT_COLOR : Color.white;
                b.Colorize(c);
            }
        }
    }
}
