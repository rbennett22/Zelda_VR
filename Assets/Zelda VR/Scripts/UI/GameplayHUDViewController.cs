using Immersio.Utility;
using UnityEngine;


public class GameplayHUDViewController : Singleton<GameplayHUDViewController>
{
    [SerializeField]
    GameplayHUDView _view;

    Inventory _inventory;


    override protected void Awake()
    {
        base.Awake();

        _inventory = Inventory.Instance;
    }

    void OnLevelWasLoaded(int level)
    {
        if (WorldInfo.Instance.ShouldShowGameplayHUDInCurrentScene())
        {
            ShowView();
        }
        else
        {
            HideView();
        }
    }

    void Start()
    {
        ZeldaVRSettings s = ZeldaVRSettings.Instance;
        InitOverworldMap(s.overworldWidthInSectors, s.overworldHeightInSectors);
        InitDungeonMap(s.dungeonWidthInSectors, s.dungeonHeightInSectors);
    }


    public bool IsViewShowing { get; private set; }
    public void ShowView()
    {
        IsViewShowing = true;

        _view.gameObject.SetActive(true);
    }
    public void HideView()
    {
        IsViewShowing = false;

        _view.gameObject.SetActive(false);
    }


    void Update()
    {
        UpdateView();
    }


    void UpdateView()
    {
        // TODO: Only update views when data changes (instead of every frame)

        UpdateView_EquippedItemSlots();
        UpdateView_Hearts();
        UpdateView_ItemCounts();

        if (WorldInfo.Instance.IsOverworld)
        {
            _view.DisplayMode = GameplayHUDView.DisplayModeEnum.Overworld;
        }
        else if (WorldInfo.Instance.IsInDungeon)
        {
            _view.DisplayMode = GameplayHUDView.DisplayModeEnum.Dungeon;

            UpdateView_DungeonMap();

            _view.UpdateLevelNumText(WorldInfo.Instance.DungeonNum);
        }

        UpdateViewPosition();
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


    void InitOverworldMap(int sectorsWide, int sectorsHigh)
    {
        _view.InitOverworldMap(sectorsWide, sectorsHigh);

        Player player = CommonObjects.Player_C;
        if (WorldInfo.Instance.IsOverworld)
        {
            Index2 playerOccupiedSector = player.GetOccupiedOverworldSector();
            if (playerOccupiedSector != null)
            {
                _view.UpdateOverworldMap(playerOccupiedSector);
            }
        }

        player.OccupiedSectorChanged += PlayerOccupiedSectorChanged;
    }
    void PlayerOccupiedSectorChanged(Index2 prevSector, Index2 newSector)
    {
        _view.UpdateOverworldMap(newSector);
    }

    void InitDungeonMap(int sectorsWide, int sectorsHigh)
    {
        _view.InitDungeonMap(sectorsWide, sectorsHigh);
    }
    void UpdateView_DungeonMap()
    {
        int dungeonNum = WorldInfo.Instance.DungeonNum;
        bool hasMap = _inventory.HasMapForDungeon(dungeonNum);
        bool hasCompass = _inventory.HasCompassForDungeon(dungeonNum);

        _view.ShouldDungeonMapRevealVisitedRooms = hasMap;
        _view.ShouldDungeonMapRevealUnvisitedRooms = hasMap;
        _view.ShouldDungeonMapRevealTriforceRoom = hasCompass;
        _view.UpdateDungeonMap();
    }


    const float Y_BASE_OFFSET = 400;

    [SerializeField]
    Transform _pausedTransform;

    public int vertShiftSpeed = 1200;

    void UpdateViewPosition()
    {
        if (PauseManager.Instance.IsPaused_Inventory)
        {
            _view.transform.position = _pausedTransform.position;
            _view.transform.rotation = _pausedTransform.rotation;
        }
        else
        {
            // As player tilts head upwards, the gameplayHUD moves downwards

            Vector3 camForward = CommonObjects.PlayerController_C.LineOfSight;
            float dot = Vector3.Dot(camForward, Vector3.up);
            int y = Mathf.RoundToInt(Y_BASE_OFFSET - dot * vertShiftSpeed);
            _view.transform.SetLocalY(y);
        }
    }
}