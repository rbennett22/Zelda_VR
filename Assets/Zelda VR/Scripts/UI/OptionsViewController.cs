using Immersio.Utility;
using UnityEngine;

public class OptionsViewController : Singleton<OptionsViewController>
{
    const string OPTIONS_VIEW_PREFAB_PATH = "Options View";


    [SerializeField]
    OptionsView _view;

    OptionsView View { get { return _view ?? (_view = InstantiateView(OPTIONS_VIEW_PREFAB_PATH, _canvasOffsetT)); } }
    OptionsView InstantiateView(string prefabPath, Transform canvasOffsetT = null)
    {
        if (ViewCanvas == null)
        {
            Debug.LogWarning("ViewCanvas is null");
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
        t.SetParent(ViewCanvas.transform);

        if (canvasOffsetT != null)
        {
            t.localPosition = canvasOffsetT.localPosition;
            t.localRotation = canvasOffsetT.localRotation;
            t.localScale = canvasOffsetT.localScale;
        }
        else
        {
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;
            t.localScale = Vector3.one;
        }


        OptionsView view = g.GetComponent<OptionsView>();
        AddButtonClickListeners(view);

        return view;
    }


    [SerializeField]
    Transform _canvasOffsetT;       // Offset is only applied if view is instantiated at runtime


    Canvas ViewCanvas { get { return CommonObjects.Instance.headSpaceCanvas; } }


    override protected void Awake()
    {
        base.Awake();

        if (_view != null)
        {
            AddButtonClickListeners(_view);
        }

        HideView();
    }


    public bool IsViewShowing { get; private set; }
    public void ShowView()
    {
        IsViewShowing = true;

        View.gameObject.SetActive(true);
    }
    public void HideView()
    {
        IsViewShowing = false;

        View.gameObject.SetActive(false);
    }


    #region Button Click Handlers

    void AddButtonClickListeners(OptionsView view)
    {
        view.AddListener_OnResumeClicked(Resume);
        view.AddListener_OnMusicClicked(ToggleMusic);
        view.AddListener_OnControlsClicked(DisplayControls);
        view.AddListener_OnQuitClicked(SaveAndQuit);
    }


    public void Resume()
    {
        PauseManager.Instance.ResumeGame_Options();
    }

    public void ToggleMusic()
    {
        Music.Instance.ToggleEnabled();
    }

    public void DisplayControls()
    {
        // TODO
    }

    public void SaveAndQuit()
    {
        SaveManager.Instance.SaveGame();

        PauseManager.Instance.ResumeGame_Options();
        PauseManager.Instance.ResumeGame_Inventory();

        Locations.Instance.LoadTitleScreen();
    }

    #endregion Button Click Handlers


    void Update()
    {
        if (!IsViewShowing)
        {
            return;
        }

        UpdateCursor();

        if (ZeldaInput.GetButtonDown(ZeldaInput.Button.Start)
            || ZeldaInput.GetButtonDown(ZeldaInput.Button.SwordAttack))
        {
            _view.ClickSelectedButton();
        }
    }

    void UpdateCursor()
    {
        float moveVert = ZeldaInput.GetAxis(ZeldaInput.Axis.MoveVertical);
        _view.MoveCursor(new Vector2(0, moveVert));
    }
}