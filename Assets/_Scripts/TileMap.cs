using UnityEngine;
using System;
using System.IO;
using System.Globalization;


public class TileMap : MonoBehaviour
{
    public const int InvalidTile = -1;
    public const int EntranceTileBlockHeight = 2;
    public const int EntranceTileYOffset = 1;


    public GameObject blockPrefab, shortBlockPrefab, invisibleBlockPrefab;
    public TileTexture tileTexture;
    public TextAsset textAsset;

    public int tilesWide, tilesLong;
    public int blockHeight = 1;
    public float blockHeightVariance = 0;
    public float shortBlockHeight = 1;
    public float flatBlockHeight = 0.4f;

    public int sectorWidthInTiles;
    public int sectorLengthInTiles;


    public int SectorsWide { get { return (int)(tilesWide / sectorWidthInTiles); } }
    public int SectorsLong { get { return (int)(tilesLong / sectorLengthInTiles); } }


    Transform _blockContainer, _specialBlocksContainer;
    int[,] _tiles;
    bool[,] _populationFlags;
    bool[,] _specialBlockPopulationFlags;


    public int Tile(int x, int y)
    {
        if (x < 0 || x >= tilesWide) { return InvalidTile; }
        if (y < 0 || y >= tilesLong) { return InvalidTile; }

        return _tiles[y, x];
    }


    void Awake()
    {
        _blockContainer = GameObject.Find("Blocks").transform;
        _populationFlags = new bool[tilesLong, tilesWide];

        InitSpecialBlocks();

        if (Cheats.Instance.SecretDetectionModeIsEnabled)
        {
            HighlightAllSpecialBlocks();
        }
    }

    void InitSpecialBlocks()
    {
        _specialBlocksContainer = GameObject.Find("Special Blocks").transform;
        _specialBlockPopulationFlags = new bool[tilesLong, tilesWide];

        foreach (Transform sb in _specialBlocksContainer)
        {
            if (!sb.gameObject.activeSelf) { continue; }

            int xLen = (int)(sb.lossyScale.x);
            int zLen = (int)(sb.lossyScale.z);
            int startX = (int)(sb.position.x - xLen * 0.5f);
            int startZ = (int)(sb.position.z - zLen * 0.5f);

            //print("startX: " + startX + ", startZ: " + startZ + ", xLen: " + xLen + ", zLen: " + zLen);

            Block b = sb.GetComponent<Block>();
            if (b != null)
            {
                float blockheight = b.isShortBlock ? shortBlockHeight : 1;
                SetBlockHeight(b.gameObject, blockheight);
                SetBlockTexture(b.gameObject, b.tileCode);
            }

            // Set Population Flags
            for (int z = startZ; z < startZ + zLen; z++)
            {
                for (int x = startX; x < startX + xLen; x++)
                {
                    if (x < 0 || x > tilesWide - 1) { continue; }
                    if (z < 0 || z > tilesLong - 1) { continue; }
                    _specialBlockPopulationFlags[z, x] = true;

                    // Create Overhead Block?
                    /*if (b != null && b.isBombable)
                    {
                        GameObject overheadBlock = CreateBlock(b.tileCode, x, z, blockPrefab, 3, 1);
                        overheadBlock.name = "overheadBlock_" + x + "_" + z;
                        overheadBlock.transform.parent = _specialBlocksContainer;
                        overheadBlock.transform.SetY(1.5f);
                    }*/
                }
            }
        }
    }

    public void LoadMap()
    {
        StringReader reader = new StringReader(textAsset.text);

        _tiles = new int[tilesLong, tilesWide];

        const int ReadCount = 3;
        char[] buffer = new char[ReadCount];
        int tileCode;
        for (int y = tilesLong - 1; y >= 0; y--)
        {
            for (int x = 0; x < tilesWide; x++)
            {
                ReadToNext(reader);
                reader.Read(buffer, 0, ReadCount);
                string str = new String(buffer);

                if (int.TryParse(str, NumberStyles.HexNumber, null, out tileCode))
                {
                    _tiles[y, x] = tileCode;
                }
                else
                {
                    _tiles[y, x] = InvalidTile;
                }
            }
        }
    }

    void ReadToNext(StringReader reader)
    {
        bool done = false;
        do
        {
            char nextChar = (char)reader.Peek();
            
            done = (nextChar != ' ') && (nextChar != '\r') && (nextChar != '\n');

            if (!done)
            {
                reader.Read();
            }
        }
        while (!done);
    }


    public void PopulateWorld()
    {
        PopulateWorld(0, 0, tilesWide, tilesLong);
    }

    public void PopulateWorld(int left, int top, int width, int length)
    {
        int right = left + width;
        int bottom = top + length;

        left = Mathf.Clamp(left, 0, tilesWide);
        right = Mathf.Clamp(right, 0, tilesWide);
        top = Mathf.Clamp(top, 0, tilesLong);
        bottom = Mathf.Clamp(bottom, 0, tilesLong);

        for (int z = top; z < bottom; z++)
        {
            for (int x = left; x < right; x++)
            {
                if (_populationFlags[z, x] || _specialBlockPopulationFlags[z, x])
                {
                    continue;
                }

                int tileCode = _tiles[z, x];

                // Armos
                if (TileInfo.IsArmosTile(tileCode))
                {
                    tileCode = TileInfo.GetReplacementTileForArmosTile(tileCode);
                }

                // Above Entrance
                int yOffset = 0;
                if (z > 0)
                {
                    int tileCodeBelow = _tiles[z - 1, x];
                    if (TileInfo.IsTileAnEntrance(tileCodeBelow))
                    {
                        yOffset = 1;
                    }
                    else if(z > 1)
                    {
                        int tileCode2xBelow = _tiles[z - 2, x];
                        if (TileInfo.IsTileAnEntrance(tileCode2xBelow))
                        {
                            HandleTwoAboveEntranceCase(tileCode, tileCodeBelow, x, z);
                            continue;
                        }
                    }
                }

                GameObject prefab = null;
                float actualBlockHeight = 1;
                if (TileInfo.IsTileAnEntrance(tileCode))
                {
                    int tileCodeAbove = _tiles[z + 1, x];
                    CreateBlock(tileCodeAbove, x, z, blockPrefab, EntranceTileBlockHeight, EntranceTileYOffset);
                }
                else
                {
                    if (TileInfo.IsTileFlat(tileCode))
                    {
                        if (TileInfo.IsTileFlatImpassable(tileCode))
                        {
                            prefab = invisibleBlockPrefab;
                            actualBlockHeight = flatBlockHeight;
                        }
                    }
                    else if (TileInfo.IsTileShort(tileCode))
                    {
                        prefab = shortBlockPrefab;
                        actualBlockHeight = shortBlockHeight;
                    }
                    else if (TileInfo.IsTileUnitHeight(tileCode))
                    {
                        prefab = blockPrefab;
                        actualBlockHeight = 1;
                    }
                    else
                    {
                        prefab = blockPrefab;
                        actualBlockHeight = GetRandomHeight();
                    }

                    if (prefab != null)
                    {
                        CreateBlock(tileCode, x, z, prefab, actualBlockHeight, yOffset);
                    }
                }
            }
        }
    }

    void HandleTwoAboveEntranceCase(int tileCode, int tileCodeBelow, int x, int z)
    {
        GameObject prefab = null;
        float actualBlockHeight;
        if (TileInfo.IsTileFlat(tileCode))
        {
            tileCode = tileCodeBelow;
            prefab = blockPrefab;
            actualBlockHeight = 1;
        }
        else if (TileInfo.IsTileShort(tileCode))
        {
            prefab = shortBlockPrefab;
            actualBlockHeight = 1;
        }
        else if (TileInfo.IsTileUnitHeight(tileCode))
        {
            prefab = blockPrefab;
            actualBlockHeight = 1;
        }
        else
        {
            prefab = blockPrefab;
            actualBlockHeight = GetRandomHeight();
        }

        if (prefab != null)
        {
            CreateBlock(tileCode, x, z, prefab, actualBlockHeight);
        }
    }

    public void DePopulateWorld()
    {
        DePopulateWorld(0, 0, tilesWide, tilesLong);
    }

    public void DePopulateWorld(int left, int top, int width, int height)
    {
        int right = left + width;
        int bottom = top + height;

        left = Mathf.Clamp(left, 0, tilesWide);
        right = Mathf.Clamp(right, 0, tilesWide);
        top = Mathf.Clamp(top, 0, tilesLong);
        bottom = Mathf.Clamp(bottom, 0, tilesLong);

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

        outerLeft = Mathf.Clamp(outerLeft, 0, tilesWide);
        outerRight = Mathf.Clamp(outerRight, 0, tilesWide);
        outerTop = Mathf.Clamp(outerTop, 0, tilesLong);
        outerBottom = Mathf.Clamp(outerBottom, 0, tilesLong);

        for (int z = outerTop; z < outerBottom; z++)
        {
            for (int x = outerLeft; x < outerRight; x++)
            {
                if (_populationFlags[z, x] == false)
                {
                    continue;
                }

                if ((z >= top)  && (z < top + height) &&
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
        Vector3 pos = new Vector3(tileX + 0.5f, 0, tileY + 0.5f);
        Quaternion rot = Quaternion.Euler(0, 180, 0);
        GameObject g = Instantiate(prefab, pos, rot) as GameObject;

        SetBlockHeight(g, actualBlockHeight, yOffset);

        g.name = NameForBlockAtTile(tileX, tileY);
        g.transform.parent = _blockContainer;

        SetBlockTexture(g, tileCode, prefab.GetComponent<Renderer>().sharedMaterial, actualBlockHeight);
        
        _populationFlags[tileY, tileX] = true;

        return g;
    }

    void SetBlockHeight(GameObject block, float height, float yOffset = 0.0f)
    {
        Vector3 pos = block.transform.position;
        pos.y = height * 0.5f + yOffset;
        block.transform.position = pos;

        Vector3 scale = block.transform.localScale;
        scale.y = height;
        block.transform.localScale = scale;
    }

    void SetBlockTexture(GameObject block, int tileCode, Material sourceMaterial = null, float actualBlockHeight = 1.0f)
    {
        Renderer r = block.GetComponent<Renderer>();

        Texture2D tex = tileTexture.GetTexture(tileCode);
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
        int min = (int)(blockHeight * (1 - blockHeightVariance));
        int max = (int)(blockHeight * (1 + blockHeightVariance));
        return UnityEngine.Random.Range(min, max + 1);
    }

    public void RemoveBlock(int tileX, int tileY, GameObject block = null)
    {
        if (block == null)
        {
            string blockName = NameForBlockAtTile(tileX, tileY);
            block = GameObject.Find(blockName);
        }
        Destroy(block);
        _populationFlags[tileY, tileX] = false;
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


    public Vector2 GetSectorForPosition(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x / sectorWidthInTiles);
        int y = Mathf.FloorToInt(pos.z / sectorLengthInTiles);
        return new Vector2(x, y);
    }


    string NameForBlockAtTile(int tileX, int tileY)
    {
        return "Block_" + tileX + "_" + tileY;
        //return "Block_" + tileX + "_" + tileY + "_" + tileCode.ToString("X2");
    }


    override public string ToString()
    {
        string output = name + "\n";
        for (int y = 0; y < tilesLong; y++)
        {
            for (int x = 0; x < tilesWide; x++)
            {
                string hexStr;
                string str = _tiles[y, x].ToString();
                if (str == " -1")
                { hexStr = "-1"; }
                else
                { hexStr = _tiles[y, x].ToString("X2"); }
                output += hexStr + " ";
            }
            output += "\n";
        }
        return output;
    }

    public string RowToString(int row)
    {
        string output = name + " - Row: " + row + "\n";

        for (int x = 0; x < tilesWide; x++)
        {
            string hexStr;
            string str = _tiles[row, x].ToString();
            if (str == " -1")
                { hexStr = "-1"; }
            else
                { hexStr = _tiles[row, x].ToString("X2"); }
            output += hexStr + " ";
        }

        return output;
    }
}
