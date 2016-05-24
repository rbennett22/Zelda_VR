using System.Collections;
using UnityEngine;

public class Triforce : MonoBehaviour
{
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
        CommonObjects.Player_C.IsParalyzed = true;
        PauseManager.Instance.IsPauseAllowed_Inventory = false;
        PauseManager.Instance.IsPauseAllowed_Options = false;

        iTween.MoveAdd(gameObject, new Vector3(0, 1, 0), fanfareDuration * 0.7f);

        iTween.ValueTo(gameObject, iTween.Hash(
            "from", _light.intensity,
            "to", 10.0f,
            "time", fanfareDuration + _fadeDuration,
            "easetype", iTween.EaseType.easeInQuint,
            "onupdate", "LightIntensityTweenCallback"));

        yield return new WaitForSeconds(fanfareDuration);

        OverlayViewController.Instance.ShowTriforceOverlay(_fadeDuration);

        yield return new WaitForSeconds(_fadeDuration);

        WarpToOverworld();
    }

    void LightIntensityTweenCallback(float intensity)
    {
        _light.intensity = intensity;
    }

    void WarpToOverworld()
    {
        OverlayViewController.Instance.HideTriforceOverlay(_fadeDuration);
        Destroy(gameObject, _fadeDuration + 0.1f);

        Locations.Instance.WarpToOverworldDungeonEntrance(false);
    }
}