using UnityEngine;
using System.Collections.Generic;


public class EnemyAI_Patra : EnemyAI 
{
    public float BaseRadius = 1.3f;
    public float ExpandedRadius = 4.5f;
    public float ChanceToPulse = 0.5f;
    public float pulseSpeed = 1.0f;
    public GameObject smallPatraPrefab;


    int _frameCount;
    bool _isPulsing;
    int _numSmallPatra = 8;
    List<EnemyAI_PatraSmall> _smallPatras = new List<EnemyAI_PatraSmall>();


    void Start()
    {
        for (int i = 0; i < _numSmallPatra; i++)
        {
            float phaseOffset = 2 * Mathf.PI * i / _numSmallPatra;
            SpawnSmallPatra(phaseOffset);
        }
    }

    void SpawnSmallPatra(float phaseOffset)
    {
        GameObject g = Instantiate(smallPatraPrefab) as GameObject;
        g.name = smallPatraPrefab.name;
        g.transform.parent = transform;
        g.transform.localPosition = Vector3.zero;

        EnemyAI_PatraSmall smallPatra = g.GetComponent<EnemyAI_PatraSmall>();
        smallPatra.phaseOffset = phaseOffset;
        smallPatra.ParentPatra = this;
        smallPatra.Radius = BaseRadius;

        _smallPatras.Add(smallPatra);
    }


    void Update()
    {
        if (!_doUpdate) { return; }
        if (_enemy.IsParalyzed || _enemy.IsStunned) { return; }

        if (++_frameCount == 30)
        {
            _frameCount = 0;

            if (!_isPulsing)
            {
                if (Extensions.FlipCoin(ChanceToPulse))
                {
                    Pulse();
                }
            }
        }
    }

    void Pulse()
    {
        _isPulsing = true;

        // Expand
        iTween.ValueTo(gameObject, iTween.Hash(
            "from", BaseRadius, "to", ExpandedRadius,
            "speed", pulseSpeed,
            "onupdate", "OnUpdateRadius",
            "oncomplete", "Contract", "oncompletetarget", gameObject
            ));
    }

    void Contract()
    {
        iTween.ValueTo(gameObject, iTween.Hash(
            "from", ExpandedRadius, "to", BaseRadius,
            "speed", pulseSpeed,
            "onupdate", "OnUpdateRadius",
            "oncomplete", "OnCompletedPulse", "oncompletetarget", gameObject
            ));
    }

    void OnUpdateRadius(float radius)
    {
        foreach (var patra in _smallPatras)
        {
            patra.Radius = radius;
        }
    }

    void OnCompletedPulse()
    {
        _isPulsing = false;
    }


    void OnSmallPatraDied(EnemyAI_PatraSmall smallPatra)
    {
        if (--_numSmallPatra == 0)
        {
            GetComponent<HealthController>().isIndestructible = false;
        }
    }

}