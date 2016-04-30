using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class OptionsView : MonoBehaviour
{
    [SerializeField]
    OverlayView _bgOverlay;
    [SerializeField]
    Button _resumeBtn, _musicBtn, _controlsBtn, _quitBtn;


    /*public Button ResumeButton { get { return _resumeBtn; } }
    public Button MusicBtn { get { return _musicBtn; } }
    public Button ControlsBtn { get { return _controlsBtn; } }
    public Button QuitBtn { get { return _quitBtn; } }*/


    void Awake()
    {
        _bgOverlay.gameObject.SetActive(true);
    }

    void OnEnable()
    {
        EventSystem.current.SetSelectedGameObject(_resumeBtn.gameObject);
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
}
