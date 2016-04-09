using UnityEngine;
using UnityEngine.UI;

public class PauseMenuUI : MonoBehaviour
{
    [SerializeField]
    Button _resumeButton;


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
}
