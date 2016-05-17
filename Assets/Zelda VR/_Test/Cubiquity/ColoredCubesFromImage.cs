using Cubiquity;
using UnityEngine;

[ExecuteInEditMode]
public class ColoredCubesFromImage : MonoBehaviour
{
    void Start()
    {
        Texture2D image = Resources.Load("Images/OW_TileMap_ORIG") as Texture2D;

        int width = image.width;
        int height = 16;
        int depth = image.height;

        Region region = new Region(0, 0, 0, width - 1, height - 1, depth - 1);
        ColoredCubesVolumeData data = VolumeData.CreateEmptyVolumeData<ColoredCubesVolumeData>(region);

        ColoredCubesVolume coloredCubesVolume = gameObject.GetComponent<ColoredCubesVolume>();
        coloredCubesVolume.data = data;

        for (int z = 0; z < depth; z++)
        {
            for (int x = 0; x < width; x++)
            {
                Color color = image.GetPixel(x, z);
                QuantizedColor qColor = new QuantizedColor(
                    (byte)(color.r * 255),
                    (byte)(color.g * 255),
                    (byte)(color.b * 255),
                    (byte)(color.a * 255));

                /*int tileSize = 1;
				int tileXOffset = 0;
				int tileZOffset = 0;
				int tileXPos = (x + tileXOffset) / tileSize;
				int tileZPos = (z + tileZOffset) / tileSize;*/

                data.SetVoxel(x, 0, z, qColor);

                //for(int y = height-1; y > 0; y--) { }
            }
        }
    }
}