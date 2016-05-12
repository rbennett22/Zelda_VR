#pragma warning disable 0649 // variable is never assigned to

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System;
using Immersio.Utility;

public class OptionsView : MonoBehaviour
{
    const int DEFAULT_CURSOR_INDEX = 0;

    static Color DESELECTED_BTN_COLOR = Color.white;
    static Color SELECTED_BTN_COLOR = Color.red;


    [SerializeField]
    OverlayView _bgOverlay;
    [SerializeField]
    Button _resumeBtn, _musicBtn, _controlsBtn, _quitBtn;
    Button[] _allButtons;
    Button _selectedBtn;


    /*public Button ResumeButton { get { return _resumeBtn; } }
    public Button MusicBtn { get { return _musicBtn; } }
    public Button ControlsBtn { get { return _controlsBtn; } }
    public Button QuitBtn { get { return _quitBtn; } }*/


    [SerializeField]
    MenuCursor _cursor;
    [SerializeField]
    GameObject _cursorView;

    public Action<OptionsView> onCursorIndexChanged_Callback;


    void Awake()
    {
        _allButtons = new Button[] { _resumeBtn, _musicBtn, _controlsBtn, _quitBtn };

        _bgOverlay.gameObject.SetActive(true);

        _cursor.numColumns = 1;
        _cursor.numRows = _allButtons.Length;
        _cursor.onIndexChanged_Callback = OnCursorIndexChanged;

        CursorIndex = DEFAULT_CURSOR_INDEX;
        OnCursorIndexChanged(_cursor);
    }

    void OnEnable()
    {
        CursorIndex = DEFAULT_CURSOR_INDEX;
        OnCursorIndexChanged(_cursor);
    }


    public void AddListener_OnResumeClicked(UnityAction action)
    {
        _resumeBtn.onClick.AddListener(action);
    }
    public void AddListener_OnMusicClicked(UnityAction action)
    {
        _musicBtn.onClick.AddListener(action);
    }
    public void AddListener_OnControlsClicked(UnityAction action)
    {
        _controlsBtn.onClick.AddListener(action);
    }
    public void AddListener_OnQuitClicked(UnityAction action)
    {
        _quitBtn.onClick.AddListener(action);
    }


    void Update()
    {
        if (_selectedBtn != null)
        {
            // Reposition the cursor view
            // For some reason this doesn't happen if called from OnEnable, so we just call it every frame in Update for now
            _cursorView.transform.SetY(_selectedBtn.transform.position.y);
        }
    }


    public int CursorIndex
    {
        get { return _cursor.CursorIndex.y; }
        set { _cursor.CursorIndex.y = value; }
    }
    void OnCursorIndexChanged(MenuCursor sender)
    {
        if(sender != _cursor)
        {
            return;
        }

        SelectButton(CursorIndex);

        // Reposition the cursor view
        _cursorView.transform.SetY(_selectedBtn.transform.position.y);

        // Notify our delegate
        if (onCursorIndexChanged_Callback != null)
        {
            onCursorIndexChanged_Callback(this);
        }
    }

    void SelectButton(int index)
    {
        SelectButton(_allButtons[index]);
    }
    void SelectButton(Button newSelectedBtn)
    {
        Button prevSelectedBtn = _selectedBtn;
        if(newSelectedBtn == prevSelectedBtn)
        {
            return;
        }

        // Deselect previous
        if (prevSelectedBtn != null)
        {
            prevSelectedBtn.GetComponentInChildren<Image>().color = DESELECTED_BTN_COLOR;
        }

        // Select new
        _selectedBtn = newSelectedBtn;
        if (_selectedBtn != null)
        {
            _selectedBtn.GetComponentInChildren<Image>().color = SELECTED_BTN_COLOR;
        }
    }

    public void MoveCursor(Vector2 vec)
    {
        _cursor.TryMoveCursor(vec);
    }
    public void MoveCursor(Index2.Direction dir)
    {
        _cursor.TryMoveCursor(dir);
    }


    public void ClickSelectedButton()
    {
        if (_selectedBtn != null)
        {
            PointerEventSimulator.SimulateClick(_selectedBtn.gameObject);
        }
    }
}