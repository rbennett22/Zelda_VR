using Immersio.Utility;
using UnityEngine;

public class TileMapTexture : MonoBehaviour
{
    [SerializeField]
    Texture2D _texture;


    int _tileWidth, _tileHeight;
    int _sideLengthInTiles;


    void Awake()
    {
        InitFromSettings(ZeldaVRSettings.Instance);
    }

    public void InitFromSettings(ZeldaVRSettings s)
    {
        _tileWidth = s.tileMapTileWidthInPixels;
        _tileHeight = s.tileMapTileHeightInPixels;
        _sideLengthInTiles = s.tileMapSideLengthInTiles;
    }


    public Texture2D GetTexture(int tileCode)
    {
        Rect r = RectForTile(tileCode);
        //print("tileCode: " + tileCode + ", Rect: " + r);

        Color[] pixels = _texture.GetPixels_Safe((int)r.x, (int)r.y, (int)r.width, (int)r.height);
        if (pixels == null)
        {
            return null;
        }

        Texture2D tex = new Texture2D((int)r.width, (int)r.height);
        tex.name = "Texture_" + tileCode.ToString("X2");
        tex.SetPixels(pixels);
        tex.Apply();

        return tex;
    }


    public Index2 IndexForTileCode(int tileCode)
    {
        int tileX = tileCode % _sideLengthInTiles;
        int tileY = (int)(tileCode / _sideLengthInTiles);

        return new Index2(tileX, tileY);
    }

    public Rect RectForTile(int tileCode)
    {
        Index2 idx = IndexForTileCode(tileCode);
        int x = idx.x * _tileWidth;
        int y = idx.y * _tileHeight;
        y = _texture.height - 1 - y - _tileHeight;

        return new Rect(x + 1, y + 1, _tileWidth - 1, _tileHeight - 1);
    }
}