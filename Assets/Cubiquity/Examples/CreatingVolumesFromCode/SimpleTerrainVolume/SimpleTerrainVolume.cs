using Cubiquity;
using UnityEngine;

public class SimpleTerrainVolume : MonoBehaviour
{
    // Use this for initialization
    void Start()
    {
        // Dimensions of our volume.
        int width = 64;
        int height = 64;
        int depth = 64;

        TerrainVolumeData volumeData = VolumeData.CreateEmptyVolumeData<TerrainVolumeData>(new Region(0, 0, 0, width - 1, height - 1, depth - 1));

        float noiseScale = 32.0f;
        float invNoiseScale = 1.0f / noiseScale;

        // Let's keep the allocation outside of the loop.
        MaterialSet materialSet = new MaterialSet();

        // Iterate over each voxel and assign a value to it
        for (int z = 0; z < depth; z++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Simplex noise is quite high frequency. We scale the sample position to reduce this.
                    float sampleX = (float)x * invNoiseScale;
                    float sampleY = (float)y * invNoiseScale;
                    float sampleZ = (float)z * invNoiseScale;

                    // Get the noise value for the current position.
                    // Returned value should be in the range -1 to +1.
                    float simplexNoiseValue = SimplexNoise.Noise.Generate(sampleX, sampleY, sampleZ);

                    // Cubiquity material weights need to be in the range 0 - 255.
                    simplexNoiseValue += 1.0f; // Now it's 0.0 to 2.0
                    simplexNoiseValue *= 127.5f; // Now it's 0.0 to 255.0

                    materialSet.weights[0] = (byte)simplexNoiseValue;

                    // We can now write our computed voxel value into the volume.
                    volumeData.SetVoxel(x, y, z, materialSet);
                }
            }
        }

        //Add the required volume component.
        TerrainVolume terrainVolume = gameObject.AddComponent<TerrainVolume>();

        // Set the provided data.
        terrainVolume.data = volumeData;

        // Add the renderer
        gameObject.AddComponent<TerrainVolumeRenderer>();
    }
}