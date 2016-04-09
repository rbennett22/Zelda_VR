using UnityEngine;
using Immersio.Utility;


public class PauseMenu : Singleton<PauseMenu>
{
    const int NumOptions = 5;
    const int ButtonHeight = 40;


    public bool IsShowing { get; private set; }


    int _cursorIndex;
    bool _cursorCooldownActive = false;
    int _cursorCooldownDuration = 6;
    int _cursorCooldownCounter = 0;

    string[] _buttonNames = { "Resume", "Toggle Music", "View Controls", "Look Sensitivity", "Save & Quit" };


    void Awake()
    {
        Hide();
    }


    public void Show()
    {
        PauseMenu_UI.SetActive(true);

        _cursorIndex = 0;
        IsShowing = true;
    }

    public void Hide()
    {
        PauseMenu_UI.SetActive(false);

        IsShowing = false;
    }


    void Update()
    {
        if (IsShowing)
        {
            UpdateCursor();

            if (ZeldaInput.GetButtonUp(ZeldaInput.Button.Start))
            {
                SelectOption(_cursorIndex);
            }
        }
    }

    void UpdateCursor()
    {
        if (_cursorCooldownActive)
        {
            if (++_cursorCooldownCounter >= _cursorCooldownDuration) { _cursorCooldownActive = false; }
        }
        else
        {
            float vertAxis = ZeldaInput.GetAxis(ZeldaInput.Axis.MoveVertical);
            int direction = 0;
            if (vertAxis != 0) { direction = vertAxis < 0 ? 1 : -1; }

            if (direction == 0)
            {
                if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) { direction = -1; }
                if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) { direction = 1; }
            }

            if (direction != 0)
            {
                MoveCursor(direction);
            }
        }
    }

    void MoveCursor(int direction)
    {
        SetCursorIndex(_cursorIndex + direction);
    }

    void SetCursorIndex(int index)
    {
        _cursorIndex = index;
        if (_cursorIndex < 0) { _cursorIndex = NumOptions - 1; }
        else if (_cursorIndex > NumOptions - 1) { _cursorIndex = 0; }

        SoundFx sfx = SoundFx.Instance;
        sfx.PlayOneShot(sfx.cursor);

        _cursorCooldownActive = true;
        _cursorCooldownCounter = 0;
    }


    void SelectOption(int index)
    {
        switch (index)
        {
            case 0: Resume(); break;
            case 1: ToggleMusic(); break;
            case 2: ViewControls(); break;
            case 3: EditLookSensitivity(); break;
            case 4: SaveAndQuit(); break;
            default: break;
        }
    }

    public void Resume()
    {
        Pause.Instance.ForceHideMenu();
    }

    public void ToggleMusic()
    {
        Music.Instance.ToggleEnabled();
    }

    public void ViewControls()
    {
        // TODO
    }

    public void EditLookSensitivity()
    {
        // TODO
    }

    public void SaveAndQuit()
    {
        Pause.Instance.ForceHideMenu();
        Pause.Instance.ForceHideInventory();
        SaveManager.Instance.SaveGame();
        Locations.Instance.LoadTitleScreen();
    }


    #region UI Stuff

    const string PauseMenuUIPrefabPath = "PauseMenu UI";

    const float CanvasWidth = 1920f;
    const float CanvasHeight = 1080f;
    const float CanvasScale = 0.001f;
    const float CanvasOffsetZ = 0.64f;


    static GameObject _pauseMenuCanvas;
    static GameObject PauseMenuCanvas { get { return _pauseMenuCanvas ?? (_pauseMenuCanvas = CreateCanvasOnVRCamera()); } }
    static GameObject _pauseMenu_UI;
    static GameObject PauseMenu_UI { get { return _pauseMenu_UI ?? (_pauseMenu_UI = InstantiateUiPanel(PauseMenuUIPrefabPath)); } }

    static GameObject CreateCanvasOnVRCamera()
    {
        GameObject centerEyeAnchor = GameObject.Find("CenterEyeAnchor");
        if (centerEyeAnchor == null)
        {
            return null;
        }

        GameObject pauseMenuCanvas = new GameObject();
        pauseMenuCanvas.name = "Pause Menu Canvas";
        pauseMenuCanvas.transform.parent = centerEyeAnchor.transform;

        RectTransform rt = pauseMenuCanvas.AddComponent<RectTransform>();
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, CanvasWidth);
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, CanvasHeight);
        rt.localScale = CanvasScale * Vector3.one;
        rt.localPosition = new Vector3(0, 0, CanvasOffsetZ);
        rt.localEulerAngles = Vector3.zero;

        Canvas canvas = pauseMenuCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.pixelPerfect = false;

        return pauseMenuCanvas;
    }

    static GameObject InstantiateUiPanel(string prefabPath)
    {
        if (PauseMenuCanvas == null)
        {
            return null;
        }

        GameObject prefab = Resources.Load<GameObject>(prefabPath);
        if (prefab == null)
        {
            Debug.LogWarning("Prefab not found in Resources: " + prefabPath);
            return null;
        }

        GameObject g = Instantiate(prefab) as GameObject;
        g.name = prefab.name;

        Transform t = g.transform;
        t.SetParent(PauseMenuCanvas.transform);
        t.localPosition = Vector3.zero;
        t.localRotation = Quaternion.identity;
        t.localScale = Vector3.one;

        return g;
    }

    #endregion


    /*StereoscopicGUI _stereoscopicGUI;

    void OnStereoscopicGUI(StereoscopicGUI stereoscopicGUI)
    {
        _stereoscopicGUI = stereoscopicGUI;

        if (IsShowing) { GUIShowMenu(); }
    }

    void GUIShowMenu()
    {
        int fontSize = 24;
        TextAnchor alignment = TextAnchor.UpperCenter;

        Vector2 center = new Vector2(Screen.width, Screen.height) * 0.5f;
        int y = (int)(center.y - ButtonHeight * 0.5f);

        int yInc = ButtonHeight + 10;
        string text;
        
        int storedFontSize = GUI.skin.label.fontSize;
        TextAnchor storedAlignment = alignment;

        GUI.skin.label.fontSize = fontSize;
        GUI.skin.label.alignment = TextAnchor.UpperCenter;
        {
            for (int i = 0; i < NumOptions; i++)
            {
                bool highlight = (i == _cursorIndex);
                DrawButton(y, _buttonNames[i], highlight);
                y += yInc;
            }
        }
        GUI.skin.label.fontSize = storedFontSize;
        GUI.skin.label.alignment = storedAlignment;
    }

    void DrawButton(int y, string text, bool highlighted = false)
    {
        Vector2 center = new Vector2(Screen.width, Screen.height) * 0.5f;
        int w = 0;
        int h = ButtonHeight;
        int x = (int)(center.x - w * 0.5f);
        
        Color color = highlighted ? Color.white : new Color(0.3f, 0.3f, 0.3f, 1.0f);
        _stereoscopicGUI.GuiHelper.StereoLabel(x, y, w, h, ref text, color);
    }*/

}
