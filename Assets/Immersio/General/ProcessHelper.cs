using UnityEngine;
using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;

public class ProcessHelper : MonoBehaviour 
{
    //const int TimeToWaitForExit = 1000;

    static List<ProcessHelper> _processHelpers = new List<ProcessHelper>();


    public delegate void runProcessExited_Delegate(int exitCode);


    public static void RunProcess(string fileName, string args, runProcessExited_Delegate onProcessExited = null)
    {
        print("RunProcess:  fileName: " + fileName + ", args: " + args);

        ProcessHelper ph = new ProcessHelper();
        ph.Run(fileName, args, onProcessExited);
        _processHelpers.Add(ph);

        /*int exitCode = 0;

        try
        {
            _myProcess = Process.Start(fileName, args);

            do
            {
                if (!_myProcess.HasExited)
                {
                    string status = _myProcess.Responding ? "Running" : "Not Responding";
                    print(" ... Status = " + status + " ...");
                }
            }
            while (!_myProcess.WaitForExit(TimeToWaitForExit));

            exitCode = _myProcess.ExitCode;
            print("Process exited with code: " + exitCode);
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogException(e);
            throw e;
        }
        finally
        {
            if (_myProcess != null)
            {
                _myProcess.Close();
            }
        }

        if (onProcessExited != null)
        {
            onProcessExited(exitCode);
        }*/
    }
    

    public void Run(string fileName, string args, runProcessExited_Delegate onProcessExited = null)
    {
        StartCoroutine(WaitForProcessToExit(fileName, args, onProcessExited));
    }

    IEnumerator WaitForProcessToExit(string fileName, string args, runProcessExited_Delegate onProcessExited = null)
    {
        Process myProcess = null;
        int exitCode = 0;

        try
        {
            myProcess = Process.Start(fileName, args);

            while (!myProcess.HasExited)
            {
                string status = myProcess.Responding ? "Running" : "Not Responding";
                print(" ... Status = " + status + " ...");

                yield return new WaitForSeconds(1);
            }

            exitCode = myProcess.ExitCode;
            print("Process exited with code: " + exitCode);
        }
        finally
        {
            if (myProcess != null)
            {
                myProcess.Close();
            }
        }

        if (onProcessExited != null)
        {
            onProcessExited(exitCode);
        }
    }

}
