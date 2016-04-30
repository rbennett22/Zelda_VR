using UnityEngine;

public class ZeldaVRSettings : SettingsObject<ZeldaVRSettings>
{
    [Tooltip("...")]
    [SerializeField] public int overworldWidthInSectors = 16;
    [Tooltip("...")]
    [SerializeField] public int overworldHeightInSectors = 8;  
    [Tooltip("...")]
    [SerializeField] public int overworldSectorWidthInTiles = 16;  
    [Tooltip("...")]
    [SerializeField] public int overworldSectorHeightInTiles = 11;

    [Tooltip("...")]
    [SerializeField] public int tileRemovalDistance = 36;

    [Tooltip("...")]
    [SerializeField] public int dungeonWidthInSectors = 8;
    [Tooltip("...")]
    [SerializeField] public int dungeonHeightInSectors = 8;
}