using UnityEngine;
using System;

using Cmd_Trigger = ZeldaInput.Cmd_Trigger;
using Cmd_Bool = ZeldaInput.Cmd_Bool;
using Cmd_Float = ZeldaInput.Cmd_Float;

public class ZeldaInputTest : MonoBehaviour
{
    const string SEPERATOR = "\n";


    string _cmdTriggerOutput, _cmdBoolOutput, _cmdFloatOutput;


    float[] _triggerTimeStamps;
    float _triggerDisplayDuration = 0.25f;


    void Awake()
    {
        _triggerTimeStamps = new float[Enum.GetValues(typeof(Cmd_Trigger)).Length];
    }


    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Alpha0))
        {
            SwitchInputMode();
        }

        _cmdTriggerOutput = GetString_CmdTrigger();
        _cmdBoolOutput = GetString_CmdBool();
        _cmdFloatOutput = GetString_CmdFloat();

        int i = 0;
        foreach (Cmd_Trigger t in Enum.GetValues(typeof(Cmd_Trigger)))
        {
            if(ZeldaInput.GetCommand_Trigger(t))
            {
                _triggerTimeStamps[i] = _triggerDisplayDuration;
            }
            else
            {
                _triggerTimeStamps[i] -= Time.deltaTime;
                _triggerTimeStamps[i] = Mathf.Max(_triggerTimeStamps[i], 0);
            }
            i++;
        }
    }

    void SwitchInputMode()
    {
        switch (ZeldaInput.InputMode)
        {
            case ZeldaInput.InputModeEnum.MouseAndKeyboard:
                ZeldaInput.InputMode = ZeldaInput.InputModeEnum.XBOX_Gamepad;
                break;
            case ZeldaInput.InputModeEnum.XBOX_Gamepad:
                ZeldaInput.InputMode = ZeldaInput.InputModeEnum.OculusTouch;
                break;
            case ZeldaInput.InputModeEnum.OculusTouch:
                ZeldaInput.InputMode = ZeldaInput.InputModeEnum.MouseAndKeyboard;
                break;
            default:
                break;
        }        
    }

    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 500, 500));

        Zelda_GUI();

        GUILayout.EndArea();
    }

    void Zelda_GUI()
    {
        GUILayout.Label("InputMode: " + ZeldaInput.InputMode + "\n");

        GUILayout.Label(_cmdTriggerOutput);
        GUILayout.Label(_cmdBoolOutput);
        GUILayout.Label(_cmdFloatOutput);
    }

   
    string GetString_CmdTrigger()
    {
        string s = " --- Zelda CmdTrigger ---\n";
        int i = 0;
        foreach (Cmd_Trigger t in Enum.GetValues(typeof(Cmd_Trigger)))
        {
            bool b = _triggerTimeStamps[i] > 0;
            s += t.ToString() + " = " + b + SEPERATOR;

            i++;
        }
        return s;
    }
    string GetString_CmdBool()
    {
        string s = " --- Zelda CmdBool ---\n";
        foreach (Cmd_Bool b in Enum.GetValues(typeof(Cmd_Bool)))
        {
            s += b.ToString() + " = " + ZeldaInput.GetCommand_Bool(b) + SEPERATOR;
        }
        return s;
    }
    string GetString_CmdFloat()
    {
        string s = " --- Zelda CmdFloat ---\n";
        foreach (Cmd_Float f in Enum.GetValues(typeof(Cmd_Float)))
        {
            s += f.ToString() + " = " + ZeldaInput.GetCommand_Float(f).ToString("F3") + SEPERATOR;
        }
        return s;
    }
}