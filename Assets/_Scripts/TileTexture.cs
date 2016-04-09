using UnityEngine;
using System;
using System.Collections;


public class TileTexture : MonoBehaviour
{

    public Texture2D texture;

    public int tileWidth;
    public int tileHeight;

    public int tilesWide;
    public int tilesHigh;


    public int Width { get { return tilesWide * tileWidth; } }
    public int Height { get { return tilesHigh * tileHeight; } }

	
    public Texture2D GetTexture(int tileCode)
    {
        Rect r = RectForTile(tileCode);
        //print("tileCode: " + tileCode + ", Rect: " + r);

        Color[] pixels = texture.GetPixels((int)r.x, (int)r.y, (int)r.width, (int)r.height);

        Texture2D tex = new Texture2D((int)r.width, (int)r.height);
        tex.name = "Texture_" + tileCode.ToString("X2");
        tex.SetPixels(pixels);
        tex.Apply();

        return tex;
    }

    public Rect RectForTile(int tileCode)
    {
        //if (tileCode == TileMap.InvalidTile) { Debug.LogError("Invalid tileCode"); }

        int tileX = tileCode % tilesWide;
        int tileY = (int)(tileCode / tilesWide);
        //tileY = tilesHigh - tileY - 1;

        int x = tileX * tileWidth;
        int y = tileY * tileHeight;
        y = texture.height - 1 - y - tileHeight;

        return new Rect(x+1, y+1, tileWidth-1, tileHeight-1);
    }

}