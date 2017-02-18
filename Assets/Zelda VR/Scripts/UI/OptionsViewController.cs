using Immersio.Utility;
using UnityEngine;

public class OptionsViewController : Singleton<OptionsViewController>
{
    #region OptionsView

    [SerializeField]
    GameObject _optionsViewPrefab;

    OptionsView _optionsView;
    public OptionsView OptionsView { get { return _optionsView ?? (_optionsView = InstantiateOptionsView(_optionsViewPrefab)); } }
    OptionsView InstantiateOptionsView(GameObject prefab)
    {
        Transform parent = CommonObjects.ActiveCanvas.OptionsViewContainer;
        GameObject g = ZeldaViewController.InstantiateView(prefab, parent);
        OptionsView v = g.GetComponent<OptionsView>();
        AddButtonClickListeners(v);
        return v;
    }

    #endregion OptionsView


    #region ControlsView

    [SerializeField]
    GameObject _controlsViewPrefab;

    ControlsView _controlsView;
    public ControlsView ControlsView { get { return _controlsView ?? (_controlsView = InstantiateControlsView(_controlsViewPrefab)); } }
    ControlsView InstantiateControlsView(GameObject prefab)
    {
        Transform parent = CommonObjects.ActiveCanvas.ControlsViewContainer;
        GameObject g = ZeldaViewController.InstantiateView(prefab, parent);
        ControlsView v = g.GetComponent<ControlsView>();
        return v;
    }

    #endregion ControlsView


    override protected void Awake()
    {
        base.Awake();

        OptionsViewActive = false;
    }


    public bool OptionsViewActive
    {
        get { return OptionsView.gameObject.activeSelf; }
        set {
            OptionsView.gameObject.SetActive(value);
            if (value == false)
            {
                ControlsViewActive = false;
            }
        }
    }

    bool ControlsViewActive
    {
        get { return ControlsView.gameObject.activeSelf; }
        set { ControlsView.gameObject.SetActive(value); }
    }


    #region Button Click Handlers

    void AddButtonClickListeners(OptionsView view)
    {
        view.AddListener_OnResumeClicked(Resume);
        view.AddListener_OnMusicClicked(ToggleMusic);
        view.AddListener_OnControlsClicked(ToggleControlsView);
        view.AddListener_OnQuitClicked(SaveAndQuit);
    }


    void Resume()
    {
        PauseManager.Instance.ResumeGame_Options();
    }

    void ToggleMusic()
    {
        Music.Instance.enabled = !Music.Instance.enabled;
    }

    void ToggleControlsView()
    {
        ControlsViewActive = !ControlsViewActive;
    }

    void SaveAndQuit()
    {
        SaveManager.Instance.SaveGame();

        PauseManager.Instance.ResumeGame_Options();
        PauseManager.Instance.ResumeGame_Inventory();

        Locations.Instance.GoToTitleScreen();
    }

    #endregion Button Click Handlers


    void Update()
    {
        if (!OptionsViewActive)
        {
            return;
        }

        UpdateCursor();

        if (ZeldaInput.GetCommand_Trigger(ZeldaInput.Cmd_Trigger.MenuNavSelect))
        {
            _optionsView.ClickSelectedButton();
        }
    }

    void UpdateCursor()
    {
        if (ControlsViewActive)
        {
            return;
        }

        float moveVert = ZeldaInput.GetCommand_Float(ZeldaInput.Cmd_Float.MenuNavVertical);
        IndexDirection2 dir = new IndexDirection2(0, moveVert);
        _optionsView.MoveCursor(dir);
    }
}