using Immersio.Utility;
using UnityEngine;

public class PauseManager : Singleton<PauseManager>
{
    const ZeldaInput.Button SHOW_OPTIONS_BUTTON = ZeldaInput.Button.Back;
    const ZeldaInput.Button SHOW_INVENTORY_BUTTON = ZeldaInput.Button.Start;


    [SerializeField]
    OptionsViewController _optionsViewController;

    [SerializeField]
    InventoryViewController _inventoryViewController;


    public bool IsPauseAllowed_Options { get; set; }
    public bool IsPaused_Options { get; private set; }
    public void PauseGame_Options()
    {
        if (IsPaused_Options)
            return;
        IsPaused_Options = true;

        _optionsViewController.ShowView();
        RefreshFreezeState();
    }
    public void ResumeGame_Options()
    {
        if (!IsPaused_Options)
            return;
        IsPaused_Options = false;

        _optionsViewController.HideView();
        RefreshFreezeState();
    }
    public void TogglePause_Options()
    {
        if (IsPaused_Options)
            ResumeGame_Options();
        else
            PauseGame_Options();
    }

    public bool IsPauseAllowed_Inventory { get; set; }
    public bool IsPaused_Inventory { get; private set; }
    public void PauseGame_Inventory()
    {
        if (IsPaused_Inventory)
            return;
        IsPaused_Inventory = true;

        _inventoryViewController.ShowView();
        RefreshFreezeState();
    }
    public void ResumeGame_Inventory()
    {
        if (!IsPaused_Inventory)
            return;
        IsPaused_Inventory = false;

        _inventoryViewController.HideView();
        RefreshFreezeState();
    }
    public void TogglePause_Inventory()
    {
        if (IsPaused_Inventory)
            ResumeGame_Inventory();
        else
            PauseGame_Inventory();
    }

    public bool IsPaused_Any { get { return IsPaused_Options || IsPaused_Inventory; } }


    void Update()
    {
        if (ZeldaInput.GetButtonDown(SHOW_OPTIONS_BUTTON))
        {
            if (IsPauseAllowed_Options)
            {
                TogglePause_Options();
            }
        }
        else if (ZeldaInput.GetButtonDown(SHOW_INVENTORY_BUTTON))
        {
            if (IsPauseAllowed_Inventory && !IsPaused_Options)
            {
                TogglePause_Inventory();
            }
        }
    }


    void RefreshFreezeState()
    {
        if (IsPaused_Any)
        {
            FreezeTime();
        }
        else
        {
            UnfreezeTime();
        }
    }

    bool _isTimeFrozen;
    void FreezeTime()
    {
        if (_isTimeFrozen) { return; }

        Time.timeScale = 0;
        _isTimeFrozen = true;
    }
    void UnfreezeTime()
    {
        if (!_isTimeFrozen) { return; }

        Time.timeScale = 1;
        _isTimeFrozen = false;
    }
}