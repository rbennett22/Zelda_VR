using Immersio.Utility;
using System.Collections.Generic;
using UnityEngine;

public class ZeldaInput : Singleton<ZeldaInput>
{
    public enum Axis { MoveHorizontal, MoveVertical, LookHorizontal, Triggers };

    public enum Button { SwordAttack, UseItemB, Run, Jump, Extra, Start, Back, L1, R1 };


    public bool XBoxControllerAvailable { get; set; }


    Dictionary<Axis, string> _zeldaAxisToXBox = new Dictionary<Axis, string>()
    {
        { Axis.MoveHorizontal, "XBox LS Horizontal" },
        { Axis.MoveVertical, "XBox LS Vertical" },
        { Axis.LookHorizontal, "XBox RS Horizontal" },
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


    static public float GetAxis(Axis axis) { return Instance.GetAxis_(axis); }
    static public bool GetButton(Button button) { return Instance.GetButton_(button); }
    static public bool GetButtonDown(Button button) { return Instance.GetButtonDown_(button); }
    static public bool GetButtonUp(Button button) { return Instance.GetButtonUp_(button); }
}