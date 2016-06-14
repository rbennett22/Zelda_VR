using Immersio.Utility;
using System.Globalization;
using System.IO;
using UnityEngine;

public class TileMapData : MonoBehaviour
{
    const int INVALID_TILE = -1;


    [SerializeField]
    TextAsset _textAsset;


    int _tilesWide, _tilesHigh;

    public int[,] _tiles;   // TODO: Make _tiles private and get GetCopiedTiles working


    public bool HasLoaded { get; private set; }

    public int TilesWide { get { return _tilesWide; } }
    public int TilesHigh { get { return _tilesHigh; } }

    public int TryGetTile(Index2 n)
    {
        return TryGetTile(n.x, n.y);
    }
    public int TryGetTile(int x, int y)
    {
        if (!IsTileIndexValid(x, y)) { return INVALID_TILE; }

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


    public bool IsTileIndexValid(Index2 index)
    {
        return IsTileIndexValid(index.x, index.y);
    }
    public bool IsTileIndexValid(int x, int y)
    {
        if (x < 0 || x > TilesWide - 1) { return false; }
        if (y < 0 || y > TilesHigh - 1) { return false; }

        return true;
    }


    public static bool IsTileCodeValid(int tileCode)
    {
        // TODO: There are infinitely more invalid tiles to consider...

        return tileCode != INVALID_TILE;
    }


    void Awake()
    {
        InitFromSettings(ZeldaVRSettings.Instance);
        LoadMap();
    }

    public void InitFromSettings(ZeldaVRSettings s)
    {
        _tilesWide = s.overworldWidthInTiles;
        _tilesHigh = s.overworldHeightInTiles;
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