using UnityEditor;

namespace Cubiquity
{
    public class CreateEmptyTerrainVolumeDataAssetWizard : ScriptableWizard
    {
        public int width = 128;
        public int height = 32;
        public int depth = 128;

        public bool generateFloor = true;

        void OnWizardCreate()
        {
            TerrainVolumeData data = VolumeDataAsset.CreateEmptyVolumeData<TerrainVolumeData>(new Region(0, 0, 0, width - 1, height - 1, depth - 1));

            if (generateFloor)
            {
                // Create some ground in the terrain so it shows up in the editor.
                // Soil as a base (mat 1) and then a couple of layers of grass (mat 2).
                TerrainVolumeGenerator.GenerateFloor(data, 6, (uint)1, 8, (uint)2);
            }
        }
    }
}