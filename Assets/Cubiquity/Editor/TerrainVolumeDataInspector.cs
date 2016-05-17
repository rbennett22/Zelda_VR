using UnityEditor;

namespace Cubiquity
{
    [CustomEditor(typeof(TerrainVolumeData))]
    public class TerrainVolumeDataInspector : VolumeDataInspector
    {
        public override void OnInspectorGUI()
        {
            OnInspectorGUIImpl();
        }
    }
}