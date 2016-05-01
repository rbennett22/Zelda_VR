using UnityEngine;

public class ZeldaVRSettings : SettingsObject<ZeldaVRSettings>
{
    // Overworld Dimensions
    [Tooltip("...")]
    [SerializeField] public int overworldWidthInSectors = 16;
    [Tooltip("...")]
    [SerializeField] public int overworldHeightInSectors = 8;  
    [Tooltip("...")]
    [SerializeField] public int overworldSectorWidthInTiles = 16;  
    [Tooltip("...")]
    [SerializeField] public int overworldSectorHeightInTiles = 11;

    // TileMap Dimensions
    [Tooltip("...")]
    [SerializeField] public int tileMapTileWidthInPixels = 17;
    [Tooltip("...")]
    [SerializeField] public int tileMapTileHeightInPixels = 17;     // NOTE: Uniblock requires the tileMapTileWidthInPixels to equal tileMapTileHeightInPixels
    [Tooltip("...")]
    [SerializeField] public int tileMapSideLengthInTiles = 20;
    [Tooltip("...")]
    [SerializeField] public int tileMapWidthInTiles_WithoutFiller = 18;   // NOTE: Uniblock requires the tileMap width and height to be the same.  Therefore much of the texture is just "filler"
    [Tooltip("...")]
    [SerializeField] public int tileMapHeightInTiles_WithoutFiller = 8;     

    // Other
    [Tooltip("...")]
    [SerializeField]
    public int tileRemovalDistance = 36;


    // Dungeon Dimensions
    [Tooltip("...")]
    [SerializeField] public int dungeonWidthInSectors = 8;
    [Tooltip("...")]
    [SerializeField] public int dungeonHeightInSectors = 8;
}