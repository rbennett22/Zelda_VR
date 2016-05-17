using UnityEditor;

namespace Cubiquity
{
    [CustomEditor(typeof(ColoredCubesVolumeData))]
    public class ColoredCubesVolumeDataInspector : VolumeDataInspector
    {
        public override void OnInspectorGUI()
        {
            OnInspectorGUIImpl();
        }
    }
}