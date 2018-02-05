using UnityEngine;
using Immersio.Utility;

public class ZeldaHaptics : Singleton<ZeldaHaptics>
{
    [SerializeField, Range(0, 10)]
    int _audioClipAmpMultiplier = 1;

    [SerializeField, Range (0, 1)]
    float _rumbleStrength = 1;
    public float RumbleStrength {
        get { return _rumbleStrength; }
        set
        {
            _rumbleStrength = Mathf.Clamp01(value);
            _rumbleSimpleClip = null; // This essentially marks clip as "dirty" so it will be recreated lazily.
        }
    }


    const int SAMPLES_COUNT = 64;
    byte[] _samples = new byte[SAMPLES_COUNT];

    OVRHapticsClip _rumbleSimpleClip;
    public OVRHapticsClip RumbleSimpleClip { get { return _rumbleSimpleClip ?? (_rumbleSimpleClip = CreateRumbleClip()); } }
    OVRHapticsClip CreateRumbleClip()
    {
        OVRHapticsClip clip = null;

        for (int i = 0; i < SAMPLES_COUNT; i++)
        {
            float t = 1;
            //float t = 1 - (float)i / SAMPLES_COUNT;   // Linear "crescendo"

            t *= _rumbleStrength;
            _samples[i] = (byte)(255 * t);
        }

        if (ZeldaInput.AreAnyTouchControllersActive())
        {
            clip = new OVRHapticsClip(_samples, SAMPLES_COUNT);
        }

        return clip;
    }
    

    public void RumbleSimple_Both()
    {
        RumbleSimple_Left();
        RumbleSimple_Right();
    }
    public void RumbleSimple_Left()
    {
        RumbleSimple(OVRHaptics.LeftChannel);
    }
    public void RumbleSimple_Right()
    {
        RumbleSimple(OVRHaptics.RightChannel);
    }

    public void Rumble_Both(AudioClip audioClip)
    {
        Rumble_Left(audioClip);
        Rumble_Right(audioClip);
    }
    public void Rumble_Left(AudioClip audioClip)
    {
        Rumble(OVRHaptics.LeftChannel, audioClip);
    }
    public void Rumble_Right(AudioClip audioClip)
    {
        Rumble(OVRHaptics.RightChannel, audioClip);
    }


    void RumbleSimple(OVRHaptics.OVRHapticsChannel channel)
    {
        if (channel == null) { return; }
        
        if (RumbleSimpleClip != null)
        {
            channel.Mix(RumbleSimpleClip);
        }
    }

    void Rumble(OVRHaptics.OVRHapticsChannel channel, AudioClip audioClip)
    {
        RumbleSimple(channel);      // TODO: remove

        ////////////

        /*if (channel == null) { return; }

        OVRHapticsClip clip = new OVRHapticsClip(audioClip);        // TODO: This line crashes Unity
        for (int i = 0; i < _audioClipAmpMultiplier; i++)
        {
            channel.Mix(clip);
        }*/
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
