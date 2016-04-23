using UnityEngine;
using Immersio.Utility;


public class OverlayGui : Singleton<OverlayGui>
{
    const string ShutersFinishedClosingCallbackName = "ShuttersFinishedClosing";
    const string ShutersFinishedOpeningCallbackName = "ShuttersFinishedOpening";


    //StereoscopicGUI _stereoscopicGUI;
    Texture _whiteTexture;

    public bool IsDemoThanksTextShowing { get; set; }

    
    void Start()
    {
        _whiteTexture = GfxHelper.CreateColoredTexture(Color.white);
    }


    #region Shutters

    public float shutterSpeed = 500.0f;

    float _leftShutterX, _rightShutterX;
    float _shutterWidth = Screen.width * 0.6f;
    float _shutterHeight = Screen.height * 2;
    float _screenCenterX = Screen.width * 0.5f;
    float _prevRealTime;
    bool _showShutters;
    Color _shutterColor = Color.black;
    GameObject _shutterDelegate;

    public bool ShuttersAreClosing { get; private set; }
    public bool ShuttersAreOpening { get; private set; }


    public void PlayShutterCloseSequence(GameObject notifyOnFinish = null)
    {
        PlayShutterCloseSequence(notifyOnFinish, Color.black);
    }
    public void PlayShutterCloseSequence(GameObject notifyOnFinish, Color color)
    {
        if (ShuttersAreClosing) { return; }

        _shutterColor = color;

        _leftShutterX = _screenCenterX - _shutterWidth * 2;
        _rightShutterX = _screenCenterX + _shutterWidth;
        _prevRealTime = Time.realtimeSinceStartup;
        ShuttersAreClosing = true;
        _showShutters = true;

        _shutterDelegate = notifyOnFinish;
    }

    public void PlayShutterOpenSequence(GameObject notifyOnFinish = null)
    {
        PlayShutterOpenSequence(notifyOnFinish, Color.black);
    }
    public void PlayShutterOpenSequence(GameObject notifyOnFinish, Color color)
    {
        if (ShuttersAreOpening) { return; }

        _shutterColor = color;

        _leftShutterX = _screenCenterX - _shutterWidth;
        _rightShutterX = _screenCenterX;
        _prevRealTime = Time.realtimeSinceStartup;
        ShuttersAreOpening = true;

        _shutterDelegate = notifyOnFinish;
    }

    #endregion


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
                "easetype", easeType,
                "onupdate", "WhiteOverlayTweenCallback"));
    }
    void WhiteOverlayTweenCallback(float alpha)
    {
        _whiteOverlayAlpha = alpha;
    }

    public void ShowDemoThanksText(bool doShow = true)
    {
        IsDemoThanksTextShowing = doShow;
    }

    void Update()
    {
        float deltaTime = Time.realtimeSinceStartup - _prevRealTime;
        float deltaX = deltaTime * shutterSpeed;

        if (ShuttersAreClosing)
        {
            _leftShutterX += deltaX;
            _rightShutterX -= deltaX;

            if (_leftShutterX > _screenCenterX - _shutterWidth + 25)
            {
                ShuttersAreClosing = false;
                if (_shutterDelegate != null) { _shutterDelegate.SendMessage(ShutersFinishedClosingCallbackName, SendMessageOptions.DontRequireReceiver); }
            }
        }
        else if (ShuttersAreOpening)
        {
            _leftShutterX -= deltaX;
            _rightShutterX += deltaX;

            if  (_leftShutterX < _screenCenterX - 2 * _shutterWidth)
            {
                ShuttersAreOpening = false;
                _showShutters = false;
                if (_shutterDelegate != null) { _shutterDelegate.SendMessage(ShutersFinishedOpeningCallbackName, SendMessageOptions.DontRequireReceiver); }
                _shutterDelegate = null;
            }
        }

        _prevRealTime = Time.realtimeSinceStartup;

        if (ZeldaConfig.Instance.isDemo)
        {
            UpdateDemoTextPosition();
        }
    }

    int _demoTextHeight = -440;
    void UpdateDemoTextPosition()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow)) { _demoTextHeight -= 10; }
        if (Input.GetKeyDown(KeyCode.UpArrow)) { _demoTextHeight += 10; }
    }

    /*
    void OnStereoscopicGUI(StereoscopicGUI stereoscopicGUI)
    {
        _stereoscopicGUI = stereoscopicGUI;

        if (CommonObjects.Player_C.IsDead) { GUIShowRedOverlay(); }
        if (Pause.Instance.IsInventoryShowing) { GUIShowGrayOverlay(); }
        if (Pause.Instance.IsMenuShowing) { GUIShowBlackOverlay(); }
        if (_showShutters) { GUIShutterSequence(); }
        if (_whiteOverlayAlpha > 0) { GUIShowWhiteOverlay(_whiteOverlayAlpha); }
        if (IsDemoThanksTextShowing) { GUIShowDemoThanks(); }
    }


    void GUIShutterSequence()
    {
        int w = (int)_shutterWidth;
        int h = (int)_shutterHeight;
        int y = 0;

        _stereoscopicGUI.GuiHelper.StereoDrawTexture((int)_leftShutterX, y, w, h, ref _whiteTexture, _shutterColor);
        _stereoscopicGUI.GuiHelper.StereoDrawTexture((int)_rightShutterX, y, w, h, ref _whiteTexture, _shutterColor);
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
        _stereoscopicGUI.GuiHelper.StereoDrawTexture(-d, -d, 3*d, 3*d, ref _whiteTexture, color);
    }


    void GUIShowDemoThanks()
    {
        GUIShowBackground(Color.white);

        int fontSize = 26;
        TextAnchor alignment = TextAnchor.UpperCenter;

        Vector2 center = new Vector2(Screen.width, Screen.height) * 0.5f;
        int w = 0;
        int h = _demoTextHeight;
        int x = (int)(center.x - w * 0.5f);
        int y = (int)(center.y - h * 0.5f);
        
        string text = "Thanks for playing the Zelda VR Beta! \n";
        text += "Feedback can be submitted via virtualreality.io.\n";
        text += "Full Release: March 2014 \n\n";
        text += "Press START to return to Overworld.";

        int storedFontSize = GUI.skin.label.fontSize;
        TextAnchor storedAlignment = alignment;

        GUI.skin.label.fontSize = fontSize;
        GUI.skin.label.alignment = TextAnchor.UpperCenter;
        _stereoscopicGUI.GuiHelper.StereoLabel(x, y, w, h, ref text, Color.black);

        GUI.skin.label.fontSize = storedFontSize;
        GUI.skin.label.alignment = storedAlignment;
    }
    */
}