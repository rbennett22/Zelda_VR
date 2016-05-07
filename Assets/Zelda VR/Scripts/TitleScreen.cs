using UnityEngine;


public class TitleScreen : MonoBehaviour
{

    public TextMesh versionText;
    public GameObject loadSelectScreen;


    void Start()
    {
        loadSelectScreen.SetActive(false);

        if (versionText != null)
        {
            ZeldaConfig config = ZeldaConfig.Instance;
            if (config.isDemo)
            {
                versionText.text = "BETA v" + config.version.ToString("F1");
            }
            else
            {
                versionText.text = string.Empty;
            }
        }

        PauseManager.Instance.IsPauseAllowed_Inventory = false;
        PauseManager.Instance.IsPauseAllowed_Options = false;
        CommonObjects.PlayerController_C.SetHaltUpdateMovement(true);
    }


	void Update () 
    {
        if (ZeldaInput.GetButtonDown(ZeldaInput.Button.Start) || Input.GetKeyDown(KeyCode.Return))
        {
            ShowLoadSelectScreen();
            //StartNewGame();
        }
	}

    void ShowLoadSelectScreen()
    {
        Music.Instance.Stop();

        loadSelectScreen.SetActive(true);
        gameObject.SetActive(false);
    }

    /*void StartNewGame()
    {
        SoundFx.Instance.PlayOneShot(SoundFx.Instance.select);

        CommonObjects.PlayerController_C.controlsEnabled = true;
        GameplayHUD.Instance.enabled = true;

        Locations.Instance.LoadInitialScene();
    }*/

}