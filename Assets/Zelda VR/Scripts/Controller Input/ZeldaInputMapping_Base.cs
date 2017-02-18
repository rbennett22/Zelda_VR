using UnityEngine;
using Immersio.Utility;
using System;

using Cmd_Trigger = ZeldaInput.Cmd_Trigger;
using Cmd_Bool = ZeldaInput.Cmd_Bool;
using Cmd_Float = ZeldaInput.Cmd_Float;

public abstract class ZeldaInputMapping_Base : MonoBehaviour
{
    public virtual float GetCommand_Float(Cmd_Float cmd)
    {
        throw new NotImplementedException();
    }
    public virtual bool GetCommand_Trigger(Cmd_Trigger cmd)
    {
        throw new NotImplementedException();
    }
    public virtual bool GetCommand_Bool(Cmd_Bool cmd)
    {
        throw new NotImplementedException();
    }
}