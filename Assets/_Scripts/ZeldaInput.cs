using UnityEngine;
using System.Collections.Generic;
using Immersio.Utility;


public class ZeldaInput : Singleton<ZeldaInput>
{

    public enum Axis { MoveHorizontal, MoveVertical, LookHorizontal };
    public enum Button { SwordAttack, UseItemB, Run, Jump, Extra, Start, Select, Pause, L1, R1, L2, R2 };


    //bool GameControllerAvailable { get { return OVRGamepadController.GPC_IsAvailable(); } }
    bool XBoxControllerAvailable { get; set; }


    Dictionary<Axis, string> _zeldaAxisToXBox = new Dictionary<Axis, string>()
    {
        { Axis.MoveHorizontal, "XBox LS Horizontal" },
        { Axis.MoveVertical, "XBox LS Vertical" },
        { Axis.LookHorizontal, "XBox RS Horizontal" }
    };

    /*Dictionary<Axis, OVRGamepadController.Axis> _zeldaAxisToXBox = new Dictionary<Axis, OVRGamepadController.Axis>()
    {
        { Axis.MoveHorizontal, OVRGamepadController.Axis.LeftXAxis },
        { Axis.MoveVertical, OVRGamepadController.Axis.LeftYAxis },
        { Axis.LookHorizontal, OVRGamepadController.Axis.RightXAxis }
    };*/

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

    /*Dictionary<Button, OVRGamepadController.Button> _zeldaButtonToXBox = new Dictionary<Button, OVRGamepadController.Button>()
    {
        { Button.SwordAttack, OVRGamepadController.Button.A },
        { Button.UseItemB, OVRGamepadController.Button.B },
        { Button.Run, OVRGamepadController.Button.LStick },
        { Button.Jump, OVRGamepadController.Button.X },
        { Button.Extra, OVRGamepadController.Button.Y },
        { Button.Start, OVRGamepadController.Button.Start },
        { Button.Select, OVRGamepadController.Button.Back },
        { Button.Pause, OVRGamepadController.Button.Start },
        { Button.L1, OVRGamepadController.Button.L1 },
        { Button.R1, OVRGamepadController.Button.R1 }
        //{ Button.L2, null },
        //{ Button.R2, null },
    };*/


    bool _controllerWasAvailable;
    bool _hasDeterminedContollerAvailablity;


    /*void Update()
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
        //return XBoxControllerAvailable ? OVRGamepadController.GPC_GetAxis((int)axis) : Input.GetAxis(axis.ToString());
        return XBoxControllerAvailable ? Input.GetAxis(_zeldaAxisToXBox[axis].ToString()) : Input.GetAxis(axis.ToString());
    }

    // TODO: GetButton is not available for XBox controller
    bool GetButton_(Button button)
    {
        if (!_hasDeterminedContollerAvailablity) { DetermineControllerAvailability(button); }

        //return XBoxControllerAvailable ? false : Input.GetButton(button.ToString());

        if (XBoxControllerAvailable && _zeldaButtonToXBox[button] == null) { return false; }

        string btnName = XBoxControllerAvailable ? _zeldaButtonToXBox[button] : button.ToString();
        return XBoxControllerAvailable ? Input.GetButton(btnName) : Input.GetButton(btnName);
    }

    bool GetButtonDown_(Button button)
    {
        if (!_hasDeterminedContollerAvailablity) { DetermineControllerAvailability(button); }

        //return XBoxControllerAvailable ? OVRGamepadController.GPC_GetButton((int)_zeldaButtonToXBox[button]) : Input.GetButtonDown(button.ToString());

        if (XBoxControllerAvailable && _zeldaButtonToXBox[button] == null) { return false; }

        string btnName = XBoxControllerAvailable ? _zeldaButtonToXBox[button] : button.ToString();
        return XBoxControllerAvailable ? Input.GetButtonDown(btnName) : Input.GetButtonDown(btnName);
    }
	
    bool GetButtonUp_(Button button)
    {
        if (!_hasDeterminedContollerAvailablity) { DetermineControllerAvailability(button); }

        //return XBoxControllerAvailable ? OVRGamepadController.GPC_GetButton((int)_zeldaButtonToXBox[button]) : Input.GetButtonUp(button.ToString());

        if (XBoxControllerAvailable && _zeldaButtonToXBox[button] == null) { return false; }

        string btnName = XBoxControllerAvailable ? _zeldaButtonToXBox[button] : button.ToString();
        return XBoxControllerAvailable ? Input.GetButtonUp(btnName) : Input.GetButtonUp(btnName);
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

        /*if (!GameControllerAvailable) { return; }
        
        if (Input.GetButton(testButton.ToString()))
        {
            XBoxControllerAvailable = OVRGamepadController.GPC_GetButton((int)_zeldaButtonToXBox[testButton]);
            _hasDeterminedContollerAvailablity = true;
        }*/
    }

}