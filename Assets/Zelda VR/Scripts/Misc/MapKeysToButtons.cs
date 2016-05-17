using UnityEngine;

public class MapKeysToButtons : MonoBehaviour
{
    [SerializeField]
    KeyCode[] _keys;

    [SerializeField]
    GameObject[] _buttons;


    void Update()
    {
        for (int i = 0; i < _keys.Length; i++)
        {
            if (Input.GetKeyUp(_keys[i]))
            {
                PressButtonCorrespondingToKeyIndex(i);
            }
        }
    }


    void PressButtonCorrespondingToKeyIndex(int keyIdx)
    {
        if (keyIdx < 0 || keyIdx > _buttons.Length - 1)
        {
            return;
        }

        GameObject btn = _buttons[keyIdx];
        if (btn != null)
        {
            if (!btn.activeSelf)
            {
                btn.SetActive(true);
            }
            PointerEventSimulator.SimulateClick(btn);
        }
    }
}