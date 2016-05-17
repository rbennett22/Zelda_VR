using Immersio.Utility;
using System.Collections.Generic;
using UnityEngine;

public class ZeldaFont : Singleton<ZeldaFont>
{
    const int TILE_WIDTH = 48, TILE_HEIGHT = 48;
    const int CHAR_WIDTH = 24, CHAR_HEIGHT = 24;

    const string ZELDA_FONT_TEXTURE_PREFAB_PATH = "zeldaFontTexture";


    Texture2D _fontTexture;
    public Texture2D FontTexture { get { return _fontTexture ?? (_fontTexture = LoadZeldaFontTexture()); } }
    Texture2D LoadZeldaFontTexture()
    {
        Texture2D t = Resources.Load<Texture2D>(ZELDA_FONT_TEXTURE_PREFAB_PATH);
        if (t == null)
        {
            Debug.LogWarning("Texture2D asset not found in Resources: " + ZELDA_FONT_TEXTURE_PREFAB_PATH);
            return null;
        }
        return t;
    }


    Dictionary<char, int> _charToIndex = new Dictionary<char, int>()
    {
        { ',', 36 },
        { '!', 37 },
        { '\'', 38 },
        { '&', 39 },
        { '.', 40 },
        { '"', 41 },
        { '?', 42 },
        { '-', 43 }
    };


    public Texture2D TextureForCharacter(char c)
    {
        return TextureForString(c.ToString());
    }

    public Texture2D TextureForString(string str)
    {
        if (FontTexture == null)
        {
            return null;
        }

        if (string.IsNullOrEmpty(str)) { return null; }

        str = str.ToUpper();

        string[] lines = str.SplitByNewLine();

        int longestLineLength = 0;
        foreach (var line in lines)
        {
            if (line.Length > longestLineLength) { longestLineLength = line.Length; }
        }

        int texWidth = (int)(longestLineLength * CHAR_WIDTH);
        int texHeight = (int)(lines.Length * CHAR_HEIGHT);

        Texture2D tex = new Texture2D(texWidth, texHeight);

        Color[] clearPixels = new Color[texWidth * texHeight];
        for (int i = 0; i < clearPixels.Length; i++)
        {
            clearPixels[i] = Color.clear;
        }
        tex.SetPixels(clearPixels);


        int lineNum = 0;
        int x = (int)((texWidth - (lines[lineNum].Length * CHAR_WIDTH)) * 0.5f);
        int y = texHeight - CHAR_HEIGHT;
        int sampleY = (int)(0.5f * (TILE_HEIGHT - CHAR_HEIGHT));

        Color[] pixels = new Color[CHAR_WIDTH * CHAR_HEIGHT];

        foreach (char c in str.ToCharArray())
        {
            //print("CHAR: " + c);
            if (c == '\n')
            {
                lineNum++;
                y -= CHAR_HEIGHT;
                x = (int)((texWidth - (lines[lineNum].Length * CHAR_WIDTH)) * 0.5f);
                continue;
            }
            if (c == ' ') { x += CHAR_WIDTH; continue; }

            int sampleX = GetSampleXForCharCode(c);
            if (sampleX < 0) { continue; }

            pixels = FontTexture.GetPixels(sampleX, sampleY, CHAR_WIDTH, CHAR_HEIGHT);
            tex.SetPixels(x, y, CHAR_WIDTH, CHAR_HEIGHT, pixels);

            x += CHAR_WIDTH;
        }

        tex.wrapMode = TextureWrapMode.Clamp;
        tex.Apply();
        return tex;
    }


    int GetSampleXForCharCode(char c)
    {
        int charCode = (int)c;
        int sampleIndex;
        if (charCode >= (int)('0') && charCode <= (int)('9'))
        {
            sampleIndex = charCode - (int)('0');
        }
        else if (charCode >= (int)('A') && charCode <= (int)('Z'))
        {
            sampleIndex = charCode - (int)('A') + 10;
        }
        else if (_charToIndex.ContainsKey(c))
        {
            sampleIndex = _charToIndex[c];
        }
        else
        {
            return -1;
        }

        int sampleX = (int)((sampleIndex * TILE_WIDTH) + 0.5f * (TILE_WIDTH - CHAR_WIDTH));

        return sampleX;
    }
}