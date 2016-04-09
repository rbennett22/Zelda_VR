using UnityEngine;

public class GfxHelper
{
    public static Texture CreateBlackTexture()
    {
        return CreateColoredTexture(Color.black);
    }
    public static Texture CreateColoredTexture(Color color)
    {
        Texture2D tex = new Texture2D(1, 1, TextureFormat.ARGB32, false);
        tex.SetPixel(0, 0, color);
        tex.Apply();
        return tex;
    }
}
