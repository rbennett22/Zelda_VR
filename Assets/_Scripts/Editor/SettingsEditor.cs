using UnityEngine;
using UnityEditor;

public class Menu
{
    const string MENU_NAME = "ZeldaVR";


    [MenuItem(MENU_NAME + "/Settings")]
    static void SelectSettings()
    {
        Selection.activeObject = ZeldaVRSettings.Instance;
    }
        
    /*[CustomEditor(typeof(ZeldaVRSettings))]
    public class ESettingsObject : Editor
    {
        ZeldaVRSettings _script;
        void OnEnable()
        {
            _script = (ZeldaVRSettings)target;
        }

        public override void OnInspectorGUI()
        {
            //
            base.OnInspectorGUI();
        }
    }*/
}