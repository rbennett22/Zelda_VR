using UnityEngine;
using System;

public class InputTest : MonoBehaviour
{
    public bool printJoysticks = true, printGeneric = true, printXBox = true;
    public bool printAxesRaw;


    void OnGUI()
    {
        if (printJoysticks) { ListAllJoysticks_GUI(); }
        if (printGeneric) { GenericInputs_GUI(); }
        if (printXBox) { XBOX_GUI(); }
    }
    void ListAllJoysticks_GUI()
    {
        GUILayout.Label(GetString_Joysticks());
    }
    void GenericInputs_GUI()
    {
        GUILayout.Label(GetString_Buttons_JS());
        GUILayout.Label(GetString_Axes_JS());
        if (printAxesRaw)
        {
            GUILayout.Label(GetString_AxesRaw_JS());
        }
    }
    void XBOX_GUI()
    {
        GUILayout.Label(GetString_Buttons_XBOX());
        GUILayout.Label(GetString_Axes_XBOX());
        if (printAxesRaw)
        {
            GUILayout.Label(GetString_AxesRaw_XBOX());
        }
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
    string GetString_AxesRaw_JS()
    {
        string s = " --- Axes (JS) ---\n";
        s += string.Format("A1:{0:F3}  A2:{1:F3}  A3:{2:F3}  A4:{3:F3}  A5:{4:F3}  A6:{5:F3}  A7:{6:F3}  A8:{7:F3}  A9:{8:F3}  A10:{9:F3}",
            Input.GetAxisRaw("JS_A1"), Input.GetAxisRaw("JS_A2"),
            Input.GetAxisRaw("JS_A3"), Input.GetAxisRaw("JS_A4"),
            Input.GetAxisRaw("JS_A5"), Input.GetAxisRaw("JS_A6"),
            Input.GetAxisRaw("JS_A7"), Input.GetAxisRaw("JS_A8"),
            Input.GetAxisRaw("JS_A9"), Input.GetAxisRaw("JS_A10"));
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

    string GetString_Axes_XBOX()
    {
        string s = " --- Axes (XBOX) ---\n";
        foreach (XboxGamepadInput.Axis axis in Enum.GetValues(typeof(XboxGamepadInput.Axis)))
        {
            s += axis.ToString() + "=" + XboxGamepadInput.GetAxis(axis).ToString("F3") + "  ";
        }
        return s;
    }
    string GetString_AxesRaw_XBOX()
    {
        string s = " --- AxesRaw (XBOX) ---\n";
        foreach (XboxGamepadInput.Axis axis in Enum.GetValues(typeof(XboxGamepadInput.Axis)))
        {
            s += axis.ToString() + "=" + XboxGamepadInput.GetAxisRaw(axis).ToString("F3") + "  ";
        }
        return s;
    }
    string GetString_Buttons_XBOX()
    {
        string s = " --- Buttons (XBOX) ---\n";
        foreach (XboxGamepadInput.Button b in Enum.GetValues(typeof(XboxGamepadInput.Button)))
        {
            s += b.ToString() + "=" + XboxGamepadInput.GetButton(b) + "  ";
        }
        return s;
    }
}