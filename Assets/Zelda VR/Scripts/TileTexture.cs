using UnityEngine;

public class TileTexture : MonoBehaviour
{
    [SerializeField]
    Texture2D _texture;

    [SerializeField]
    bool _initFromSettings;

    public int tileWidth, tileHeight;
    public int sideLengthInTiles;


    void Awake()
    {
        if(_initFromSettings)
        {
            tileWidth = ZeldaVRSettings.Instance.tileMapTileWidthInPixels;
            tileHeight = ZeldaVRSettings.Instance.tileMapTileHeightInPixels;
            sideLengthInTiles = ZeldaVRSettings.Instance.tileMapSideLengthInTiles;
        }
    }


    public Texture2D GetTexture(int tileCode)
    {
        Rect r = RectForTile(tileCode);
        //print("tileCode: " + tileCode + ", Rect: " + r);

        Color[] pixels = _texture.GetPixels_Safe((int)r.x, (int)r.y, (int)r.width, (int)r.height);
        if(pixels == null)
        {
            return null;
        }

        Texture2D tex = new Texture2D((int)r.width, (int)r.height);
        tex.name = "Texture_" + tileCode.ToString("X2");
        tex.SetPixels(pixels);
        tex.Apply();

        return tex;
    }


    public Index IndexForTileCode(int tileCode)
    {
        int tileX = tileCode % sideLengthInTiles;
        int tileY = (int)(tileCode / sideLengthInTiles);

        return new Index(tileX, tileY);
    }

    public Rect RectForTile(int tileCode)
    {
        Index idx = IndexForTileCode(tileCode);
        int x = idx.x * tileWidth;
        int y = idx.y * tileHeight;
        y = _texture.height - 1 - y - tileHeight;

        return new Rect(x + 1, y + 1, tileWidth - 1, tileHeight - 1);
    }
}