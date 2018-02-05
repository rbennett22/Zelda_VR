using Immersio.Utility;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameplayHUDViewController : Singleton<GameplayHUDViewController>
{
    #region View

    [SerializeField]
    GameObject _viewPrefab;

    GameplayHUDView _view;
    public GameplayHUDView View { get { return _view ?? (_view = InstantiateView(_viewPrefab)); } }
    GameplayHUDView InstantiateView(GameObject prefab)
    {
        Transform parent = CommonObjects.ActiveCanvas.GameplayHUDViewContainer;
        GameObject g = ZeldaViewController.InstantiateView(prefab, parent);
        GameplayHUDView v = g.GetComponent<GameplayHUDView>();
        return v;
    }

    #endregion View


    Inventory _inventory;


    override protected void Awake()
    {
        base.Awake();

        _inventory = Inventory.Instance;

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
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

        View.gameObject.SetActive(true);
    }
    public void HideView()
    {
        IsViewShowing = false;

        View.gameObject.SetActive(false);
    }


    void Update()
    {
        UpdateView();   // TODO: Only update views when data changes (instead of every frame)
    }

    void UpdateView()
    {
        UpdateView_EquippedItemSlots();
        UpdateView_Hearts();
        UpdateView_ItemCounts();

        if (WorldInfo.Instance.IsOverworld)
        {
            View.DisplayMode = GameplayHUDView.DisplayModeEnum.Overworld;
        }
        else if (WorldInfo.Instance.IsInDungeon)
        {
            View.DisplayMode = GameplayHUDView.DisplayModeEnum.Dungeon;

            UpdateView_DungeonMap();

            View.UpdateLevelNumText(WorldInfo.Instance.DungeonNum);
        }

        UpdateViewPosition();
    }

    void UpdateView_EquippedItemSlots()
    {
        Item itemA = _inventory.EquippedItemA;
        Texture texture = (itemA == null) ? null : itemA.GuiSpriteTexture;
        View.UpdateTextureForEquippedItemSlotA(texture);

        Item ItemB = _inventory.EquippedItemB;
        texture = (ItemB == null) ? null : ItemB.GuiSpriteTexture;
        View.UpdateTextureForEquippedItemSlotB(texture);
    }

    void UpdateView_Hearts()
    {
        Item heartContainer = Inventory.Instance.GetItem("HeartContainer");

        View.UpdateHeartContainerCount(heartContainer.count);
        View.UpdateHeartContainersFillState(CommonObjects.Player_C.HealthInHalfHearts);
    }

    void UpdateView_ItemCounts()
    {
        Inventory inv = Inventory.Instance;

        // Rupees
        int numRupees = inv.RupeeCount;
        View.UpdateRupeesCountText(numRupees);

        // Keys
        if (inv.HasItem("MagicKey"))
        {
            View.UpdateKeysCountText_SetToInfinite();
        }
        else
        {
            int numKeys = inv.GetItem("Key").count;
            View.UpdateKeysCountText(numKeys);
        }

        // Bombs
        int bombs = inv.GetItem("Bomb").count;
        View.UpdateBombsCountText(bombs);
    }


    void InitOverworldMap(int sectorsWide, int sectorsHigh)
    {
        View.InitOverworldMap(sectorsWide, sectorsHigh);

        Player player = CommonObjects.Player_C;
        if (WorldInfo.Instance.IsOverworld)
        {
            Index2 playerOccupiedSector = player.GetOccupiedOverworldSector();
            if (playerOccupiedSector != null)
            {
                View.UpdateOverworldMap(playerOccupiedSector);
            }
        }

        player.OccupiedSectorChanged += PlayerOccupiedSectorChanged;
    }
    void PlayerOccupiedSectorChanged(Index2 prevSector, Index2 newSector)
    {
        View.UpdateOverworldMap(newSector);
    }

    void InitDungeonMap(int sectorsWide, int sectorsHigh)
    {
        View.InitDungeonMap(sectorsWide, sectorsHigh);
    }
    void UpdateView_DungeonMap()
    {
        int dungeonNum = WorldInfo.Instance.DungeonNum;
        bool hasMap = _inventory.HasMapForDungeon(dungeonNum);
        bool hasCompass = _inventory.HasCompassForDungeon(dungeonNum);

        View.ShouldDungeonMapRevealVisitedRooms = hasMap;
        View.ShouldDungeonMapRevealUnvisitedRooms = hasMap;
        View.ShouldDungeonMapRevealTriforceRoom = hasCompass;
        View.UpdateDungeonMap();
    }


    void UpdateViewPosition()
    {
        Transform t = View.transform;

        if (PauseManager.Instance.IsPaused_Inventory)
        {
            Transform pausedT = CommonObjects.ActiveCanvas.GameplayHUDPausedTransform;
            t.position = pausedT.position;
            t.rotation = pausedT.rotation;
        }
        else
        {
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;

            //ApplyDynamicTilting();
        }
    }


    public void ForceRupeeCountTextToAmount(int amount)
    {
        View.ForceRupeeCountTextToAmount(amount);
    }

    /*
    const float Y_BASE_OFFSET = 400;
    public int vertShiftSpeed = 600;

    void ApplyDynamicTilting()
    {
        // As player tilts head upwards, the gameplay HUD moves downwards

        Vector3 camForward = CommonObjects.PlayerController_C.LineOfSight;
        float dot = Vector3.Dot(camForward, Vector3.up);
        int y = Mathf.RoundToInt(Y_BASE_OFFSET - dot * vertShiftSpeed);
        t.SetLocalY(y);
    }*/
}