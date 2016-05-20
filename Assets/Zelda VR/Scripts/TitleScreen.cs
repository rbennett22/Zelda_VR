using UnityEngine;

public class TitleScreen : MonoBehaviour
{
    const string VERSION_STRING = "Version {0}.{1}.{2}";


    public ZeldaText versionText;
    public GameObject savedGamesScreen;


    void Start()
    {
        savedGamesScreen.SetActive(false);

        UpdateVersionText();

        PauseManager.Instance.IsPauseAllowed_Inventory = false;
        PauseManager.Instance.IsPauseAllowed_Options = false;
        CommonObjects.PlayerController_C.SetHaltUpdateMovement(true);
    }

    void UpdateVersionText()
    {
        ZeldaConfig c = ZeldaConfig.Instance;
        versionText.Text = string.Format(VERSION_STRING, c.version, c.subVersion, c.subSubVersion);
    }


    void Update()
    {
        if (ZeldaInput.GetButtonDown(ZeldaInput.Button.Start))
        {
            ShowSavedGamesScreen();
        }
    }

    void ShowSavedGamesScreen()
    {
        //Music.Instance.Stop();

        savedGamesScreen.SetActive(true);
        gameObject.SetActive(false);
    }
}