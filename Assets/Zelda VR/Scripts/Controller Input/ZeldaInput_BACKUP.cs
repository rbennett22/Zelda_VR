using Immersio.Utility;
using System.Collections.Generic;
using UnityEngine;

public class ZeldaInput_BACKUP : Singleton<ZeldaInput_BACKUP>
{
    public enum Axis { MoveHorizontal, MoveVertical, LookHorizontal, MenuNavVertical, Triggers };

    public enum Button { SwordAttack, UseItemB, Run, Jump, Extra, Start, Back, L1, R1 };


    Dictionary<Axis, string> _zeldaAxisToXBox = new Dictionary<Axis, string>()
    {
        { Axis.MoveHorizontal, "XBox LS Horizontal" },
        { Axis.MoveVertical, "XBox LS Vertical" },
        { Axis.LookHorizontal, "XBox RS Horizontal" },
        { Axis.MenuNavVertical, "XBox RS Vertical" },
        { Axis.Triggers, "XBox Triggers" }
    };

    Dictionary<Button, string> _zeldaButtonToXBox = new Dictionary<Button, string>()
    {
        { Button.SwordAttack, "XBox A" },
        { Button.UseItemB, "XBox B" },
        { Button.Run, "XBox LS Button" },
        { Button.Jump, "XBox X" },
        { Button.Extra, "XBox Y" },
        { Button.Start, "XBox START" },
        { Button.Back, "XBox BACK" },
        { Button.L1, "XBox LB" },
        { Button.R1, "XBox RB" }
    };


    float GetAxis_(Axis axis)
    {
        string axisName = _zeldaAxisToXBox[axis];
        float v = (string.IsNullOrEmpty(axisName)) ? 0 : Input.GetAxisRaw(axisName);
        if (v == 0)
        {
            axisName = axis.ToString();
            v = (string.IsNullOrEmpty(axisName)) ? 0 : Input.GetAxisRaw(axisName);
        }

        // TODO
        if (AreAnyTouchControllersActive())
        {
            if (axis == Axis.MoveVertical)
            {
                v = OVRInput.Get(OVRInput.RawAxis2D.LThumbstick).y;
            }
            else if (axis == Axis.MenuNavVertical)
            {
                v = OVRInput.Get(OVRInput.RawAxis2D.RThumbstick).y;
            }
        }

        return v;
    }

    bool GetButton_(Button button)
    {
        string btnName = _zeldaButtonToXBox[button];
        bool b = (string.IsNullOrEmpty(btnName)) ? false : Input.GetButton(btnName);
        if (!b)
        {
            btnName = button.ToString();
            b = (string.IsNullOrEmpty(btnName)) ? false : Input.GetButton(btnName);
        }
        return b;
    }

    bool GetButtonDown_(Button button)
    {
        string btnName = _zeldaButtonToXBox[button];
        bool b = (string.IsNullOrEmpty(btnName)) ? false : Input.GetButtonDown(btnName);
        if (!b)
        {
            btnName = button.ToString();
            b = (string.IsNullOrEmpty(btnName)) ? false : Input.GetButtonDown(btnName);
        }
        return b;
    }

    bool GetButtonUp_(Button button)
    {
        string btnName = _zeldaButtonToXBox[button];
        bool b = (string.IsNullOrEmpty(btnName)) ? false : Input.GetButtonUp(btnName);
        if (!b)
        {
            btnName = button.ToString();
            b = (string.IsNullOrEmpty(btnName)) ? false : Input.GetButtonUp(btnName);
        }
        return b;
    }


    float GetMenuNavHorzAxis_()
    {
        float moveVert = GetAxis(Axis.MoveHorizontal)
            + GetAxis(Axis.LookHorizontal);
        return moveVert;
    }
    float GetMenuNavVertAxis_()
    {
        float moveVert = GetAxis(Axis.MoveVertical)
            + GetAxis(Axis.MenuNavVertical);
        return moveVert;
    }


    bool AreAnyTouchControllersActive_()
    {
        OVRInput.Controller c = OVRInput.GetActiveController();
        return c == OVRInput.Controller.Touch
            || c == OVRInput.Controller.LTouch
            || c == OVRInput.Controller.RTouch;
    }


    #region static

    static public float GetAxis(Axis axis) { return Instance.GetAxis_(axis); }
    static public bool GetButton(Button button) { return Instance.GetButton_(button); }
    static public bool GetButtonDown(Button button) { return Instance.GetButtonDown_(button); }
    static public bool GetButtonUp(Button button) { return Instance.GetButtonUp_(button); }
        
    static public float GetMenuNavHorzAxis() { return Instance.GetMenuNavHorzAxis_(); }
    static public float GetMenuNavVertAxis() { return Instance.GetMenuNavVertAxis_(); }

    static public bool AreAnyTouchControllersActive() { return Instance.AreAnyTouchControllersActive_(); }

    #endregion static
}