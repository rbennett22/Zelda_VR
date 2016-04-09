using UnityEngine;
using System.Collections;


public class Triforce : MonoBehaviour 
{

    public Light light;
    public float fanfareDuration = 7.0f;


    float _fadeDuration = 3.0f;


    public void Fanfare()
    {
        StartCoroutine("Fanfare_Coroutine");
    }
    IEnumerator Fanfare_Coroutine()
    {
        Music.Instance.Stop();
        SoundFx.Instance.PlayOneShot(SoundFx.Instance.triforceFanfare);
        CommonObjects.Player_C.IsParalyzed = true;
        Pause.Instance.IsAllowed = false;

        iTween.MoveAdd(gameObject, new Vector3(0, 1, 0), fanfareDuration * 0.7f);

        iTween.ValueTo(gameObject, iTween.Hash(
            "from", light.intensity,
            "to", 10.0f,
            "time", fanfareDuration + _fadeDuration,
            "easetype", iTween.EaseType.easeInQuint,
            "onupdate", "LightIntensityTweenCallback"));

        yield return new WaitForSeconds(fanfareDuration);

        OverlayGui.Instance.WhiteFade(false, _fadeDuration);
        yield return new WaitForSeconds(_fadeDuration);
		
        WarpToOverworld();
    }

    void LightIntensityTweenCallback(float intensity)
    {
        light.intensity = intensity;
    }

    void ShuttersFinishedClosing()
    {
        OverlayGui.Instance.ShowDemoThanksText();
    }

    void Update()
    {
        if (OverlayGui.Instance.IsDemoThanksTextShowing)
        {
            if (ZeldaInput.GetButtonDown(ZeldaInput.Button.Start))
            {
                SoundFx.Instance.PlayOneShot(SoundFx.Instance.select);
                OverlayGui.Instance.ShowDemoThanksText(false);

                WarpToOverworld();
            }
        }
    }

    void WarpToOverworld()
    {
        CommonObjects.Player_C.IsParalyzed = false;
        Pause.Instance.IsAllowed = true;

        OverlayGui.Instance.WhiteFade(true, _fadeDuration);
        Destroy(gameObject, _fadeDuration + 0.1f);

        Locations.Instance.WarpToOverworldDungeonEntrance(false);
    }

}