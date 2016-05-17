using UnityEditor;

namespace Cubiquity
{
    public class CreateEmptyColoredCubesVolumeDataAssetWizard : ScriptableWizard
    {
        public int width = 256;
        public int height = 32;
        public int depth = 256;

        public bool generateFloor = true;

        void OnWizardCreate()
        {
            ColoredCubesVolumeData data = VolumeDataAsset.CreateEmptyVolumeData<ColoredCubesVolumeData>(new Region(0, 0, 0, width - 1, height - 1, depth - 1));

            if (generateFloor)
            {
                // Create a floor so the volume data is actually visible in the editor.
                int floorThickness = 8;
                QuantizedColor floorColor = new QuantizedColor(192, 192, 192, 255);

                for (int z = 0; z <= depth - 1; z++)
                {
                    for (int y = 0; y < floorThickness; y++)
                    {
                        for (int x = 0; x <= width - 1; x++)
                        {
                            data.SetVoxel(x, y, z, floorColor);
                        }
                    }
                }
            }
        }
    }
}