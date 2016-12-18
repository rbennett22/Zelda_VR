using UnityEngine;

public class LightsOnOffMaterial : MonoBehaviour
{
    Color _origColor;


    public bool IsOn { get; private set; }


    void Awake()
    {
        IsOn = true;
    }

    public void SetLightsOnOff(bool value = true)
    {
        if (IsOn == value)
        {
            return;
        }
        IsOn = value;

        if (IsOn)
        {
            GetComponent<Renderer>().material.color = _origColor;
        }
        else
        {
            SetColorToBlack();
        }
    }

    // Call this if you change the object's material from outside this class
    public void OnMaterialChanged()
    {
        if (IsOn) { return; }

        SetColorToBlack();
    }


    void SetColorToBlack()
    {
        Material m = GetComponent<Renderer>().material;
        _origColor = m.color;
        m.color = new Color(0, 0, 0, m.color.a);
    }
}