using UnityEngine;

public class LostWoodsAtmosphere : MonoBehaviour 
{
    [SerializeField]
    LostSector _lostWoods;

    public float innerRadius, outerRadius;


    ZeldaFog _zeldaFog;


    void Start()
    {
        _zeldaFog = ZeldaFog.Instance;
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

            _zeldaFog.SetFogRatio(t);
            _zeldaFog.SetSunlightRatio(t);
        }
    }
}