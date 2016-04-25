using UnityEngine;

public class ZeldaVRSettings : SettingsObject<ZeldaVRSettings>
{
    [SerializeField]
    [Tooltip("...")]
    public int overworldWidthInSectors = 16;
    [SerializeField]
    [Tooltip("...")]
    public int overworldHeightInSectors = 8;

    [SerializeField]
    [Tooltip("...")]
    public int dungeonWidthInSectors = 8;
    [SerializeField]
    [Tooltip("...")]
    public int dungeonHeightInSectors = 8;
}