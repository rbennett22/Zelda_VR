using UnityEngine;

namespace Cubiquity
{
    [ExecuteInEditMode]
    /// Controls some visual aspects of the terrain volume and allows it to be rendered.
    /**
	 * See the base VolumeRenderer class for further details and available properties.
	 */
    public class TerrainVolumeRenderer : VolumeRenderer
    {
        void Awake()
        {
            if (material == null)
            {
                // Triplanar textuing seems like a good default material for the terrain volume.
                material = Instantiate(Resources.Load("Materials/Triplanar", typeof(Material))) as Material;
            }
        }
    }
}