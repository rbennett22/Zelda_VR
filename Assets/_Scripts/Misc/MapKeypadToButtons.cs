using UnityEngine;

public class MapKeypadToButtons : MonoBehaviour
{
    [SerializeField]
    GameObject[] _buttons;      // The keypad number keys will map to corresponding indices (0-9) in this array


	void Update ()
    {
        int inputKeyNum = GetKeypadNumInput();
        if(inputKeyNum != -1)
        {
            PressButtonCorrespondingToKeypadNumber(inputKeyNum);
        }
    }

    int GetKeypadNumInput()
    {
        int n = -1;

        if (Input.GetKeyUp(KeyCode.Keypad0))
        {
            n = 0;
        }
        else if (Input.GetKeyUp(KeyCode.Keypad1))
        {
            n = 1;
        }
        else if (Input.GetKeyUp(KeyCode.Keypad2))
        {
            n = 2;
        }
        else if (Input.GetKeyUp(KeyCode.Keypad3))
        {
            n = 3;
        }
        else if (Input.GetKeyUp(KeyCode.Keypad4))
        {
            n = 4;
        }
        else if (Input.GetKeyUp(KeyCode.Keypad5))
        {
            n = 5;
        }
        else if (Input.GetKeyUp(KeyCode.Keypad6))
        {
            n = 6;
        }
        else if (Input.GetKeyUp(KeyCode.Keypad7))
        {
            n = 7;
        }
        else if (Input.GetKeyUp(KeyCode.Keypad8))
        {
            n = 8;
        }
        else if (Input.GetKeyUp(KeyCode.Keypad9))
        {
            n = 9;
        }

        return n;
    }

    void PressButtonCorrespondingToKeypadNumber(int keyNum)
    {
        if(keyNum < 0 || keyNum > _buttons.Length - 1)
        {
            return;
        }

        GameObject btn = _buttons[keyNum];
        if(btn != null)
        {
            if(!btn.activeSelf)
            {
                btn.SetActive(true);
            }
            PointerEventSimulator.SimulateClick(btn);
        }
    }
}
