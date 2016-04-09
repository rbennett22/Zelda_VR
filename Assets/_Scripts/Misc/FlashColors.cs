using UnityEngine;

public class FlashColors : MonoBehaviour 
{
    int _flashIdx = 0;
    Color[] _flashColors = { 
                               Color.white, 
                               new Color(1, 0.3f, 0.3f), 
                               new Color(0.3f, 1, 0.3f), 
                               new Color(0.3f, 0.3f, 1) };


    void OnDisable()
    {
        GetComponent<Renderer>().material.color = _flashColors[0];
    }

    public void Update()
    {
        if (++_flashIdx >= _flashColors.Length) { _flashIdx = 0; }
        GetComponent<Renderer>().material.color = _flashColors[_flashIdx];
    }

}