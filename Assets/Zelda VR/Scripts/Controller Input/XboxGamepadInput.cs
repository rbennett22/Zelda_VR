using UnityEngine;
using System.Collections.Generic;

public static class XboxGamepadInput
{
    public enum Axis
    {
        LeftStick_H, LeftStick_V,
        RightStick_H, RightStick_V,
        DPad_H, DPad_V,
        Triggers
    }

    public enum Button
    {
        Back, Start,
        A, B, X, Y,
        LeftBumper, RightBumper,
        LeftStick, RightStick
    }


    static Dictionary<Axis, string> _axisToString = new Dictionary<Axis, string>()
    {
        { Axis.LeftStick_H, "XBox LS Horizontal" },
        { Axis.LeftStick_V, "XBox LS Vertical" },
        { Axis.RightStick_H, "XBox RS Horizontal" },
        { Axis.RightStick_V, "XBox RS Vertical" },
        { Axis.DPad_H, "XBox DPad Horizontal" },
        { Axis.DPad_V, "XBox DPad Vertical" },
        { Axis.Triggers, "XBox Triggers" }
    };

    static Dictionary<Button, string> _buttonToString = new Dictionary<Button, string>()
    {
        { Button.Back, "XBox BACK" },
        { Button.Start, "XBox START" },
        { Button.A, "XBox A" },
        { Button.B, "XBox B" },
        { Button.X, "XBox X" },
        { Button.Y, "XBox Y" },
        { Button.LeftBumper, "XBox LB" },
        { Button.RightBumper, "XBox RB" },
        { Button.LeftStick, "XBox LS Button" },
        { Button.RightStick, "XBox RS Button" }
    };


    public static float GetAxis(Axis axis)
    {
        string axisName = _axisToString[axis];
        float v = string.IsNullOrEmpty(axisName) ? 0 : Input.GetAxis(axisName);
        return v;
    }
    public static float GetAxisRaw(Axis axis)
    {
        string axisName = _axisToString[axis];
        float v = string.IsNullOrEmpty(axisName) ? 0 : Input.GetAxisRaw(axisName);
        return v;
    }

    public static bool GetButton(Button button)
    {
        string btnName = _buttonToString[button];
        bool b = (string.IsNullOrEmpty(btnName)) ? false : Input.GetButton(btnName);
        return b;
    }

    public static bool GetButtonDown(Button button)
    {
        string btnName = _buttonToString[button];
        bool b = (string.IsNullOrEmpty(btnName)) ? false : Input.GetButtonDown(btnName);
        return b;
    }

    public static bool GetButtonUp(Button button)
    {
        string btnName = _buttonToString[button];
        bool b = (string.IsNullOrEmpty(btnName)) ? false : Input.GetButtonUp(btnName);
        return b;
    }


    /*// VirtualButton:  Allows an Axis to behave as a Button
    class VirtualButton
    {
        public Axis axis;
        public float threshold; // The threshold value the axis must have to be considered "down" (or "pressed")

        public bool GetButton()
        {
            float f = GetAxisRaw(axis);
            return (threshold < 0) ? (f < threshold) : (f >= threshold);
        }
    }

    // VirtualAxis:  Allows a Button to behave as an Axis
    class VirtualAxis
    {
        public Button btn;
        public float value; // The value the button will return when down

        public float GetAxis()
        {
            bool b = GetButton(btn);
            return b ? value : 0;
        }
    }*/


}
