using UnityEngine;


public class GamepadTest : MonoBehaviour 
{
    public bool printJoysticks, printAxes, printButtons;


    void Update()
    {
        if (printJoysticks) { PrintJoysticks(); }
        if (printAxes) { PrintAxes(); }
        if (printButtons) { PrintButtons(); }
    }


    void PrintJoysticks()
    {
        int i = 0;
        string output = " --- Joysticks ---\n";
        foreach (string joyName in Input.GetJoystickNames())
        {
            output += i.ToString() + ":" + joyName + "\n";
            i++;
        }
        print(output);
    }

    void PrintAxes()
    {
        string output = " --- Axes ---\n";
        output += string.Format("A1:{0:F3} A2:{1:F3} A3:{2:F3} A4:{3:F3} A5:{4:F3} A6:{5:F3} A7:{6:F3} A8:{7:F3} A9:{8:F3} A10:{9:F3}",
            Input.GetAxis("JS_A1"), Input.GetAxis("JS_A2"),
            Input.GetAxis("JS_A3"), Input.GetAxis("JS_A4"),
            Input.GetAxis("JS_A5"), Input.GetAxis("JS_A6"),
            Input.GetAxis("JS_A7"), Input.GetAxis("JS_A8"),
            Input.GetAxis("JS_A9"), Input.GetAxis("JS_A10"));
        print(output);
    }

    void PrintButtons()
    {
        string output = " --- Buttons ---\n";
        output += string.Format("0:{0} 1:{1} 2:{2} 3:{3} 4:{4} 5:{5} 6:{6} 7:{7} 8:{8} 9:{9} 10:{10} 11:{11}",
            Input.GetButton("JS_B0"), Input.GetButton("JS_B1"),
            Input.GetButton("JS_B2"), Input.GetButton("JS_B3"),
            Input.GetButton("JS_B4"), Input.GetButton("JS_B5"),
            Input.GetButton("JS_B6"), Input.GetButton("JS_B7"),
            Input.GetButton("JS_B8"), Input.GetButton("JS_B9"),
            Input.GetButton("JS_B10"), Input.GetButton("JS_B11"));
        print(output);
    }
}
