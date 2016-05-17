using UnityEditor;

namespace Cubiquity
{
    [CustomEditor(typeof(TerrainVolumeCollider))]
    public class TerrainVolumeColliderInspector : VolumeColliderInspector
    {
        public override void OnInspectorGUI()
        {
            OnInspectorGUIImpl();
        }
    }
}