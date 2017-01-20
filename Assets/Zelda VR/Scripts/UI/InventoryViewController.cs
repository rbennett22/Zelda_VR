using Immersio.Utility;
using System;
using System.Collections.Generic;
using UnityEngine;

//[RequireComponent(typeof(InventoryView))]

public class InventoryViewController : MonoBehaviour
{
    #region View

    [SerializeField]
    GameObject _viewPrefab;

    InventoryView _view;
    public InventoryView View { get { return _view ?? (_view = InstantiateView(_viewPrefab)); } }
    InventoryView InstantiateView(GameObject prefab)
    {
        Transform parent = CommonObjects.ActiveCanvas.InventoryViewContainer;
        GameObject g = ZeldaViewController.InstantiateView(prefab, parent);
        InventoryView v = g.GetComponent<InventoryView>();
        v.BGOverlayIsActive = false;    // TODO: Set this to true when not using Touch Controllers
        return v;
    }

    #endregion  // View


    Inventory _inventory;


    public Action<MenuCursor> onCursorIndexChanged_Callback;


    public bool IsViewShowing { get; private set; }
    public void ShowView()
    {
        if (IsViewShowing)
        {
            return;
        }
        IsViewShowing = true;

        View.gameObject.SetActive(true);
        PlayInventoryToggleSound();

        Item itemB = _inventory.EquippedItemB;
        if (itemB != null)
        {
            InventoryViewItemMapping mapping = itemB.GetComponent<InventoryViewItemMapping>();
            View.CursorIndex = new Index2(mapping.Column, mapping.Row);
        }
    }
    public void HideView()
    {
        if (!IsViewShowing)
        {
            return;
        }
        IsViewShowing = false;

        View.gameObject.SetActive(false);
        PlayInventoryToggleSound();
    }


    void Awake()
    {
        _inventory = Inventory.Instance;

        View.gameObject.SetActive(false);
        View.onCursorIndexChanged_Callback = OnCursorIndexChanged;
    }

    void Start()
    {
        ZeldaVRSettings s = ZeldaVRSettings.Instance;
        InitDungeonMap(s.dungeonWidthInSectors, s.dungeonHeightInSectors);
    }

    void InitDungeonMap(int sectorsWide, int sectorsHigh)
    {
        View.InitDungeonMap(sectorsWide, sectorsHigh);
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

    void UpdateCursor()
    {
        int numSelectableItems = GetSelectableItems().Count;
        if (numSelectableItems == 0)
        {
            return;
        }

        float moveHorz = ZeldaInput.GetAxis(ZeldaInput.Axis.MoveHorizontal);
        float moveVert = ZeldaInput.GetAxis(ZeldaInput.Axis.MoveVertical);
        IndexDirection2 dir = new IndexDirection2(moveHorz, moveVert);

        View.MoveCursor(dir);
    }
    void OnCursorIndexChanged(InventoryView sender)
    {
        if (sender != View)
        {
            return;
        }

        Index2 idx = sender.CursorIndex;
        _inventory.EquippedItemB = _inventory.GetEquippableSecondaryItem(idx.x, idx.y);
    }

    void UpdateView()
    {
        UpdateView_Items();

        if (WorldInfo.Instance.IsOverworld)
        {
            View.DisplayMode = InventoryView.DisplayModeEnum.Overworld;

            UpdateView_TriforcePieces();
        }
        else if (WorldInfo.Instance.IsInDungeon)
        {
            View.DisplayMode = InventoryView.DisplayModeEnum.Dungeon;

            UpdateView_DungeonMapAndItems();
        }
    }

    void UpdateView_Items()
    {
        View.ClearAllItemSlots();

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
        Texture texture = item.GuiSpriteTexture;
        if (texture == null)
        {
            Debug.LogError("Null texture for Item: " + item.name);
            return;
        }

        View.SetTextureForMappedItem(texture, mapping, isEquippedItemB);
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
            View.SetTriforcePieceVisible(dungeonNum, showTriPiece);
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
        View.ShouldDungeonMapRevealVisitedRooms = true;
        View.ShouldDungeonMapRevealUnvisitedRooms = false;
        View.ShouldDungeonMapRevealTriforceRoom = false;
        View.UpdateDungeonMap();
    }


    void PlayInventoryToggleSound()
    {
        SoundFx.Instance.PlayOneShot(SoundFx.Instance.pause);
    }


    // Returns a list of Items that the MenuCursor is currently allowed to be over
    List<Item> GetSelectableItems()
    {
        List<Item> selectableItems = new List<Item>();

        foreach (Item item in _inventory.Items.Values)
        {
            if (!ShouldRenderItem(item)) { continue; }

            InventoryViewItemMapping mapping = item.GetComponent<InventoryViewItemMapping>();
            if (mapping == null || mapping.Type != InventoryViewItemMapping.TypeEnum.Active)
            {
                continue;
            }

            selectableItems.Add(item);
        }

        return selectableItems;
    }
}