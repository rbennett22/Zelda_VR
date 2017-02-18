using UnityEngine;
using System.Collections.Generic;

using Cmd_Trigger = ZeldaInput.Cmd_Trigger;
using Cmd_Bool = ZeldaInput.Cmd_Bool;
using Cmd_Float = ZeldaInput.Cmd_Float;

public class ZeldaInputMapping_OculusTouch : ZeldaInputMapping_Xbox
{
    /* // TODO
    class InputControl
    {
        public Button[] buttons = { };
        public Axis[] axes = { };
    }


    #region Input Control Settings 

    Dictionary<Cmd_Trigger, InputControl> _cmdTriggerToInputControl = new Dictionary<Cmd_Trigger, InputControl>()
    {
        { Cmd_Trigger.SwordAttack, new InputControl {
                buttons = new Button[] { Button.A } }
        },
        { Cmd_Trigger.UseItemB, new InputControl {
                buttons = new Button[] { Button.B } }
        },
        { Cmd_Trigger.Jump, new InputControl {
                buttons = new Button[] { Button.X } }
        },
        { Cmd_Trigger.ToggleInventory, new InputControl {
                buttons = new Button[] { Button.Start } }
        },
        { Cmd_Trigger.ToggleOptionsMenu, new InputControl {
                buttons = new Button[] { Button.Back } }
        },
        { Cmd_Trigger.ToggleDebugOptions, new InputControl {
                buttons = new Button[] { Button.LeftBumper } }
        },
        { Cmd_Trigger.MenuNavBack, new InputControl {
                buttons = new Button[] { Button.B } }
        },
        { Cmd_Trigger.MenuNavSelect, new InputControl {
                buttons = new Button[] { Button.A } }
        },
        { Cmd_Trigger.EraseSaveEntry, new InputControl {
                buttons = new Button[] { Button.X } }
        },
        { Cmd_Trigger.ToggleGodMode, new InputControl {
                //axes = new Axis[] { Axis.Triggers },
                buttons = new Button[] { Button.LeftBumper } }
        },
        { Cmd_Trigger.ToggleGhostMode, new InputControl {
                buttons = new Button[] { Button.RightBumper } }
        },
        { Cmd_Trigger.ToggleFlying, new InputControl {
                buttons = new Button[] { Button.Y } }
        },
    };

    Dictionary<Cmd_Bool, InputControl> _cmdBoolToInputControl = new Dictionary<Cmd_Bool, InputControl>()
    {
        { Cmd_Bool.Run, new InputControl {
                buttons = new Button[] { Button.LeftStick } }
        },
    };

    Dictionary<Cmd_Float, InputControl> _cmdFloatToInputControl = new Dictionary<Cmd_Float, InputControl>()
    {
        { Cmd_Float.MoveHorizontal, new InputControl {
                axes = new Axis[] { Axis.LeftStick_H, Axis.DPad_H } }
        },
        { Cmd_Float.MoveVertical, new InputControl {
                axes = new Axis[] { Axis.LeftStick_V, Axis.DPad_V } }
        },
        { Cmd_Float.LookHorizontal, new InputControl {
                axes = new Axis[] { Axis.RightStick_H } }
        },
        { Cmd_Float.MenuNavHorizontal, new InputControl {
                axes = new Axis[] { Axis.RightStick_H, Axis.LeftStick_H, Axis.DPad_H } }
        },
        { Cmd_Float.MenuNavVertical, new InputControl {
                axes = new Axis[] { Axis.RightStick_V, Axis.LeftStick_V, Axis.DPad_V } }
        },
    };

    #endregion Input Control Settings


    public override bool GetCommand_Trigger(Cmd_Trigger cmd)
    {
        InputControl input = _cmdTriggerToInputControl[cmd];

        // Buttons
        foreach (var btn in input.buttons)
        {
            if (XboxGamepadInput.GetButtonDown(btn))        // TODO: what if button up
            {
                return true;
            }
        }

        return false;
    }
    public override bool GetCommand_Bool(Cmd_Bool cmd)
    {
        InputControl input = _cmdBoolToInputControl[cmd];

        // Buttons
        foreach (var btn in input.buttons)
        {
            if (XboxGamepadInput.GetButton(btn))
            {
                return true;
            }
        }

        return false;
    }
    public override float GetCommand_Float(Cmd_Float cmd)
    {
        float value = 0.0f;

        InputControl input = _cmdFloatToInputControl[cmd];

        // Axes
        foreach (var axis in input.axes)
        {
            value += XboxGamepadInput.GetAxisRaw(axis);
        }

        return Mathf.Clamp(value, -1f, 1f);
    }
*/
}
