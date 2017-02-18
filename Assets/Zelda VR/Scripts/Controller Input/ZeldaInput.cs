using Immersio.Utility;
using System.Collections.Generic;
using UnityEngine;

public class ZeldaInput : Singleton<ZeldaInput>
{
    public enum InputModeEnum
    {
        MouseAndKeyboard,
        XBOX_Gamepad,
        OculusTouch
    }
    InputModeEnum _inputMode;
    public InputModeEnum InputMode_ {
        get { return _inputMode; }
        set
        {
            if (value == _inputMode)
            {
                return;
            }

            InputModeEnum prevMode = _inputMode;
            _inputMode = value;

            OnInputModeChanged(prevMode, _inputMode);
        }
    }
    void OnInputModeChanged(InputModeEnum prevMode, InputModeEnum newMode)
    {
        Destroy(_zeldaInputMapping);

        switch (newMode)
        {
            case InputModeEnum.MouseAndKeyboard:
                _zeldaInputMapping = gameObject.AddComponent<ZeldaInputMapping_MouseKeyboard>();
                break;
            case InputModeEnum.XBOX_Gamepad:
                _zeldaInputMapping = gameObject.AddComponent<ZeldaInputMapping_Xbox>();
                break;
            case InputModeEnum.OculusTouch:
                _zeldaInputMapping = gameObject.AddComponent<ZeldaInputMapping_OculusTouch>();
                break;
            default:
                break;
        }
    }

    ZeldaInputMapping_Base _zeldaInputMapping;


    public enum Cmd_Float
    {
        MoveHorizontal, MoveVertical,
        LookHorizontal,
        MenuNavHorizontal, MenuNavVertical
    };
    public enum Cmd_Trigger
    {
        SwordAttack, UseItemB, Jump,
        ToggleInventory, ToggleOptionsMenu, ToggleDebugOptions,
        MenuNavBack, MenuNavSelect,
        EraseSaveEntry,
        ToggleGodMode, ToggleGhostMode, ToggleFlying
    };
    public enum Cmd_Bool
    {
        Run
    };


    override protected void Awake()
    {
        base.Awake();

        _inputMode = InputModeEnum.OculusTouch;
        _zeldaInputMapping = gameObject.AddComponent<ZeldaInputMapping_OculusTouch>();
    }


    bool GetCommand_Bool_(Cmd_Bool cmd)
    {
        return _zeldaInputMapping.GetCommand_Bool(cmd);
    }
    bool GetCommand_Trigger_(Cmd_Trigger cmd)
    {
        return _zeldaInputMapping.GetCommand_Trigger(cmd);
    }
    float GetCommand_Float_(Cmd_Float cmd)
    {
        return _zeldaInputMapping.GetCommand_Float(cmd);
    }


    bool AreAnyTouchControllersActive_()
    {
        OVRInput.Controller c = OVRInput.GetActiveController();
        return c == OVRInput.Controller.Touch
            || c == OVRInput.Controller.LTouch
            || c == OVRInput.Controller.RTouch;
    }


    #region static

    static public InputModeEnum InputMode { get { return Instance.InputMode_; } set { Instance.InputMode_ = value; } }

    static public bool GetCommand_Bool(Cmd_Bool cmd) { return Instance.GetCommand_Bool_(cmd); }
    static public bool GetCommand_Trigger(Cmd_Trigger cmd) { return Instance.GetCommand_Trigger_(cmd); }
    static public float GetCommand_Float(Cmd_Float cmd) { return Instance.GetCommand_Float_(cmd); }

    static public bool AreAnyTouchControllersActive() { return Instance.AreAnyTouchControllersActive_(); }

    #endregion static
}