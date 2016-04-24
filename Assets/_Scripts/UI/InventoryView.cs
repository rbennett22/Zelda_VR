using UnityEngine;
using UnityEngine.UI;

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
    GameObject _overworldView, _dungeonView;
    [SerializeField]
    DungeonMapView _dungeonMapView;

    [SerializeField]
    GameObject _itemEquippedB,
               _itemP0, _itemP1, _itemP2, _itemP3, _itemP4, _itemP5,
               _itemA00, _itemA01, _itemA02, _itemA03,
               _itemA10, _itemA11, _itemA12, _itemA13,
               _aux0, _aux1,
               _tri0, _tri1, _tri2, _tri3, _tri4, _tri5, _tri6, _tri7;

    GameObject[] _passiveItemSlots;
    GameObject[,] _activeItemSlots;
    GameObject[] _auxItemSlots;
    GameObject[] _triforceItemSlots;

    [SerializeField]
    GameObject _cursor;
    int _cursorIndexX, _cursorIndexY;


    public bool ShouldDungeonMapRevealUnvisitedRooms { set { _dungeonMapView.DoRenderUnvisitedRooms = value; } }
    public bool ShouldDungeonMapRevealTriforceRoom { set { _dungeonMapView.DoRenderTriforceSymbol = value; } }


    void Awake()
    {
        _passiveItemSlots = new GameObject[] { _itemP0, _itemP1, _itemP2, _itemP3, _itemP4, _itemP5 };
        _activeItemSlots = new GameObject[,] { { _itemA00, _itemA01, _itemA02, _itemA03 }, { _itemA10, _itemA11, _itemA12, _itemA13 } };
        _auxItemSlots = new GameObject[] { _aux0, _aux1 };
        _triforceItemSlots = new GameObject[] { _tri0, _tri1, _tri2, _tri3, _tri4, _tri5, _tri6, _tri7 };

        ClearAllItemSlots();

        DisplayMode = _displayMode;

        CursorIndices = new Vector2(0, 0);
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


    public void MoveCursor(Vector2 dir)
    {
        if (dir.x < 0) { _cursorIndexX--; }
        else if (dir.x > 0) { _cursorIndexX++; }
        if (_cursorIndexX < 0) { _cursorIndexX += ACTIVE_ITEM_COLS; }
        else if (_cursorIndexX >= ACTIVE_ITEM_COLS) { _cursorIndexX -= ACTIVE_ITEM_COLS; }

        if (dir.y < 0) { _cursorIndexY--; }
        else if (dir.y > 0) { _cursorIndexY++; }
        if (_cursorIndexY < 0) { _cursorIndexY += ACTIVE_ITEM_ROWS; }
        else if (_cursorIndexY >= ACTIVE_ITEM_ROWS) { _cursorIndexY -= ACTIVE_ITEM_ROWS; }

        CursorIndices = new Vector2(_cursorIndexX, _cursorIndexY);
    }

    public Vector2 CursorIndices {
        get { return new Vector2(_cursorIndexX, _cursorIndexY); }
        set
        {
            _cursorIndexX = Mathf.Clamp((int)value.x, 0, ACTIVE_ITEM_COLS - 1);
            _cursorIndexY = Mathf.Clamp((int)value.y, 0, ACTIVE_ITEM_ROWS - 1);

            GameObject itemSlot = _activeItemSlots[_cursorIndexY, _cursorIndexX];
            _cursor.transform.position = itemSlot.transform.position;
        }
    }


    public void UpdateDungeonMap()
    {
        _dungeonMapView.UpdateDungeonMap();
    }
}
