using UnityEngine;
using Immersio.Utility;

public class ZeldaHaptics : Singleton<ZeldaHaptics>
{
    [SerializeField, Range (0, 1)]
    float _rumbleStrength = 1;
    public float RumbleStrength {
        get { return _rumbleStrength; }
        set
        {
            _rumbleStrength = Mathf.Clamp01(value);
            InitRumbleClip();
        }
    }


    const int SAMPLES_COUNT = 64;
    byte[] _samples = new byte[SAMPLES_COUNT];

    OVRHapticsClip _rumbleClip;


    ZeldaHaptics()
    {
        InitRumbleClip();
    }

    void InitRumbleClip()
    {
        for (int i = 0; i < SAMPLES_COUNT; i++)
        {
            float t = 1;
            //float t = 1 - (float)i / SAMPLES_COUNT;   // Linear "crescendo"

            t *= _rumbleStrength;
            _samples[i] = (byte)(255 * t);
        }

        _rumbleClip = new OVRHapticsClip(_samples, SAMPLES_COUNT);
    }


    public void Rumble_Both()
    {
        Rumble_Left();
        Rumble_Right();
    }
    public void Rumble_Left()
    {
        Rumble(OVRHaptics.Channels[0]);
    }
    public void Rumble_Right()
    {
        Rumble(OVRHaptics.Channels[1]);
    }


    void Rumble(OVRHaptics.OVRHapticsChannel channel)
    {
        channel.Mix(_rumbleClip);
    }


    /*void Update()
    {
        //Update_TEST();
    }
    void Update_TEST()
    {
        RumbleStrength = _rumbleStrength;

        if (ZeldaInput.GetButton(ZeldaInput.Button.UseItemB))
        {
            Rumble_Both();
        }
        if (Input.GetKeyDown(KeyCode.Z))
        {
            Rumble_Left();
        }
        if (Input.GetKeyDown(KeyCode.X))
        {
            Rumble_Right();
        }
    }*/
}
