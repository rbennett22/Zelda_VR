using UnityEngine;

public static class GfxHelper
{
    public static Texture CreateUnitTexture(Color color, TextureFormat format = TextureFormat.ARGB32)
    {
        Texture2D tex = new Texture2D(1, 1, format, false);
        tex.SetPixel(0, 0, color);
        tex.Apply();
        return tex;
    }


    #region Texture2D Extensions

    public static void SetColorForArea(this Texture2D tex, Rect block, Color color)
    {
        tex.SetColorForArea((int)block.xMin, (int)block.yMin, (int)block.width, (int)block.height, color);
    }
    public static void SetColorForArea(this Texture2D tex, int x, int y, int blockWidth, int blockHeight, Color color)
    {
        Color[] colors = new Color[blockWidth * blockHeight];
        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = color;
        }
        tex.SetPixels(x, y, blockWidth, blockHeight, colors);
    }
    public static void Clear(this Texture2D tex)
    {
        tex.Clear(Color.clear);
    }
    public static void Clear(this Texture2D tex, Color color)
    {
        tex.SetColorForArea(0, 0, tex.width, tex.height, color);
    }

    #endregion Texture2D Extensions
}