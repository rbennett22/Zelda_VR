using UnityEngine;
using Immersio.Utility;


public class Pause : Singleton<Pause> 
{
    const ZeldaInput.Button ShowInventory_Button = ZeldaInput.Button.Pause;
    const ZeldaInput.Button ShowMenu_Button = ZeldaInput.Button.Select;


    public bool IsAllowed { get; set; }

    public bool IsTimeFrozen { get; private set; }

    public bool IsInventoryShowing { get; private set; }
    public bool IsMenuShowing { get; private set; }


    public void ForceHideInventory()
    {
        HideInventory(true);
            
        if (!IsMenuShowing)
        {
            UnfreezeTime();
        }
    }

    public void ForceHideMenu()
    {
        HideMenu(true);
        
        if (!IsInventoryShowing)
        {
            UnfreezeTime();
        }
    }


    void Update()
    {
        if (!IsAllowed) { return; }

        if (ZeldaInput.GetButtonDown(ShowMenu_Button))
        {
            ToggleMenu();
            if (!IsInventoryShowing)
            {
                ToggleTimeFreeze();
            }
        }

        if (ZeldaInput.GetButtonDown(ShowInventory_Button))
        {
            if (!IsMenuShowing)
            {
                ToggleTimeFreeze();
                ToggleInventory();
            }
        }
    }


    void ToggleTimeFreeze()
    {
        if (IsTimeFrozen) { UnfreezeTime(); }
        else { FreezeTime(); }
    }

    void FreezeTime()
    {
        if (IsTimeFrozen || !IsAllowed) { return; }

        Time.timeScale = 0;
        IsTimeFrozen = true;
    }

    void UnfreezeTime(bool force = false)
    {
        if (!force)
        {
            if (!IsTimeFrozen || !IsAllowed) { return; }
        }

        Time.timeScale = 1;
        IsTimeFrozen = false;
    }


    void ToggleInventory()
    {
        if (IsInventoryShowing) { HideInventory(); }
        else { ShowInventory(); }
        PlayPauseSound();
    }

    void ShowInventory()
    {
        if (IsInventoryShowing || !IsAllowed) { return; }

        IsInventoryShowing = true;
    }

    void HideInventory(bool force = false)
    {
        if (!force)
        {
            if (!IsInventoryShowing || !IsAllowed) { return; }
        }

        IsInventoryShowing = false;
    }


    void ToggleMenu()
    {
        if (IsMenuShowing) { HideMenu(); }
        else { ShowMenu(); }
        PlayPauseSound();
    }

    void ShowMenu()
    {
        if (IsMenuShowing || !IsAllowed) { return; }

        PauseMenu.Instance.Show();
        IsMenuShowing = true;
    }

    void HideMenu(bool force = false)
    {
        if (!force)
        {
            if (!IsMenuShowing || !IsAllowed) { return; }
        }

        PauseMenu.Instance.Hide();
        IsMenuShowing = false;
    }


    void PlayPauseSound()
    {
        SoundFx.Instance.PlayOneShot(SoundFx.Instance.pause);
    }

}