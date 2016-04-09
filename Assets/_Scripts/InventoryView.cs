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
    GameObject _itemB;
    [SerializeField]
    GameObject _itemP0, _itemP1, _itemP2, _itemP3, _itemP4, _itemP5;
    [SerializeField]
    GameObject _itemA00, _itemA01, _itemA02, _itemA03;
    [SerializeField]
    GameObject _itemA10, _itemA11, _itemA12, _itemA13;

    [SerializeField]
    GameObject _cursor;
    

    int _cursorIndexX, _cursorIndexY;


    void Awake()
    {
        DisplayMode = _displayMode;

        CreateDungeonMapTextures();
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
        }
    }


    #region Triforce, Map, Compass


    void GUIShowTriforce(Color color)
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

    void GUIShowMap(Color color)
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
                rect = new Rect(sectorX + hh, sectorY + hh, DungeonSectorWidth - 2 * hh, DungeonSectorHeight - 2 * hh);
                StereoDrawTexture(rect, ref _blackTexture, color, false);

                // West Hall
                if (room.CanPassThrough(DungeonRoomInfo.WallDirection.West))
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
