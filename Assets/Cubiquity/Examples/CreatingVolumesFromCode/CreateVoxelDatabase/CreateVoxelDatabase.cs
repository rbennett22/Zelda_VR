using UnityEngine;

namespace Cubiquity
{
    public class CreateVoxelDatabase : MonoBehaviour
    {
        void Start()
        {
            System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
            Create2DSimplexNoiseVolumeVDB();
            stopwatch.Stop();
            Debug.Log("Created data and VDB in " + stopwatch.ElapsedMilliseconds / 1000 + " seconds");
        }

        // This function creates a spherical volume with ,multiple layers, which can then be used as a planet.
        // We used it (or an earlier version?) to create the Earth and Moon for the the 'solar system' example.
        void CreatePlanetVDB()
        {
            int planetRadius = 60;

            // Randomize the filename incase the file already exists
            System.Random randomIntGenerator = new System.Random();
            int randomInt = randomIntGenerator.Next();
            string saveLocation = Paths.voxelDatabases + "/planet-" + randomInt + ".vdb";

            Region volumeBounds = new Region(-planetRadius, -planetRadius, -planetRadius, planetRadius, planetRadius, planetRadius);
            TerrainVolumeData data = VolumeData.CreateEmptyVolumeData<TerrainVolumeData>(volumeBounds, saveLocation);

            // The numbers below control the thickness of the various layers.
            TerrainVolumeGenerator.GeneratePlanet(data, planetRadius, planetRadius - 1, planetRadius - 10, planetRadius - 35);

            // We need to commit this so that the changes made by the previous,line are actually written
            // to the voxel database. Otherwise they are just kept in temporary storage and will be lost.
            data.CommitChanges();

            Debug.Log("Voxel database has been saved to '" + saveLocation + "'");
        }

        void Create3DSimplexNoiseVolumeVDB()
        {
            // Randomize the filename incase the file already exists
            System.Random randomIntGenerator = new System.Random();
            int randomInt = randomIntGenerator.Next();
            string saveLocation = Paths.voxelDatabases + "/3d-simplex-noise-" + randomInt + ".vdb";

            // The size of the volume we will generate
            int width = 256;
            int height = 256;
            int depth = 256;

            TerrainVolumeData data = VolumeData.CreateEmptyVolumeData<TerrainVolumeData>(new Region(0, 0, 0, width - 1, height - 1, depth - 1), saveLocation);

            // This scale factor comtrols the size of the rocks which are generated.
            float rockScale = 32.0f;
            float invRockScale = 1.0f / rockScale;

            // Let's keep the allocation outside of the loop.
            MaterialSet materialSet = new MaterialSet();

            // Iterate over every voxel of our volume
            for (int z = 0; z < depth; z++)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        // Make sure we don't have anything left in here from the previous voxel
                        materialSet.weights[0] = 0;
                        materialSet.weights[1] = 0;
                        materialSet.weights[2] = 0;

                        // Simplex noise is quite high frequency. We scale the sample position to reduce this.
                        float sampleX = (float)x * invRockScale;
                        float sampleY = (float)y * invRockScale;
                        float sampleZ = (float)z * invRockScale;

                        // Get the noise value for the current position.
                        // Returned value should be in the range -1 to +1.
                        float simplexNoiseValue = SimplexNoise.Noise.Generate(sampleX, sampleY, sampleZ);

                        // Cubiquity renders anything below the threshold as empty and anythng above as solid, but
                        // in general it is easiest if empty space is completly empty and solid space is completly
                        // solid. The exception to this is the region near our surface, where a gentle transition helps
                        // obtain smooth shading. By scaling by a large number and then clamping we achieve this effect
                        // of making most voxels fully solid or fully empty except near the surface..
                        simplexNoiseValue *= 5.0f;
                        simplexNoiseValue = Mathf.Clamp(simplexNoiseValue, -0.5f, 0.5f);

                        // Go back to the range 0.0 to 1.0;
                        simplexNoiseValue += 0.5f;

                        // And then to 0 to 255, ready to convert into a byte.
                        simplexNoiseValue *= 255;

                        // Write the final value value into the first material channel (the one with the rock texture).
                        // The value being written is usually 0 (empty) or 255 (solid) except around the transition.
                        materialSet.weights[0] = (byte)simplexNoiseValue;

                        // We can now write our computed voxel value into the volume.
                        data.SetVoxel(x, y, z, materialSet);
                    }
                }
            }

            // We need to commit this so that the changes made by the previous,line are actually written
            // to the voxel database. Otherwise they are just kept in temporary storage and will be lost.
            data.CommitChanges();

            Debug.Log("Voxel database has been saved to '" + saveLocation + "'");
        }

        void Create2DSimplexNoiseVolumeVDB()
        {
            // Randomize the filename incase the file already exists
            System.Random randomIntGenerator = new System.Random();
            int randomInt = randomIntGenerator.Next();
            string saveLocation = Paths.voxelDatabases + "/2d-simplex-noise-" + randomInt + ".vdb";

            // The size of the volume we will generate
            int width = 256;
            int height = 32;
            int depth = 256;

            TerrainVolumeData data = VolumeData.CreateEmptyVolumeData<TerrainVolumeData>(new Region(0, 0, 0, width - 1, height - 1, depth - 1), saveLocation);

            // This scale factor comtrols the size of the rocks which are generated.
            float rockScale = 32.0f;
            float invRockScale = 1.0f / rockScale;

            // Let's keep the allocation outside of the loop.
            MaterialSet materialSet = new MaterialSet();

            // Iterate over every voxel of our volume
            for (int z = 0; z < depth; z++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Simplex noise is quite high frequency. We scale the sample position to reduce this.
                    float sampleX = (float)x * invRockScale;
                    float sampleZ = (float)z * invRockScale;

                    // Get the noise value for the current position.
                    // Returned value should be in the range -1 to +1.
                    float simplexNoiseValue = SimplexNoise.Noise.Generate(sampleX, sampleZ);

                    simplexNoiseValue += 1.0f;
                    simplexNoiseValue *= 16.0f;

                    for (int y = 0; y < height; y++)
                    {
                        // Make sure we don't have anything left in here from the previous voxel
                        materialSet.weights[0] = 0;
                        materialSet.weights[1] = 0;
                        materialSet.weights[2] = 0;

                        if (y < simplexNoiseValue)
                        {
                            // Write the final value value into the first material channel (the one with the rock texture).
                            // The value being written is usually 0 (empty) or 255 (solid) except around the transition.
                            materialSet.weights[0] = (byte)255;
                        }

                        // We can now write our computed voxel value into the volume.
                        data.SetVoxel(x, y, z, materialSet);
                    }
                }
            }

            // We need to commit this so that the changes made by the previous,line are actually written
            // to the voxel database. Otherwise they are just kept in temporary storage and will be lost.
            data.CommitChanges();

            Debug.Log("Voxel database has been saved to '" + saveLocation + "'");
        }
    }
}