using UnityEngine;

[RequireComponent(typeof(DebugOptions))]

public class DebugOptions_Zelda : MonoBehaviour
{
    public bool useRandysPreferredDebugOptions;


    DebugOption[] _options = {
        new DebugOption(KeyCode.F2, "God Mode", ToggleGodMode),
        new DebugOption(KeyCode.F3, "Invincibility", ToggleInvincibility),
        new DebugOption(KeyCode.F4, "Ghost Mode", ToggleGhostMode),

        new DebugOption(KeyCode.F5, "Air Jumping", ToggleAirJumping),
        new DebugOption(KeyCode.F6, "Moon Mode", ToggleMoonMode),

        new DebugOption(KeyCode.F7, "Secret Detection Mode", ToggleSecretDetectionMode),
        
        DebugOption.EmptyOption(),

        new DebugOption(KeyCode.F8, "Return To Ground Level", ReturnToGroundLevel, false, true),
        new DebugOption(KeyCode.F9, "Increase Run Multiplier", IncreaseRunMultiplier, false, true),
        new DebugOption(KeyCode.F10, "Increase Jump Height", IncreaseJumpHeight, false, true),

        new DebugOption(KeyCode.F11, "Max Out Inventory", MaxOutInventory, false, true),
        new DebugOption(KeyCode.F12, "Max Out Rupees", MaxOutRupees, false, true),

        new DebugOption(KeyCode.Equals, "Restore Player Health", RestorePlayerHealth, false, true),
        new DebugOption(KeyCode.Minus, "Damage Player", DamagePlayer, false, true),
        new DebugOption(KeyCode.Alpha0, "Kill Player", KillPlayer, false, true),

        DebugOption.EmptyOption(),
    };


    static Cheats CheatsInstance { get { return Cheats.Instance; } }

    static bool ToggleGodMode(bool value)
    {
        CheatsInstance.ToggleGodMode(value);
        return true;
    }
    static bool ToggleInvincibility(bool value)
    {
        CheatsInstance.ToggleInvincibility(value);
        return true;
    }
    static bool MaxOutInventory(bool value)
    {
        CheatsInstance.MaxOutInventory();
        return true;
    }
    static bool ToggleGhostMode(bool value)
    {
        CheatsInstance.ToggleGhostMode(value);
        return true;
    }
    static bool IncreaseRunMultiplier(bool value)
    {
        CheatsInstance.IncreaseRunMultiplier();
        return true;
    }
    static bool IncreaseJumpHeight(bool value)
    {
        CheatsInstance.IncreaseJumpHeight();
        return true;
    }
    static bool ToggleAirJumping(bool value)
    {
        CheatsInstance.ToggleAirJumping(value);
        return true;
    }
    static bool ToggleMoonMode(bool value)
    {
        CheatsInstance.ToggleMoonMode(value);
        return true;
    }
    static bool RestorePlayerHealth(bool value)
    {
        CheatsInstance.RestorePlayerHealth();
        return true;
    }
    static bool DamagePlayer(bool value)
    {
        CheatsInstance.DamagePlayer();
        return true;
    }
    static bool KillPlayer(bool value)
    {
        CheatsInstance.KillPlayer();
        return true;
    }
    static bool ToggleSecretDetectionMode(bool value)
    {
        CheatsInstance.ToggleSecretDetectionMode(value);
        return true;
    }
    static bool ReturnToGroundLevel(bool value)
    {
        CheatsInstance.ReturnToGroundLevel();
        return true;
    }
    static bool MaxOutRupees(bool value)
    {
        CheatsInstance.MaxOutRupees();
        return true;
    }


    DebugOptions _debugOptions;


    void Awake()
    {
        _debugOptions = GetComponent<DebugOptions>();
        _debugOptions.Options.AddRange(_options);

        ApplyCurrentSettings();
    }

    void OnLevelWasLoaded(int level)
    {
        ApplyCurrentSettings();
    }

    void ApplyCurrentSettings()
    {
        _debugOptions.ApplyCurrentSettings();
        if (useRandysPreferredDebugOptions)
        {
            ApplyRandysPrefferredSettings();
        }
    }

    void ApplyRandysPrefferredSettings()
    {
        _debugOptions.ActivateOptionByName("God Mode", true);
        _debugOptions.ActivateOptionByName("God Mode", true);
    }
}