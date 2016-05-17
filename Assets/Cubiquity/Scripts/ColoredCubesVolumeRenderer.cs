using UnityEngine;

namespace Cubiquity
{
    [ExecuteInEditMode]
    /// Controls some visual aspects of the colord cubes volume and allows it to be rendered.
    /**
	 * See the base VolumeRenderer class for further details and available properties.
	 */
    public class ColoredCubesVolumeRenderer : VolumeRenderer
    {
        void Awake()
        {
            if (material == null)
            {
                // This shader should be appropriate in most scenarios, and makes a good default.
                material = Instantiate(Resources.Load("Materials/ColoredCubes", typeof(Material))) as Material;
            }
        }
    }
}