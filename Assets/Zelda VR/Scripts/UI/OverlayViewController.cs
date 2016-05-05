using UnityEngine;
using Immersio.Utility;

public class OverlayViewController : Singleton<OverlayViewController>
{
    [SerializeField]
    OverlayView _playerDiedOverlay;
    [SerializeField]
    OverlayView _triforceAquiredOverlay;


    public void ShowPlayerDiedOverlay(float duration = 0)
    {
        FadeOverlay(_playerDiedOverlay, true, duration);
    }
    public void HidePlayerDiedOverlay(float duration = 0)
    {
        FadeOverlay(_playerDiedOverlay, false, duration);
    }

    public void ShowTriforceOverlay(float duration = 0)
    {
        FadeOverlay(_triforceAquiredOverlay, true, duration);
    }
    public void HideTriforceOverlay(float duration = 0)
    {
        FadeOverlay(_triforceAquiredOverlay, false, duration);
    }

    void FadeOverlay(OverlayView overlay, bool fadeIn, float duration)
    {
        if(fadeIn)
        {
            overlay.FadeOut(0);
            overlay.FadeIn(duration);
        }
        else
        {
            overlay.FadeIn(0);
            overlay.FadeOut(duration);
        }
    }


    override protected void Awake()
    {
        base.Awake();

        _playerDiedOverlay.gameObject.SetActive(true);
        _triforceAquiredOverlay.gameObject.SetActive(true);

        HidePlayerDiedOverlay();
        HideTriforceOverlay();
    }
}
