using UnityEngine;
using System.Collections.Generic;

using Cmd_Trigger = ZeldaInput.Cmd_Trigger;
using Cmd_Bool = ZeldaInput.Cmd_Bool;
using Cmd_Float = ZeldaInput.Cmd_Float;

public class ZeldaInputMapping_MouseKeyboard : ZeldaInputMapping_Base 
{
    public enum Axis { MoveHorizontal, MoveVertical, LookHorizontal, MenuNavVertical, Triggers };

    public enum Button { SwordAttack, UseItemB, Run, Jump, Extra, Start, Back, L1, R1 };


    Dictionary<Cmd_Float, Axis[]> _cmdFloatToAxis = new Dictionary<Cmd_Float, Axis[]>()
    {
        { Cmd_Float.MoveHorizontal, new Axis[] { Axis.MoveHorizontal } },
        { Cmd_Float.MoveVertical, new Axis[] { Axis.MoveVertical } },
        { Cmd_Float.LookHorizontal, new Axis[] { Axis.LookHorizontal } },
        { Cmd_Float.MenuNavHorizontal, new Axis[] { Axis.LookHorizontal, Axis.MoveHorizontal } },
        { Cmd_Float.MenuNavVertical, new Axis[] { Axis.MenuNavVertical, Axis.MoveVertical } },
        { Cmd_Float.Fly, new Axis[] { Axis.Triggers } },
    };
    Dictionary<Cmd_Trigger, Button[]> _cmdTriggerToButton = new Dictionary<Cmd_Trigger, Button[]>()
    {
        { Cmd_Trigger.SwordAttack, new Button[] { Button.SwordAttack } },
        { Cmd_Trigger.UseItemB, new Button[] { Button.UseItemB } },
        { Cmd_Trigger.Jump, new Button[] { Button.Jump } },
        { Cmd_Trigger.ToggleInventory, new Button[] { Button.Start } },
        { Cmd_Trigger.ToggleOptionsMenu, new Button[] { Button.Back } },
        { Cmd_Trigger.ToggleDebugOptions, new Button[] { Button.L1 } },
        { Cmd_Trigger.MenuNavBack, new Button[] { Button.UseItemB } },
        { Cmd_Trigger.MenuNavSelect, new Button[] { Button.SwordAttack } },
        { Cmd_Trigger.EraseSaveEntry, new Button[] { Button.Jump } },
        { Cmd_Trigger.ToggleGodMode, new Button[] { Button.L1 } },
        { Cmd_Trigger.ToggleGhostMode, new Button[] { Button.R1 } },
        { Cmd_Trigger.ToggleFlying, new Button[] { Button.Extra } },
    };
    Dictionary<Cmd_Bool, Button[]> _cmdBoolToButton = new Dictionary<Cmd_Bool, Button[]>()
    {
        { Cmd_Bool.Run, new Button[] { Button.Run } },
    };

    public override float GetCommand_Float(Cmd_Float cmd)
    {
        float value = 0.0f;

        Axis[] axes = _cmdFloatToAxis[cmd];

        foreach (var axis in axes)
        {
            string axisName = axis.ToString();
            if (string.IsNullOrEmpty(axisName))
            {
                continue;
            }

            value += Input.GetAxisRaw(axisName);
        }

        return Mathf.Clamp(value, -1f, 1f);
    }

    public override bool GetCommand_Trigger(Cmd_Trigger cmd)
    {
        Button[] buttons = _cmdTriggerToButton[cmd];

        foreach (var btn in buttons)
        {
            string btnName = btn.ToString();
            if (string.IsNullOrEmpty(btnName))
            {
                continue;
            }

            if (Input.GetButtonDown(btnName))       // TODO: what if button up     
            {
                return true;
            }
        }

        return false;
    }

    public override bool GetCommand_Bool(Cmd_Bool cmd)
    {
        Button[] buttons = _cmdBoolToButton[cmd];

        foreach (var btn in buttons)
        {
            string btnName = btn.ToString();
            if (string.IsNullOrEmpty(btnName))
            {
                continue;
            }

            if (Input.GetButton(btnName))
            {
                return true;
            }
        }

        return false;
    }
}
