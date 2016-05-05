using UnityEngine;

public class ZeldaVRSettings : SettingsObject<ZeldaVRSettings>
{
    // Overworld Dimensions
    [SerializeField] public int overworldWidthInSectors = 16;
    [SerializeField] public int overworldHeightInSectors = 8;  
    [SerializeField] public int overworldSectorWidthInTiles = 16;  
    [SerializeField] public int overworldSectorHeightInTiles = 11;

    // TileMap Dimensions
    [SerializeField] public int tileMapTileWidthInPixels = 17;
    [SerializeField] public int tileMapTileHeightInPixels = 17;     // NOTE: Uniblock requires the tileMapTileWidthInPixels to equal tileMapTileHeightInPixels
    [SerializeField] public int tileMapSideLengthInTiles = 20;
    [SerializeField] public int tileMapWidthInTiles_WithoutFiller = 18;   // NOTE: Uniblock requires the tileMap width and height to be the same.  Therefore much of the texture is just "filler"
    [SerializeField] public int tileMapHeightInTiles_WithoutFiller = 8;

    // Block Heights
    [SerializeField] public int blockHeight = 4;
    [SerializeField] public float blockHeightVariance = 2;
    [SerializeField] public float shortBlockHeight = 1;
    [SerializeField] public float flatBlockHeight = 0;

    // Other
    [SerializeField] public int collectibleRemovalDistance = 36;


    // Dungeon Dimensions
    [SerializeField] public int dungeonWidthInSectors = 8;
    [SerializeField] public int dungeonHeightInSectors = 8;
}