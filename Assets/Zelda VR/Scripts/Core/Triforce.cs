using System.Collections;
using UnityEngine;

public class Triforce : MonoBehaviour
{
    const float MAX_LIGHT_INTENSITY = 10.0f;


    public float fanfareDuration = 7.0f;

    [SerializeField]
    Light _light;


    float _fadeDuration = 3.0f;


    public void Fanfare()
    {
        StartCoroutine("Fanfare_Coroutine");
    }
    IEnumerator Fanfare_Coroutine()
    {
        Music.Instance.Stop();
        SoundFx.Instance.PlayOneShot(SoundFx.Instance.triforceFanfare);

        Locations.Instance.LimitControls();

        GetComponent<Collectible>().PlayRisingTween();

        iTween.ValueTo(gameObject, iTween.Hash(
            "from", _light.intensity,
            "to", MAX_LIGHT_INTENSITY,
            "time", fanfareDuration + _fadeDuration,
            "easetype", iTween.EaseType.easeInQuint,

            "onupdate", "LightIntensityTween_OnUpdate"));

        yield return new WaitForSeconds(fanfareDuration);

        OverlayViewController.Instance.ShowTriforceOverlay(_fadeDuration);

        yield return new WaitForSeconds(_fadeDuration);

        WarpToOverworld();
    }

    void LightIntensityTween_OnUpdate(float intensity)
    {
        _light.intensity = intensity;
    }

    void WarpToOverworld()
    {
        OverlayViewController.Instance.HideTriforceOverlay(_fadeDuration);
        Destroy(gameObject, _fadeDuration + 0.1f);

        Locations.Instance.RestoreControls();

        Locations.Instance.WarpToOverworldDungeonEntrance(false);
    }
}