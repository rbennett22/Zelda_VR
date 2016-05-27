using Immersio.Utility;
using System;
using UnityEngine;

//[RequireComponent(typeof(InventoryView))]

public class InventoryViewController : MonoBehaviour
{
    [SerializeField]
    InventoryView _view;

    Inventory _inventory;


    public Action<MenuCursor> onCursorIndexChanged_Callback;


    void Awake()
    {
        //_view = GetComponent<InventoryView>();
        _inventory = Inventory.Instance;

        _view.gameObject.SetActive(false);
        _view.onCursorIndexChanged_Callback = OnCursorIndexChanged;
    }


    void Update()
    {
        if (PauseManager.Instance.IsPaused_Options)
        {
            return;
        }

        if (!IsViewShowing)
        {
            return;
        }

        // TODO: Don't update every frame

        UpdateCursor();
        UpdateView();
    }

    public bool IsViewShowing { get; private set; }
    public void ShowView()
    {
        if (IsViewShowing)
        {
            return;
        }
        IsViewShowing = true;

        _view.gameObject.SetActive(true);
        PlayInventoryToggleSound();
    }
    public void HideView()
    {
        if (!IsViewShowing)
        {
            return;
        }
        IsViewShowing = false;

        _view.gameObject.SetActive(false);
        PlayInventoryToggleSound();
    }


    void UpdateView()
    {
        UpdateView_Items();

        if (WorldInfo.Instance.IsOverworld)
        {
            _view.DisplayMode = InventoryView.DisplayModeEnum.Overworld;

            UpdateView_TriforcePieces();
        }
        else if (WorldInfo.Instance.IsInDungeon)
        {
            _view.DisplayMode = InventoryView.DisplayModeEnum.Dungeon;

            UpdateView_DungeonMapAndItems();
        }
    }

    void UpdateView_Items()
    {
        _view.ClearAllItemSlots();

        foreach (Item item in _inventory.Items.Values)
        {
            if (!ShouldRenderItem(item)) { continue; }

            InventoryViewItemMapping mapping = item.GetComponent<InventoryViewItemMapping>();
            if (mapping == null)
            {
                Debug.LogError("Null InventoryViewItemMapping for item: " + item.name);
                continue;
            }

            DoRenderItem(item, mapping);
        }

        UpdateView_EquippedItemB();
    }
    bool ShouldRenderItem(Item item)
    {
        if (!item.AppearsInInventoryGUI) { return false; }
        if (item.count == 0) { return false; }
        if (!item.IsTheHighestUpgradeInInventory()) { return false; }
        if (item.name == "Map" || item.name == "Compass" || item.name == "Triforce") { return false; }

        return true;
    }
    void DoRenderItem(Item item, InventoryViewItemMapping mapping, bool isEquippedItemB = false)
    {
        Texture texture = item.GetGuiTexture();
        if (texture == null)
        {
            Debug.LogError("Null texture for Item: " + item.name);
            return;
        }

        _view.SetTextureForMappedItem(texture, mapping, isEquippedItemB);
    }

    void UpdateView_EquippedItemB()
    {
        Item item = _inventory.EquippedItemB;
        if (item == null)
        {
            return;
        }

        InventoryViewItemMapping mapping = item.GetComponent<InventoryViewItemMapping>();
        DoRenderItem(item, mapping, true);
    }

    void UpdateView_TriforcePieces()
    {
        for (int i = 0; i < 8; i++)
        {
            int dungeonNum = i + 1;
            bool showTriPiece = _inventory.HasTriforcePieceForDungeon(dungeonNum);
            _view.SetTriforcePieceVisible(dungeonNum, showTriPiece);
        }
    }

    void UpdateView_DungeonMapAndItems()
    {
        int dungeonNum = WorldInfo.Instance.DungeonNum;
        bool hasMap = _inventory.HasMapForDungeon(dungeonNum);
        bool hasCompass = _inventory.HasCompassForDungeon(dungeonNum);

        if (hasMap)
        {
            Item item = _inventory.GetItem("Map");
            InventoryViewItemMapping mapping = item.GetComponent<InventoryViewItemMapping>();
            DoRenderItem(item, mapping);
        }

        if (hasCompass)
        {
            Item item = _inventory.GetItem("Compass");
            InventoryViewItemMapping mapping = item.GetComponent<InventoryViewItemMapping>();
            DoRenderItem(item, mapping);
        }

        // Dungeon Map
        _view.ShouldDungeonMapRevealUnvisitedRooms = hasMap;
        _view.ShouldDungeonMapRevealTriforceRoom = hasCompass;
        _view.UpdateDungeonMap();
    }


    void UpdateCursor()
    {
        float moveHorz = ZeldaInput.GetAxis(ZeldaInput.Axis.MoveHorizontal);
        float moveVert = ZeldaInput.GetAxis(ZeldaInput.Axis.MoveVertical);
        IndexDirection2 dir = new IndexDirection2(moveHorz, moveVert);

        _view.MoveCursor(dir);
    }
    void OnCursorIndexChanged(InventoryView sender)
    {
        if (sender != _view)
        {
            return;
        }

        Index2 idx = sender.CursorIndex;
        _inventory.EquippedItemB = _inventory.GetEquippableSecondaryItem(idx.x, idx.y);
    }


    void PlayInventoryToggleSound()
    {
        SoundFx.Instance.PlayOneShot(SoundFx.Instance.pause);
    }
}