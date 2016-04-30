using UnityEngine;
using UnityEngine.UI;

public class GameplayHUDView : MonoBehaviour
{
    public enum DisplayModeEnum
    {
        Overworld,
        Dungeon
    }


    [SerializeField]
    DungeonMapView _dungeonMapView;
    [SerializeField]
    OverworldMapView _overworldMapView;
    [SerializeField]
    GameObject _equippedItemA, _equippedItemB;
    [SerializeField]
    HeartsView _heartsView;
    [SerializeField]
    ZeldaText _rupeesText, _keysText, _bombsText;
    [SerializeField]
    ZeldaText _levelNumText;


    DisplayModeEnum _displayMode;


    void Awake()
    {
        UpdateTextureForEquippedItemSlotA(null);
        UpdateTextureForEquippedItemSlotB(null);

        UpdateRupeesCountText(0);
        UpdateKeysCountText(0);
        UpdateBombsCountText(0);

        DisplayMode = _displayMode;
    }


    public DisplayModeEnum DisplayMode
    {
        get { return _displayMode; }
        set
        {
            _displayMode = value;

            _overworldMapView.gameObject.SetActive(_displayMode == DisplayModeEnum.Overworld);
            _dungeonMapView.gameObject.SetActive(_displayMode == DisplayModeEnum.Dungeon);
        }
    }

    public void UpdateTextureForEquippedItemSlotA(Texture texture)
    {
        SetTextureForEquippedItemSlot(_equippedItemA, texture);
    }
    public void UpdateTextureForEquippedItemSlotB(Texture texture)
    {
        SetTextureForEquippedItemSlot(_equippedItemB, texture);
    }

    public void UpdateHeartContainerCount(int amount) { _heartsView.UpdateHeartContainerCount(amount); }
    public void UpdateHeartContainersFillState(int numHalfHearts) { _heartsView.UpdateHeartContainersFillState(numHalfHearts); }

    public void UpdateRupeesCountText(int amount)
    {
        string amountStr = amount.ToString();
        _rupeesText.Text = (amount < 100) ? "x" + amountStr : amountStr;
    }
    
    public void UpdateKeysCountText(int amount)
    {
        _keysText.Text = "x" + amount.ToString();
    }
    public void UpdateKeysCountText_SetToInfinite()
    {
        _keysText.Text = "xA";
    }

    public void UpdateBombsCountText(int amount)
    {
        _bombsText.Text = "x" + amount.ToString();
    }

    public void UpdateLevelNumText(int num)
    {
        _levelNumText.Text = "LEVEL-" + num.ToString();
    }


    public void InitOverworldMap(int sectorsWide, int sectorsHigh)
    {
        _overworldMapView.Init(sectorsWide, sectorsHigh);
    }
    public void UpdateOverworldMap(Vector2 playerOccupiedSector)
    {
        _overworldMapView.UpdateMap(playerOccupiedSector);
    }
    

    public bool ShouldDungeonMapRevealUnvisitedRooms { set { _dungeonMapView.DoRenderUnvisitedRooms = value; } }
    public bool ShouldDungeonMapRevealTriforceRoom { set { _dungeonMapView.DoRenderTriforceSymbol = value; } }

    public void InitDungeonMap(int sectorsWide, int sectorsHigh)
    {
        _dungeonMapView.Init(sectorsWide, sectorsHigh);
    }
    public void UpdateDungeonMap()
    {
        _dungeonMapView.UpdateMap();
    }


    void SetTextureForEquippedItemSlot(GameObject itemSlot, Texture texture)
    {
        RawImage img = itemSlot.GetComponent<RawImage>();
        if (img == null)
        {
            Debug.LogError("No RawImage component exists on equippedItemSlot, '" + itemSlot.name + "'");
            return;
        }

        img.texture = texture;
        img.SetNativeSizeWhileRespectingAnchors();
        SetEquippedItemSlotVisible(itemSlot, texture != null);
    }
    void SetEquippedItemSlotVisible(GameObject itemSlot, bool value)
    {
        itemSlot.GetComponent<RawImage>().enabled = value;
    }
}