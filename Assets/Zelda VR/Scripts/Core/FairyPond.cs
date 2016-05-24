using UnityEngine;

public class FairyPond : MonoBehaviour
{
    const float RESTORATION_UPDATE_INTERVAL_MS = 125;


    public ParticleSystem heartParticleSystem;
    public float restingEmmission, peakEmission;


    bool _playingRestorationSequence;


    float ParticleEmissionRate
    {
        get { return heartParticleSystem.emission.rate.constantMax; }
        set
        {
            var em = heartParticleSystem.emission;
            var rate = new ParticleSystem.MinMaxCurve();
            rate.constantMax = value;
            em.rate = rate;
        }
    }


    void Awake()
    {
        ParticleEmissionRate = restingEmmission;
    }


    void OnTriggerEnter(Collider other)
    {
        if (CommonObjects.IsPlayer(other.gameObject))
        {
            if (!CommonObjects.Player_C.IsAtFullHealth)
            {
                PlayRestorationSequence();
            }
        }
    }

    void PlayRestorationSequence()
    {
        if (_playingRestorationSequence) { return; }

        CommonObjects.Player_C.IsParalyzed = true;
        CommonObjects.Player_C.ParalyzeAllNearbyEnemies(3.0f);
        CommonObjects.Player_C.DeactivateJinx();

        ParticleEmissionRate = peakEmission;

        SoundFx.Instance.PlayOneShot(SoundFx.Instance.fanfare);

        _playingRestorationSequence = true;

        InvokeRepeating("Restoration_Tick", 0, RESTORATION_UPDATE_INTERVAL_MS * 0.001f);
    }


    void Restoration_Tick()
    {
        if (CommonObjects.Player_C.IsAtFullHealth)
        {
            OnRestorationComplete();
            return;
        }

        //SoundFx.Instance.PlayOneShot(SoundFx.Instance.heart);

        CommonObjects.Player_C.RestoreHalfHearts(1);
    }

    void OnRestorationComplete()
    {
        _playingRestorationSequence = false;
        CancelInvoke("Restoration_Tick");

        CommonObjects.Player_C.IsParalyzed = false;
        ParticleEmissionRate = restingEmmission;
    }
}