using UnityEngine;
using UnityStandardAssets.ImageEffects;
using Immersio.Utility;

public class ZeldaFog : Singleton<ZeldaFog> 
{
    const float SUNLIGHT_INTENSITY_MIN = 0.1f;


    public float fogDensity_Max;


    GlobalFog _fog;
    public GlobalFog Fog { get { return _fog ?? (_fog = FindObjectOfType<GlobalFog>()); } }
    float _fogDensity_Normal;

    Light _sunlight;
    public Light Sunlight { get { return _sunlight ?? (_sunlight = GameObject.FindGameObjectWithTag("Sunlight").GetComponent<Light>()); } }
    float _sunlightIntensityNormal;
    

    void OnEnable()
    {
        if (Fog)
        {
            Fog.enabled = true;
        }
    }
    void OnDisable()
    {
        if (Fog)
        {
            Fog.enabled = false;
        }
    }

    void Start()
    {
        _fogDensity_Normal = Fog.heightDensity;
        _sunlightIntensityNormal = Sunlight.intensity;
    }

    
    public void SetFogRatio(float t)
    {
        Fog.heightDensity = Mathf.Lerp(_fogDensity_Normal, fogDensity_Max, 1 - t);
    }
    public void SetSunlightRatio(float t)
    {
        Sunlight.intensity = Mathf.Lerp(SUNLIGHT_INTENSITY_MIN, _sunlightIntensityNormal, t);
    }
}
