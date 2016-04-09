using UnityEngine;
using System;

public class LogCommandLineArgs : MonoBehaviour 
{
    public bool logToConsole = true;
    public bool logToGUI = false;


    string[] _args;
    //string _outputText_Commas;
    string _outputText_Lines;


	void Awake () 
    {
        _args = System.Environment.GetCommandLineArgs();
        //_outputText_Commas = GetOutputText_Commas();
        _outputText_Lines = GetOutputText_Lines();

        if (logToConsole) { print(_outputText_Lines); }
	}

    string GetOutputText_Commas()
    {
        return " -----  Command Line Args:  " + string.Join(", ", _args);
    }

    string GetOutputText_Lines()
    {
        int n = 1;

        string output = " -----  Command Line Args -----\n\n";
        foreach (var arg in _args)
        {
            output += "  " + n + ") " + arg + "\n";
            n++;
        }
        output += "\n ------------------------------";
        return output;
    }


    void OnGUI()
    {
        if (!logToGUI) { return; }

        GUI.TextField(new Rect(100, 100, 500, 400), _outputText_Lines);
    }
}
