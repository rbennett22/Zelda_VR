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


    public static void StereoDrawTexture(Rect rect, ref Texture image, Color color, bool withStretch = true)
    {/*
        //float s = withStretch ? stretch : 1;
        float s = 1;

        int x = (int)(_inventoryArea.xMin + rect.x * s);
        int y = (int)(_inventoryArea.yMin + rect.y * s);
        int w = (int)(rect.width * s);
        int h = (int)(rect.height * s);

        GuiHelper_StereoDrawTexture(x, y, w, h, ref image, color);*/
    }
    public static void StereoDrawTexture(Vector2 pos, ref Texture image, Color color, bool withStretch = true)
    {/*
        //float s = withStretch ? stretch : 1;
        float s = 1;

        int x = (int)(_inventoryArea.xMin + pos.x * s);
        int y = (int)(_inventoryArea.yMin + pos.y * s);
        int w = (int)(image.width * s);
        int h = (int)(image.height * s);

        GuiHelper_StereoDrawTexture(x, y, w, h, ref image, color);*/
    }

    /*static void GuiHelper_StereoDrawTexture(int x0, int y0, int width, int height, ref Texture image, Color color, bool withStretch = true)
    {
        Texture2D tex = new Texture2D(width, height);

        for (int x = 0; x < width; x++) 
        {
            for (int y = 0; y < height; y++)
            {
                Color col = Color.white;
                tex.SetPixel(x, y, col);
            }
        }
    }*/

    /*Rect CalcInventoryArea()
    {
        Rect r = gameplayHUD.RenderArea;
        float h = BgImage.height;
        float w = BgImage.width; //r.width;
        return new Rect(r.x, r.y - h, w, h);
    }*/
}
