using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(InventoryView))]

public class InventoryViewController : MonoBehaviour
{
    InventoryView _view;
    Inventory _inventory;

    bool _cursorCooldownActive = false;
    int _cursorCooldownDuration = 6;
    int _cursorCooldownCounter = 0;


    void Awake()
    {
        _view = GetComponent<InventoryView>();
        _inventory = Inventory.Instance;
    }


    void Update()
    {
        if (Pause.Instance.IsMenuShowing) { return; }

        if (Pause.Instance.IsInventoryShowing)
        {
            UpdateCursor();
        }
    }

    void UpdateCursor()
    {
        if (_cursorCooldownActive)
        {
            if (++_cursorCooldownCounter >= _cursorCooldownDuration) { _cursorCooldownActive = false; }
        }
        else
        {
            float moveHorz = ZeldaInput.GetAxis(ZeldaInput.Axis.MoveHorizontal);
            float moveVert = ZeldaInput.GetAxis(ZeldaInput.Axis.MoveVertical);
            Vector2 direction = new Vector2(moveHorz, moveVert);
            direction = direction.GetNearestNormalizedAxisDirection();
            if (direction.x != 0 || direction.y != 0)
            {
                MoveCursor(direction);
            }
            else
            {
                if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) { direction.x = -1; }
                if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) { direction.x = 1; }
                if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) { direction.y = 1; }
                if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) { direction.y = -1; }

                if (direction.x != 0 || direction.y != 0)
                {
                    MoveCursor(direction);
                }
            }
        }
    }
    void MoveCursor(Vector2 direction)
    {
        _view.MoveCursor(direction);

        Vector2 cursorIndices = _view.CursorIndices;
        _inventory.EquippedItemB = _inventory.GetEquippableSecondaryItem((int)cursorIndices.x, (int)cursorIndices.y);

        SoundFx sfx = SoundFx.Instance;
        sfx.PlayOneShot(sfx.cursor);
    }
    

    void GUIShowInventory()
    {
        GUIShowItems();

        if (Pause.Instance.IsInventoryShowing)
        {
            GUIShowCursor();
        }

        if (WorldInfo.Instance.IsOverworld)
        {
            GUIShowTriforce();
        }
        else if (WorldInfo.Instance.IsInDungeon)
        {
            GUIShowMap();
            GUIShowMapAndCompassIcons();
        }
    }


    void GUIShowItems()
    {
        Texture tex = null;
        foreach (KeyValuePair<string, Item> entry in _inventory.Items)
        {
            Item item = entry.Value;
            if (!item.AppearsInInventoryGUI) { continue; }
            if (item.count == 0) { continue; }
            if (!item.IsTheHighestUpgradeInInventory()) { continue; }
            if (item.name == "Compass" || item.name == "Map" || item.name == "TriforcePiece") { continue; }
            
            tex = item.GetGuiTexture();
            if (tex == null) { continue; }
            StereoDrawTexture(item.guiInventoryPosition, ref tex, color); 
        }

        GUIShowSelectedItem();
    }

    void GUIShowSelectedItem()
    {
        Color color = Color.white;
        color.a = alpha;

        Item itemB = _inventory.EquippedItemB;
        if (itemB != null)
        {
            Texture tex = itemB.GetGuiTexture();
            if (tex != null)
            {
                Vector2 pos = _showcaseItemCenter;
                pos.x -= tex.width * 0.5f;
                pos.y -= tex.height * 0.5f;
                StereoDrawTexture(pos, ref tex, color);
            }
        }
    }

    void GUIShowCursor()
    {
        const int CursorXMin = 136;
        const int CursorYMin = 54;
        const int CursorXInc = 24;
        const int CursorYInc = 16;

        Color color = Color.white;
        color.a = alpha;

        float x = CursorXMin + _cursorIndex_X * CursorXInc;
        float y = CursorYMin + _cursorIndex_Y * CursorYInc;

        Vector2 pos = new Vector2(x, y);
        pos.x -= cursorImage.width * 0.5f;
        pos.y -= cursorImage.height * 0.5f;
        StereoDrawTexture(pos, ref cursorImage, color);
    }


    #region Triforce, Map, Compass


    void GUIShowTriforce()
    {
        for (int i = 0; i < 8; i++)
		{
            Texture image;
            int dungeonNum = i + 1;
            if (!_inventory.HasTriforcePieceForDungeon(dungeonNum)) { continue; }

            if (dungeonNum == 1 || dungeonNum == 3 || dungeonNum == 6 || dungeonNum == 8)
            {
                
                image = triforceLarge_1_image;
            }
            else if (dungeonNum == 5 || dungeonNum == 7)
            {
                image = triforceLarge_2_image;
            }
            else
            {
                image = triforceLarge_3_image;
            }

            Vector2 pos = _triforcePositions[i];
            StereoDrawTexture(pos, ref image, color, false);
		}
        
    }


    const int MapX = 256, MapY = 190;

    const int DungeonSectorWidth = 16, DungeonSectorHeight = 16;
    const int DungeonHallWidth = 4, DungeonHallHeight = 2;
    const int DungeonBossWidth = 6, DungeonBossHeight = 6;
    const int DungeonLinkWidth = 6, DungeonLinkHeight = 6;

    Color _dungeonMapBossColor = Color.red;
    Color _dungeonMapLinkColor = Color.green;

    Texture _dungeonMapBossTexture, _dungeonMapLinkTexture;

    void CreateDungeonMapTextures()
    {
        _dungeonMapLinkTexture = GfxHelper.CreateColoredTexture(_dungeonMapLinkColor);
        _dungeonMapBossTexture = GfxHelper.CreateColoredTexture(_dungeonMapBossColor);
    }

    void GUIShowMap()
    {
        int dungeonNum = WorldInfo.Instance.DungeonNum;
        bool hasMap = _inventory.HasMapForDungeon(dungeonNum);
        bool hasCompass = _inventory.HasCompassForDungeon(dungeonNum);

        int indentX, indentY;
        Rect rect;
        foreach (var room in DungeonFactory.Instance.Rooms)
        {
            Vector2 sector = room.GetGridIndices();
            sector.y = DungeonFactory.MaxDungeonLengthInRooms - sector.y - 1;
            int sectorX = (int)(MapX + DungeonSectorWidth * sector.x);
            int sectorY = (int)(MapY + DungeonSectorHeight * sector.y);

            // Sector
            
            if (!room.HideOnMap && (hasMap || room.PlayerHasVisited))
            {
                int hw = DungeonHallWidth;
                int hh = DungeonHallHeight;

                // Room
                rect = new Rect(sectorX + hh, sectorY + hh, DungeonSectorWidth - 2*hh, DungeonSectorHeight - 2*hh);
                StereoDrawTexture(rect, ref _blackTexture, color, false);

                // West Hall
                if(room.CanPassThrough(DungeonRoomInfo.WallDirection.West))
                {
                    rect = new Rect(sectorX, sectorY + 0.5f * (DungeonSectorHeight - hw), hh, hw);
                    StereoDrawTexture(rect, ref _blackTexture, color, false);
                }

                // East Hall
                if (room.CanPassThrough(DungeonRoomInfo.WallDirection.East))
                {
                    rect = new Rect(sectorX + DungeonSectorWidth - hh, sectorY + 0.5f * (DungeonSectorHeight - hw), hh, hw);
                    StereoDrawTexture(rect, ref _blackTexture, color, false);
                }

                // North Hall
                if (room.CanPassThrough(DungeonRoomInfo.WallDirection.North))
                {
                    rect = new Rect(sectorX + 0.5f * (DungeonSectorWidth - hw), sectorY, hw, hh);
                    StereoDrawTexture(rect, ref _blackTexture, color, false);
                }

                // South Hall
                if (room.CanPassThrough(DungeonRoomInfo.WallDirection.South))
                {
                    rect = new Rect(sectorX + 0.5f * (DungeonSectorWidth - hw), sectorY + DungeonSectorHeight - hh, hw, hh);
                    StereoDrawTexture(rect, ref _blackTexture, color, false);
                }

            }

            // Boss
            if (hasCompass && room.ContainsTriforce)
            {
                indentX = (int)(0.5f * (DungeonSectorWidth - DungeonBossWidth));
                indentY = (int)(0.5f * (DungeonSectorHeight - DungeonBossHeight));
                rect = new Rect(sectorX + indentX, sectorY + indentY, DungeonBossWidth, DungeonBossHeight);
                StereoDrawTexture(rect, ref _dungeonMapBossTexture, color, false);
            }

            // Link
            Vector3 playerPos = CommonObjects.PlayerController_G.transform.position;
            if (room == DungeonRoom.GetRoomForPosition(playerPos))
            {
                indentX = (int)(0.5f * (DungeonSectorWidth - DungeonLinkWidth));
                indentY = (int)(0.5f * (DungeonSectorHeight - DungeonLinkHeight));
                rect = new Rect(sectorX + indentX, sectorY + indentY, DungeonLinkWidth, DungeonLinkHeight);
                StereoDrawTexture(rect, ref _dungeonMapLinkTexture, color, false);
            }
        }
    }

    void GUIShowMapAndCompassIcons()
    {
        int dungeonNum = WorldInfo.Instance.DungeonNum;
        bool hasMap = _inventory.HasMapForDungeon(dungeonNum);
        bool hasCompass = _inventory.HasCompassForDungeon(dungeonNum);

        Color color = Color.white;
        color.a = alpha;

        if (hasMap)
        {
            Item mapItem = _inventory.GetItem("Map");
            Texture tex = mapItem.GetGuiTexture();
            StereoDrawTexture(mapItem.guiInventoryPosition, ref tex, color); 
        }

        if (hasCompass)
        {
            Item compassItem = _inventory.GetItem("Compass");
            Texture tex = compassItem.GetGuiTexture();
            StereoDrawTexture(compassItem.guiInventoryPosition, ref tex, color);
        }
    }

    #endregion
}
