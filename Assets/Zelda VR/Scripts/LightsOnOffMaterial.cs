using UnityEngine;

public class LightsOnOffMaterial : MonoBehaviour 
{

    Color _origColor;


    public bool IsTurnedOn { get; private set; }


    void Awake()
    {
        IsTurnedOn = true;
    }

    public void TurnLightsOn(bool turnOn = true)
    {
        if (IsTurnedOn == turnOn) { return; }

        if (turnOn)
        {
            GetComponent<Renderer>().material.color = _origColor;
        }
        else
        {
            Material m = GetComponent<Renderer>().material;
            _origColor = m.color;
            m.color = new Color(0, 0, 0, m.color.a);
        }

        IsTurnedOn = turnOn;
    }

    // Call this if you change the object's material from outside this class
    public void OnMaterialChanged()
    {
        if (IsTurnedOn) { return; }

        Material m = GetComponent<Renderer>().material;
        _origColor = m.color;
        m.color = new Color(0, 0, 0, m.color.a);
    }

}