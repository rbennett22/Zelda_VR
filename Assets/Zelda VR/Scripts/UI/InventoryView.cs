#pragma warning disable 0649 // variable is never assigned to

using UnityEngine;
using UnityEngine.UI;
using System;
using Immersio.Utility;

public class InventoryView : MonoBehaviour
{
    const int ACTIVE_ITEM_ROWS = 2;
    const int ACTIVE_ITEM_COLS = 4;


    public enum DisplayModeEnum
    {
        Overworld,
        Dungeon
    }

    DisplayModeEnum _displayMode;
    public DisplayModeEnum DisplayMode {
        get { return _displayMode; }
        set
        {
            _displayMode = value;

            _overworldView.SetActive(_displayMode == DisplayModeEnum.Overworld);
            _dungeonView.SetActive(_displayMode == DisplayModeEnum.Dungeon);
        }
    }


    [SerializeField]
    OverlayView _bgOverlay;
    [SerializeField]
    GameObject _overworldView, _dungeonView;
    [SerializeField]
    DungeonMapView _dungeonMapView;

    [SerializeField]
    GameObject _itemEquippedB,                                              // Equipped secondary item
               _itemP0, _itemP1, _itemP2, _itemP3, _itemP4, _itemP5,        // Passive items
               _itemA00, _itemA01, _itemA02, _itemA03,                      // Active items
               _itemA10, _itemA11, _itemA12, _itemA13,
               _aux0, _aux1,                                                // Auxillary items
               _tri0, _tri1, _tri2, _tri3, _tri4, _tri5, _tri6, _tri7;      // Triforce pieces

    GameObject[] _passiveItemSlots;
    GameObject[,] _activeItemSlots;
    GameObject[] _auxItemSlots;
    GameObject[] _triforceItemSlots;


    [SerializeField]
    MenuCursor _cursor;
    [SerializeField]
    GameObject _cursorView;

    public Action<InventoryView> onCursorIndexChanged_Callback;


    public bool ShouldDungeonMapRevealUnvisitedRooms { set { _dungeonMapView.DoRenderUnvisitedRooms = value; } }
    public bool ShouldDungeonMapRevealTriforceRoom { set { _dungeonMapView.DoRenderTriforceSymbol = value; } }


    void Awake()
    {
        _bgOverlay.gameObject.SetActive(true);

        _passiveItemSlots = new GameObject[] { _itemP0, _itemP1, _itemP2, _itemP3, _itemP4, _itemP5 };
        _activeItemSlots = new GameObject[,] { { _itemA00, _itemA01, _itemA02, _itemA03 }, { _itemA10, _itemA11, _itemA12, _itemA13 } };
        _auxItemSlots = new GameObject[] { _aux0, _aux1 };
        _triforceItemSlots = new GameObject[] { _tri0, _tri1, _tri2, _tri3, _tri4, _tri5, _tri6, _tri7 };

        ClearAllItemSlots();

        DisplayMode = _displayMode;

        _cursor.numColumns = ACTIVE_ITEM_COLS;
        _cursor.numRows = ACTIVE_ITEM_ROWS;
        _cursor.onIndexChanged_Callback = OnCursorIndexChanged;
    }

    public void ClearAllItemSlots()
    {
        foreach (GameObject s in _passiveItemSlots)
        {
            SetTextureForItemSlot(s, null);
        }
        foreach (GameObject s in _activeItemSlots)
        {
            SetTextureForItemSlot(s, null);
        }
        foreach (GameObject s in _auxItemSlots)
        {
            SetTextureForItemSlot(s, null);
        }
        SetTextureForItemSlot(_itemEquippedB, null);

        for (int i = 0; i < _triforceItemSlots.Length; i++)
        {
            int dungeonNum = i + 1;
            SetTriforcePieceVisible(dungeonNum, false);
        }
    }


    public void SetTextureForMappedItem(Texture texture, InventoryViewItemMapping mapping, bool isEquippedItemB = false)
    {
        if(mapping.Type == InventoryViewItemMapping.TypeEnum.Triforce)
        {
            return;
        }

        GameObject itemSlot;
        if (isEquippedItemB)
        {
            itemSlot = _itemEquippedB;
            if (itemSlot == null)
            {
                Debug.LogError("Null itemSlot for itemEquippedB");
                return;
            }
        }
        else
        {
            itemSlot = GetItemSlotForMapping(mapping);
            if (itemSlot == null)
            {
                Debug.LogError("Null itemSlot for mapping: " + mapping.ToString());
                return;
            }
        }

        SetTextureForItemSlot(itemSlot, texture);
    }

    GameObject GetItemSlotForMapping(InventoryViewItemMapping mapping)
    {
        GameObject itemSlot = null;

        switch (mapping.Type)
        {
            case InventoryViewItemMapping.TypeEnum.Passive:     itemSlot = _passiveItemSlots[mapping.Column]; break;
            case InventoryViewItemMapping.TypeEnum.Active:      itemSlot = _activeItemSlots[mapping.Row, mapping.Column]; break;
            case InventoryViewItemMapping.TypeEnum.Auxillary:   itemSlot = _auxItemSlots[mapping.Aux]; break;
            case InventoryViewItemMapping.TypeEnum.Triforce:    break;
            default: break;
        }

        return itemSlot;
    }

    void SetTextureForItemSlot(GameObject itemSlot, Texture texture)
    {
        RawImage img = itemSlot.GetComponent<RawImage>();
        if(img == null)
        {
            Debug.LogError("No RawImage component exists on itemSlot, '" + itemSlot.name + "'");
            return;
        }

        img.texture = texture;
        img.SetNativeSizeWhileRespectingAnchors();
        SetItemSlotVisible(itemSlot, texture != null);
    }

    public void SetTriforcePieceVisible(int dungeonNum, bool value)
    {
        GameObject itemSlot = _triforceItemSlots[dungeonNum - 1];
        SetItemSlotVisible(itemSlot, value);
    }

    void SetItemSlotVisible(GameObject itemSlot, bool value)
    {
        itemSlot.GetComponent<RawImage>().enabled = value;
    }


    public Index2 CursorIndex {
        get { return _cursor.CursorIndex; }
        set { _cursor.CursorIndex = value; }
    }
    void OnCursorIndexChanged(MenuCursor sender)
    {
        if (sender != _cursor)
        {
            return;
        }

        // Reposition the cursor view
        GameObject itemSlot = _activeItemSlots[sender.CursorIndex.y, sender.CursorIndex.x];
        _cursorView.transform.position = itemSlot.transform.position;

        // Notify our delegate
        if (onCursorIndexChanged_Callback != null)
        {
            onCursorIndexChanged_Callback(this);
        }
    }

    public void MoveCursor(Vector2 vec)
    {
        if (_cursor.TryMoveCursor(vec))
        {
            PlayCursorMoveSound();
        }
    }
    public void MoveCursor(Index2.Direction dir)
    {
        if (_cursor.TryMoveCursor(dir))
        {
            PlayCursorMoveSound();
        }
    }


    public void UpdateDungeonMap()
    {
        _dungeonMapView.UpdateMap();
    }


    void PlayCursorMoveSound()
    {
        SoundFx.Instance.PlayOneShot(SoundFx.Instance.cursor);
    }
}
