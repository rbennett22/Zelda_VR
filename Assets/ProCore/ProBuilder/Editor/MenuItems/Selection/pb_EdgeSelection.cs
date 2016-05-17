using ProBuilder2.Common;
using ProBuilder2.EditorCommon;
using UnityEditor;

namespace ProBuilder2.Actions
{
    public class pb_EdgeSelection : Editor
    {
        [MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Selection/Edge Ring &r", true)]
        [MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Selection/Edge Loop &l", true)]
        public static bool VerifyEdgeSelectAction()
        {
            return pb_Editor.instance != null && pb_Editor.instance.selectedEdgeCount > 0;
        }

        [MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Selection/Edge Ring &r")]
        public static void MenuEdgeRing()
        {
            pb_Menu_Commands.MenuRingSelection(pbUtil.GetComponents<pb_Object>(Selection.transforms));
        }

        [MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Selection/Edge Loop &l")]
        public static void MenuEdgeLoop()
        {
            pb_Menu_Commands.MenuLoopSelection(pbUtil.GetComponents<pb_Object>(Selection.transforms));
        }
    }
}