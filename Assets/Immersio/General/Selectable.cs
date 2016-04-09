using UnityEngine;
using System;
using System.Collections;


public class Selectable : MonoBehaviour 
{
    public bool verbose = false;


    public enum SelectionState
    {
        Default,
        Highlighted,
        Selected,
        Inactive
    }

    SelectionState _state;
    public SelectionState State 
    {
        get { return _state; }
        set {
            if (value != _state)
            {
                SelectionState oldState = _state;
                _state = value;
                SendNotificationOfStateChange(_state, oldState);
            }
        }
    }


	void Start () 
    {
        State = SelectionState.Default;
	}


    public void SetToDefaultState()     { State = SelectionState.Default; }
    public void Highlight()             { State = SelectionState.Highlighted; }
    public void Select()                { State = SelectionState.Selected; }
    public void Deactivate()            { State = SelectionState.Inactive; }


    void SendNotificationOfStateChange(SelectionState newState, SelectionState oldState)
    {
        String methodName = String.Empty;
        switch (newState)
        {
            case SelectionState.Default:        methodName = "OnStateChange_Default";       break;
            case SelectionState.Highlighted:    methodName = "OnStateChange_Highlighted";   break;
            case SelectionState.Selected:       methodName = "OnStateChange_Selected";      break;
            case SelectionState.Inactive:       methodName = "OnStateChange_Inactive";      break;
        }

        LogStatus("SelectionStateChange from " + oldState + " to " + newState);

        SendMessage(methodName, oldState, SendMessageOptions.DontRequireReceiver);
    }


    void LogStatus(string msg)
    {
        if (verbose) { print(msg); }
    }
}
