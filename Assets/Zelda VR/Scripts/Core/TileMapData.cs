using Immersio.Utility;
using System.Globalization;
using System.IO;
using UnityEngine;

public class TileMapData : MonoBehaviour
{
    const int INVALID_TILE = -1;


    [SerializeField]
    TextAsset _textAsset;


    int _sectorHeightInTiles, _sectorWidthInTiles;
    int _sectorsWide, _sectorsHigh;
    int _tilesWide, _tilesHigh;

    public int[,] _tiles;   // TODO: Make _tiles private and get GetCopiedTiles working


    public bool HasLoaded { get; private set; }

    public int SectorHeightInTiles { get { return _sectorHeightInTiles; } }
    public int SectorWidthInTiles { get { return _sectorWidthInTiles; } }
    public int SectorsWide { get { return _sectorsWide; } }
    public int SectorsHigh { get { return _sectorsHigh; } }
    public int TilesWide { get { return _tilesWide; } }
    public int TilesHigh { get { return _tilesHigh; } }

    public int Tile(Index2 n)
    {
        return Tile(n.x, n.y);
    }
    public int Tile(int x, int y)
    {
        if (x < 0 || x > TilesWide - 1) { return INVALID_TILE; }
        if (y < 0 || y > TilesHigh - 1) { return INVALID_TILE; }

        return _tiles[y, x];
    }

    // TODO: This method doesn't work correctly for some reason
    /*public int[,] GetCopiedTiles()
    {
        int[,] copy = new int[_tilesHigh, _tilesWide];
        for (int i = 0; i < _tilesHigh; i++)
        {
            for (int j = 0; j < _tilesWide; j++)
            {
                copy[i, j] = _tiles[i, j];
            }
        }
        return copy;
    }*/

    public static bool IsTileCodeValid(int tileCode)
    {
        // TODO: There are infinitely more invalid tiles to consider...

        return tileCode != INVALID_TILE;
    }


    public Index2 GetSectorForPosition(Vector3 pos)
    {
        return GetSectorForPosition(pos.x, pos.z);
    }
    public Index2 GetSectorForPosition(float x, float z)
    {
        int sectorX = Mathf.FloorToInt(x / SectorWidthInTiles);
        int sectorY = Mathf.FloorToInt(z / SectorHeightInTiles);
        return new Index2(sectorX, sectorY);
    }

    // Returns the tile index relative to the sector the provided tile (x, y) is within.
    // The 'sector' param will also be assigned to the index of the containing sector
    public Index2 TileIndex_WorldToSector(int x, int y, out Index2 sector)
    {
        sector = GetSectorForPosition(x, y);
        Index2 sIdx = new Index2();
        sIdx.x = x % _sectorWidthInTiles;
        sIdx.y = y % _sectorHeightInTiles;
        return sIdx;
    }
    // Returns the world-space tile index corresponding to the provided sector-space tile (sX, sY) 
    public Index2 TileIndex_SectorToWorld(int sX, int sY, Index2 sector)
    {
        Index2 idx = new Index2();
        idx.x = sector.x * _sectorWidthInTiles + sX;
        idx.y = sector.y * _sectorHeightInTiles + sY;
        return idx;
    }

    // Returns the tile at the world-space tile index corresponding to the provided 
    //  sector-space tile ('tileIdx_S') and sector ('sector')
    public int GetTileInSector(Index2 sector, Index2 tileIdx_S)
    {
        Index2 t = TileIndex_SectorToWorld(tileIdx_S.x, tileIdx_S.y, sector);
        return Tile(t);
    }


    void Awake()
    {
        InitFromSettings(ZeldaVRSettings.Instance);
        LoadMap();
    }

    public void InitFromSettings(ZeldaVRSettings s)
    {
        _sectorWidthInTiles = s.overworldSectorWidthInTiles;
        _sectorHeightInTiles = s.overworldSectorHeightInTiles;
        _sectorsWide = s.overworldWidthInSectors;
        _sectorsHigh = s.overworldHeightInSectors;

        _tilesWide = SectorsWide * SectorWidthInTiles;
        _tilesHigh = SectorsHigh * SectorHeightInTiles;
    }

    public void LoadMap()
    {
        if (HasLoaded)
        {
            Debug.LogError("Attempting to Load TileMapData more than once");
            return;
        }

        StringReader reader = new StringReader(_textAsset.text);

        _tiles = new int[TilesHigh, TilesWide];

        const int ReadCount = 3;
        char[] buffer = new char[ReadCount];
        int tileCode;
        for (int y = TilesHigh - 1; y >= 0; y--)
        {
            for (int x = 0; x < TilesWide; x++)
            {
                ReadToNext(reader);
                reader.Read(buffer, 0, ReadCount);
                string str = new string(buffer);

                if (int.TryParse(str, NumberStyles.HexNumber, null, out tileCode))
                {
                    if (TileInfo.IsArmosTile(tileCode))
                    {
                        tileCode = TileInfo.GetReplacementTileForArmosTile(tileCode);
                    }

                    _tiles[y, x] = tileCode;
                }
                else
                {
                    _tiles[y, x] = INVALID_TILE;
                }
            }
        }

        HasLoaded = true;
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


    override public string ToString()
    {
        string output = name + "\n";
        for (int y = 0; y < TilesHigh; y++)
        {
            for (int x = 0; x < TilesWide; x++)
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

    string RowToString(int row)
    {
        string output = name + " - Row: " + row + "\n";

        for (int x = 0; x < TilesWide; x++)
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