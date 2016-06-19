using UnityEngine;
using UnityStandardAssets.ImageEffects;

public class LostWoodsAtmosphere : MonoBehaviour 
{
    const float SUNLIGHT_INTENSITY_MIN = 0.1f;


    [SerializeField]
    LostSector _lostWoods;

    public float fogDensity_Max;
    public float innerRadius, outerRadius;


    GlobalFog _fog;
    float _fogDensity_Normal;
    float _sunlightIntensityNormal;
    Light _sunlight;


    void Start()
    {
        _fog = FindObjectOfType<GlobalFog>();
        _fogDensity_Normal = _fog.heightDensity;

        _sunlight = GameObject.FindGameObjectWithTag("Sunlight").GetComponent<Light>();
        _sunlightIntensityNormal = _sunlight.intensity;
    }


    void Update()
    {
        Vector3 toPlayer = _lostWoods.PlayerPos - _lostWoods.Position;
        toPlayer.y = 0;
        float distToPlayer = toPlayer.magnitude;
        if (distToPlayer < outerRadius + 1)
        {
            float d = Mathf.Clamp(distToPlayer, innerRadius, outerRadius);
            float t = Mathf.InverseLerp(innerRadius, outerRadius, d);
            UpdateFog(t);
            UpdateSunlight(t);
        }
    }
    void UpdateFog(float t)
    {
        _fog.heightDensity = Mathf.Lerp(_fogDensity_Normal, fogDensity_Max, 1 - t);
    }
    void UpdateSunlight(float t)
    {
        _sunlight.intensity = Mathf.Lerp(SUNLIGHT_INTENSITY_MIN, _sunlightIntensityNormal, t);
    }
}