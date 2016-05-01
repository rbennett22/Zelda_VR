﻿using UnityEngine;
using System.Collections.Generic;
using Immersio.Utility;

public class ZeldaInput : Singleton<ZeldaInput>
{

    public enum Axis { MoveHorizontal, MoveVertical, LookHorizontal };
    public enum Button { SwordAttack, UseItemB, Run, Jump, Extra, Start, Select, Pause, L1, R1, L2, R2 };


    public bool XBoxControllerAvailable { get; set; }


    Dictionary<Axis, string> _zeldaAxisToXBox = new Dictionary<Axis, string>()
    {
        { Axis.MoveHorizontal, "XBox LS Horizontal" },
        { Axis.MoveVertical, "XBox LS Vertical" },
        { Axis.LookHorizontal, "XBox RS Horizontal" }
    };

    Dictionary<Button, string> _zeldaButtonToXBox = new Dictionary<Button, string>()
    {
        { Button.SwordAttack, "XBox A" },
        { Button.UseItemB, "XBox B" },
        { Button.Run, "XBox LS Button" },
        { Button.Jump, "XBox X" },
        { Button.Extra, "XBox Y" },
        { Button.Start, "XBox START" },
        { Button.Select, "XBox BACK" },
        { Button.Pause, "XBox START" },
        { Button.L1, "XBox LB" },
        { Button.R1, "XBox RB" },
        { Button.L2, null },
        { Button.R2, null }
    };

    
    bool _hasDeterminedContollerAvailablity;

    /*bool _controllerWasAvailable;
    void Update()
    {
        print("XBoxControllerAvailable: " + XBoxControllerAvailable);
        if (GameControllerAvailable != _controllerWasAvailable)
        {
            _hasDeterminedContollerAvailablity = false;
        }
        _controllerWasAvailable = GameControllerAvailable;
    }*/


    float GetAxis_(Axis axis)
    {
        string axisName = XBoxControllerAvailable ? _zeldaAxisToXBox[axis] : axis.ToString();
        return (string.IsNullOrEmpty(axisName)) ? 0 : Input.GetAxis(axisName);
    }

    // TODO: GetButton is not available for XBox controller
    bool GetButton_(Button button)
    {
        if (!_hasDeterminedContollerAvailablity) { DetermineControllerAvailability(button); }

        string btnName = XBoxControllerAvailable ? _zeldaButtonToXBox[button] : button.ToString();
        return (string.IsNullOrEmpty(btnName)) ? false : Input.GetButton(btnName);
    }

    bool GetButtonDown_(Button button)
    {
        if (!_hasDeterminedContollerAvailablity) { DetermineControllerAvailability(button); }

        string btnName = XBoxControllerAvailable ? _zeldaButtonToXBox[button] : button.ToString();
        return (string.IsNullOrEmpty(btnName)) ? false : Input.GetButtonDown(btnName);
    }
	
    bool GetButtonUp_(Button button)
    {
        if (!_hasDeterminedContollerAvailablity) { DetermineControllerAvailability(button); }

        string btnName = XBoxControllerAvailable ? _zeldaButtonToXBox[button] : button.ToString();
        return (string.IsNullOrEmpty(btnName)) ? false : Input.GetButtonUp(btnName);
    }


    static public float GetAxis(Axis axis) { return Instance.GetAxis_(axis); }
    static public bool GetButton(Button button) { return Instance.GetButton_(button); }
    static public bool GetButtonDown(Button button) { return Instance.GetButtonDown_(button); }
    static public bool GetButtonUp(Button button) { return Instance.GetButtonUp_(button); }


    void DetermineControllerAvailability(Button testButton)
    {
        XBoxControllerAvailable = false;

        string[] names = Input.GetJoystickNames();
        if (names.Length > 0)
        {
            foreach (var name in names)
            {
                if (name.Contains("XBOX"))
                {
                    XBoxControllerAvailable = true;
                    break;
                }
            } 
        }

        _hasDeterminedContollerAvailablity = true;
    }
}