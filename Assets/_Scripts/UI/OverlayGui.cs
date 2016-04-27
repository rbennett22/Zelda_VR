using UnityEngine;
using Immersio.Utility;

public class OverlayGui : Singleton<OverlayGui>
{
    Texture _whiteTexture;

    
    void Start()
    {
        _whiteTexture = GfxHelper.CreateUnitTexture(Color.white);
    }


    float _whiteOverlayAlpha;
    public void WhiteFade(bool fadeOut, float duration)
    {
        float fromAlpha = fadeOut ? 1.0f : 0.0f;
        float toAlpha = fadeOut ? 0.0f : 1.0f;
        iTween.EaseType easeType = fadeOut ? iTween.EaseType.easeOutSine : iTween.EaseType.linear;
        iTween.ValueTo(gameObject, iTween.Hash(
                "from", fromAlpha,
                "to", toAlpha,
                "time", duration,
                "ignoretimescale", true,
                "easetype", easeType,
                "onupdate", "WhiteFade_OnUpdate"));
    }
    void WhiteFade_OnUpdate(float alpha)
    {
        _whiteOverlayAlpha = alpha;
    }


    void Update()
    {
        UpdateOverlays();
    }

    void UpdateOverlays()
    {
        if (CommonObjects.Player_C.IsDead) { GUIShowRedOverlay(); }
        if (PauseManager.Instance.IsPaused_Inventory) { GUIShowGrayOverlay(); }
        if (PauseManager.Instance.IsPaused_Options) { GUIShowBlackOverlay(); }
        if (_whiteOverlayAlpha > 0) { GUIShowWhiteOverlay(_whiteOverlayAlpha); }
    }


    void GUIShowBlackOverlay()
    {
        GUIShowBackground(Color.black);
    }
    void GUIShowGrayOverlay()
    {
        Color c = Color.black;
        c.a = 0.4f;
        GUIShowBackground(c);
    }
    void GUIShowRedOverlay()
    {
        Color c = new Color(0.7f, 0, 0);
        c.a = 0.3f;
        GUIShowBackground(c);
    }
    void GUIShowWhiteOverlay(float alpha)
    {
        Color c = Color.white;
        c.a = alpha;
        GUIShowBackground(c);
    }

    void GUIShowBackground(Color color)
    {
        int d = 4000;
        //_stereoscopicGUI.GuiHelper.StereoDrawTexture(-d, -d, 3*d, 3*d, ref _whiteTexture, color);
    }
}