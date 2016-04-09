using UnityEngine;
using System;
using System.Diagnostics;
using System.Collections;


public class FairyPond : MonoBehaviour
{
    const float RestorationUpdateInterval_ms = 125;


    public ParticleSystem heartParticleSystem;
    public float restingEmmission, peakEmission;


    bool _playingRestorationSequence;


    void Awake()
    {
        heartParticleSystem.emissionRate = restingEmmission;
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

        heartParticleSystem.emissionRate = peakEmission;

        SoundFx.Instance.PlayOneShot(SoundFx.Instance.fanfare);

        _playingRestorationSequence = true;
        StartCoroutine("Restoration_Coroutine");
    }


    IEnumerator Restoration_Coroutine()
    {
        while (_playingRestorationSequence)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            {
                Restoration_Tick();
            }
            stopWatch.Stop();

            int elapsedTime = stopWatch.Elapsed.Milliseconds;
            float waitTime = 0.001f * Mathf.Max(0, RestorationUpdateInterval_ms - elapsedTime);
            yield return new WaitForSeconds(waitTime);
        }

        CommonObjects.Player_C.IsParalyzed = false;
        heartParticleSystem.emissionRate = restingEmmission;
    }

    void Restoration_Tick()
    {
        if (CommonObjects.Player_C.IsAtFullHealth)
        {
            _playingRestorationSequence = false;
            return;
        }

        //SoundFx.Instance.PlayOneShot(SoundFx.Instance.heart);

        CommonObjects.Player_C.RestoreHalfHearts(1);
    }

}