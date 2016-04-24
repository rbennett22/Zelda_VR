using UnityEngine;
using Immersio.Utility;


public class GameplayHUDViewController : Singleton<GameplayHUDViewController>
{
    [SerializeField]
    GameplayHUDView _view;
    Inventory _inventory;


    public int vertShiftSpeed = 600;


    float _yBaseOffset = 300;
    public int PausedYVal           // Where to vertically position HUD when game is Paused
    {
        get { return (int)(Screen.height * 0.7f + _yBaseOffset); }    
    }


    void Awake()
    {
        _inventory = Inventory.Instance;
    }


    void Update()
    {
        UpdateView();
    }


    void UpdateView()
    {
        UpdateView_EquippedItemSlots();
        UpdateView_Hearts();
        UpdateView_ItemCounts();

        if (WorldInfo.Instance.IsOverworld)
        {
            _view.DisplayMode = GameplayHUDView.DisplayModeEnum.Overworld;

            UpdateView_OverworldMap();
        }
        else if (WorldInfo.Instance.IsInDungeon)
        {
            _view.DisplayMode = GameplayHUDView.DisplayModeEnum.Dungeon;

            UpdateView_DungeonMap();

            _view.UpdateLevelNumText(WorldInfo.Instance.DungeonNum);
        }
    }

    void UpdateView_EquippedItemSlots()
    {
        Item itemA = _inventory.EquippedItemA;
        Texture texture = (itemA == null) ? null : itemA.GetGuiTexture();
        _view.UpdateTextureForEquippedItemSlotA(texture);

        Item ItemB = _inventory.EquippedItemB;
        texture = (ItemB == null) ? null : ItemB.GetGuiTexture();
        _view.UpdateTextureForEquippedItemSlotB(texture);
    }

    void UpdateView_Hearts()
    {
        Item heartContainer = Inventory.Instance.GetItem("HeartContainer");

        _view.UpdateHeartContainerCount(heartContainer.count);
        _view.UpdateHeartContainersFillState(CommonObjects.Player_C.HealthInHalfHearts);
    }

    void UpdateView_ItemCounts()
    {
        Inventory inv = Inventory.Instance;

        // Rupees
        int numRupees = inv.GetItem("Rupee").count;
        _view.UpdateRupeesCountText(numRupees);

        // Keys
        if (inv.HasItem("MagicKey"))
        {
            _view.UpdateKeysCountText_SetToInfinite();
        }
        else
        {
            int numKeys = inv.GetItem("Key").count;
            _view.UpdateKeysCountText(numKeys);
        }

        // Bombs
        int bombs = inv.GetItem("Bomb").count;
        _view.UpdateBombsCountText(bombs);
    }


    const int MapX = 16, MapY = 12;
    const int MapWidth = 64, MapHeight = 32;

    Color _owMapBgColor = new Color(0.2f, 0.2f, 0.2f);
    Color _owMapSectorColor = Color.green;

    void UpdateView_OverworldMap()
    {
        /*
        //StereoDrawTexture(new Rect(MapX, MapY, MapWidth, MapHeight), ref _owMapBgTexture, color);

        TileMap tileMap = TileProliferator.Instance.tileMap;

        Vector2 sector = tileMap.GetSectorForPosition(CommonObjects.PlayerController_G.transform.position);
        if (sector != WorldInfo.Instance.lostWoodsSector)
        {
            int w = (int)(MapWidth / tileMap.SectorsWide);
            int h = (int)(MapHeight / tileMap.SectorsLong);
            int x = (int)(MapX + w * sector.x);
            int y = (int)(MapY + h * sector.y);
            //StereoDrawTexture(new Rect(x, y, w, h), ref _owMapSectorTexture, color);
        }*/
    }

    void UpdateView_DungeonMap()
    {
        int dungeonNum = WorldInfo.Instance.DungeonNum;
        bool hasMap = _inventory.HasMapForDungeon(dungeonNum);
        bool hasCompass = _inventory.HasCompassForDungeon(dungeonNum);

        _view.ShouldDungeonMapRevealUnvisitedRooms = hasMap;
        _view.ShouldDungeonMapRevealTriforceRoom = hasCompass;
        _view.UpdateDungeonMap();
    }


    /*Rect CalcHudRenderArea()
    {
        int w = hudImage.width;
        int h = hudImage.height;
        int x = (int)((Screen.width - w) * 0.5f);

        Vector3 camForward = CommonObjects.PlayerController_C.LineOfSight;

        float dot = Vector3.Dot(camForward, Vector3.up);
        int y = Mathf.RoundToInt(_yBaseOffset + dot * vertShiftSpeed);

        if (Pause.Instance.IsInventoryShowing)
        {
            y = PausedYVal;
        }

        //print("w, h: " + w + ", " + h);
        //print("SCREEN: " + Screen.width + ", " + Screen.height);

        return new Rect(x, y, w, h);
    }*/
}