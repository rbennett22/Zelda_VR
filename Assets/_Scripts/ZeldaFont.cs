using UnityEngine;
using System.Collections.Generic;
using Immersio.Utility;


public class ZeldaFont : Singleton<ZeldaFont> 
{

    public Texture2D zeldaFontTexture;
    public int tileWidth = 48, tileHeight = 48;
    public int charWidth = 8;
    public int charHeight = 8;



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
        if (string.IsNullOrEmpty(str)) { return null; }

        str = str.ToUpper();

        string[] lines = str.SplitByNewLine();

        int longestLineLength = 0;
        foreach (var line in lines)
        {
            if (line.Length > longestLineLength) { longestLineLength = line.Length; }
        }

        int texWidth = (int)(longestLineLength * charWidth);
        int texHeight = (int)(lines.Length * charHeight);
       
        Texture2D tex = new Texture2D(texWidth, texHeight);

        Color[] clearPixels = new Color[texWidth * texHeight];
        for (int i = 0; i < clearPixels.Length; i++)
        {
            clearPixels[i] = Color.clear;
        }
        tex.SetPixels(clearPixels);


        int lineNum = 0;
        int x = (int)((texWidth - (lines[lineNum].Length * charWidth)) * 0.5f);
        int y = texHeight - charHeight;
        int sampleY = (int)(0.5f * (tileHeight - charHeight));

        Color[] pixels = new Color[charWidth * charHeight];
        
        foreach (char c in str.ToCharArray())
        {
            //print("CHAR: " + c);
            if (c == '\n') 
            {
                lineNum++;
                y -= charHeight;
                x = (int)((texWidth - (lines[lineNum].Length * charWidth)) * 0.5f);
                continue; 
            }
            if (c == ' ') { x += charWidth; continue; }

            int sampleX = GetSampleXForCharCode(c);
            if (sampleX < 0) { continue; }

            pixels = zeldaFontTexture.GetPixels(sampleX, sampleY, charWidth, charHeight);
            tex.SetPixels(x, y, charWidth, charHeight, pixels);

            x += charWidth;
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

        int sampleX = (int)((sampleIndex * tileWidth) + 0.5f * (tileWidth - charWidth));

        return sampleX;
    }
	
}