using UnityEngine;


public class GamepadTest : MonoBehaviour
{
    public bool printJoysticks, printAxes, printButtons;


    void OnGUI()
    {
        if (printJoysticks) { Joysticks_GUI(); }
        if (printAxes) { Axes_GUI(); }
        if (printButtons) { Buttons_GUI(); }
    }
    void Joysticks_GUI()
    {
        GUILayout.Label(GetString_Joysticks());
    }
    void Axes_GUI()
    {
        //GUILayout.Label(GetString_Axes_JS());
        GUILayout.Label(GetString_Axes_XBOX());
    }
    void Buttons_GUI()
    {
        //GUILayout.Label(GetString_Buttons_JS());
        GUILayout.Label(GetString_Buttons_XBOX());
    }


    string GetString_Joysticks()
    {
        int i = 0;
        string s = " --- Joysticks ---\n";
        foreach (string joyName in Input.GetJoystickNames())
        {
            s += i.ToString() + ":" + joyName + "\n";
            i++;
        }
        return s;
    }

    string GetString_Axes_XBOX()
    {
        string s = " --- Axes (XBOX) ---\n";
        s += string.Format("LS H:{0:F3}  LS V:{1:F3}  RS H:{2:F3}  RS V:{3:F3}  DPad H:{4:F3}  DPad V:{5:F3}  Triggers:{6:F3}",
            Input.GetAxis("XBox LS Horizontal"), Input.GetAxis("XBox LS Vertical"),
            Input.GetAxis("XBox RS Horizontal"), Input.GetAxis("XBox RS Vertical"),
            Input.GetAxis("XBox DPad Horizontal"), Input.GetAxis("XBox DPad Vertical"),
            Input.GetAxis("XBox Triggers"));
        return s;
    }
    string GetString_Buttons_XBOX()
    {
        string s = " --- Buttons (XBOX) ---\n";
        s += string.Format("A:{0}  B:{1}  X:{2}  Y:{3}  LB:{4}  RB:{5}  LS:{6}  RS:{7}  BACK:{8}  START:{9}",
            Input.GetAxis("XBox A"), Input.GetAxis("XBox B"),
            Input.GetAxis("XBox X"), Input.GetAxis("XBox Y"),
            Input.GetAxis("XBox LB"), Input.GetAxis("XBox RB"),
            Input.GetAxis("XBox LS Button"), Input.GetAxis("XBox RS Button"),
            Input.GetAxis("XBox BACK"), Input.GetAxis("XBox START"));
        return s;
    }

    string GetString_Axes_JS()
    {
        string s = " --- Axes (JS) ---\n";
        s += string.Format("A1:{0:F3}  A2:{1:F3}  A3:{2:F3}  A4:{3:F3}  A5:{4:F3}  A6:{5:F3}  A7:{6:F3}  A8:{7:F3}  A9:{8:F3}  A10:{9:F3}",
            Input.GetAxis("JS_A1"), Input.GetAxis("JS_A2"),
            Input.GetAxis("JS_A3"), Input.GetAxis("JS_A4"),
            Input.GetAxis("JS_A5"), Input.GetAxis("JS_A6"),
            Input.GetAxis("JS_A7"), Input.GetAxis("JS_A8"),
            Input.GetAxis("JS_A9"), Input.GetAxis("JS_A10"));
        return s;
    }
    string GetString_Buttons_JS()
    {
        string s = " --- Buttons (JS) ---\n";
        s += string.Format("0:{0}  1:{1}  2:{2}  3:{3}  4:{4}  5:{5}  6:{6}  7:{7}  8:{8}  9:{9}  10:{10}  11:{11}",
            Input.GetButton("JS_B0"), Input.GetButton("JS_B1"),
            Input.GetButton("JS_B2"), Input.GetButton("JS_B3"),
            Input.GetButton("JS_B4"), Input.GetButton("JS_B5"),
            Input.GetButton("JS_B6"), Input.GetButton("JS_B7"),
            Input.GetButton("JS_B8"), Input.GetButton("JS_B9"),
            Input.GetButton("JS_B10"), Input.GetButton("JS_B11"));
        return s;
    }
}