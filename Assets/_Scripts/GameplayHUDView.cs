using UnityEngine;
using UnityEngine.UI;

public class GameplayHUDView : MonoBehaviour
{
    public enum DisplayModeEnum
    {
        Overworld,
        Dungeon
    }

    DisplayModeEnum _displayMode;
    public DisplayModeEnum DisplayMode
    {
        get { return _displayMode; }
        set
        {
            _displayMode = value;

            //_overworldMapView.gameObject.SetActive(_displayMode == DisplayModeEnum.Overworld);
            _dungeonMapView.gameObject.SetActive(_displayMode == DisplayModeEnum.Dungeon);
        }
    }


    [SerializeField]
    DungeonMapView _dungeonMapView;
    //[SerializeField]
    //OverworldMapView _overworldMapView;

    [SerializeField]
    GameObject _equippedItemA, _equippedItemB;


    [SerializeField]
    Sprite _fullHeartSprite, _halfHeartSprite, _emptyHeartSprite;


    public float alpha = 1.0f;
    public int vertShiftSpeed = 600;


    float _yBaseOffset = 300;
    public int PausedYVal           // Where to vertically position HUD when game is Paused
    {
        get { return (int)(Screen.height * 0.7f + _yBaseOffset); }    
    }


    Texture _fullHeartImage, _halfHeartImage, _emptyHeartImage;


    void Awake()
    {
        _fullHeartImage = _fullHeartSprite.GetTextureSegment();
        _halfHeartImage = _halfHeartSprite.GetTextureSegment();
        _emptyHeartImage = _emptyHeartSprite.GetTextureSegment();

        SetTextureForEquippedItemSlotA(null);
        SetTextureForEquippedItemSlotB(null);

        DisplayMode = _displayMode;
    }


    public void SetTextureForEquippedItemSlotA(Texture texture)
    {
        SetTextureForEquippedItemSlot(_equippedItemA, texture);
    }
    public void SetTextureForEquippedItemSlotB(Texture texture)
    {
        SetTextureForEquippedItemSlot(_equippedItemB, texture);
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