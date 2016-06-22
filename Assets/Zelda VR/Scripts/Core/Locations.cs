using Immersio.Utility;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Uniblocks;

public class Locations : Singleton<Locations>
{
    public const string EMPTY_SCENE_NAME = "_Empty";
    public const string COMMON_SCENE_NAME = "Common";
    public const string TITLE_SCREEN_SCENE_NAME = "TitleScreen";
    public const string SPECIAL_SCENE_NAME = "Special";
    public const string OVERWORLD_SCENE_NAME = "Overworld";


    public bool skipTitleScreen = false;
    public bool startInSpecial;
    public int startInDungeon = -1;
    public Transform spawnLocation = null;

    public Transform titleScreen;
    public Transform special;
    public Transform overworldStart;
    public Transform[] overworldDungeonEntrance;
    public Transform[] dungeonEntranceStairs;
    public Transform[] dungeonEntranceRoom;

    [SerializeField]
    Transform _overworldLocationsContainer;

    [SerializeField]
    Transform _dungeonLocationsContainer;


    string _reloadingScene = null;

    Action _warpCompleteCallback;


    public Transform GetOverworldDungeonEntranceLocation(int dungeonNum)
    {
        return overworldDungeonEntrance[dungeonNum - 1];
    }
    public Transform GetDungeonEntranceStairsLocation(int dungeonNum)
    {
        return dungeonEntranceStairs[dungeonNum - 1];
    }
    public Transform GetDungeonEntranceRoomLocation(int dungeonNum)
    {
        return dungeonEntranceRoom[dungeonNum - 1];
    }


    override protected void Awake()
    {
        base.Awake();

        _overworldLocationsContainer.position = WorldInfo.OVERWORLD_OFFSET;
        _dungeonLocationsContainer.position = WorldInfo.DUNGEON_OFFSET;
    }

    void Start()
    {
        //print(" Start");

        LoadInfoScenes();

        if (skipTitleScreen)
        {
            LoadInitialScene();
        }
        else
        {
            LoadTitleScreen();
        }
    }

    void OnLevelWasLoaded(int level)
    {
        //print(" OnLevelWasLoaded: " + level);

        if (_reloadingScene != null)
        {
            string scene = _reloadingScene;
            _reloadingScene = null;
            LoadScene(scene);
            return;
        }

        if (spawnLocation != null)
        {
            WarpPlayerToLocation(spawnLocation);
            spawnLocation = null;
        }

        // TODO: do this elsewhere
        UnityStandardAssets.ImageEffects.GlobalFog fog = FindObjectOfType<UnityStandardAssets.ImageEffects.GlobalFog>();
        fog.enabled = WorldInfo.Instance.IsOverworld;
    }

    public void LoadTitleScreen()
    {
        spawnLocation = titleScreen;
        LoadScene(TITLE_SCREEN_SCENE_NAME);
    }

    public void LoadInitialScene()
    {
        string intitialScene;

        if (startInSpecial)
        {
            intitialScene = SPECIAL_SCENE_NAME;
            spawnLocation = special;
        }
        else if (startInDungeon == -1)
        {
            intitialScene = WorldInfo.GetSceneNameForOverworld();
            if (spawnLocation == null)
            {
                spawnLocation = overworldStart;
            }
        }
        else
        {
            intitialScene = WorldInfo.GetSceneNameForDungeon(startInDungeon);
            if (spawnLocation == null)
            {
                spawnLocation = GetDungeonEntranceStairsLocation(startInDungeon);
            }
        }

        LoadScene(intitialScene, true, true);
    }


    void LoadInfoScenes()
    {
        LoadScene(WorldInfo.GetSceneNameForOverworldInfo());

        for (int i = 0; i < WorldInfo.NUM_DUNGEONS; i++)
        {
            LoadScene(WorldInfo.GetSceneNameForDungeonInfo(i + 1));
        }
    }


    public void RespawnPlayer()
    {
        if (WorldInfo.Instance.IsOverworld)
        {
            spawnLocation = overworldStart;
        }
        else if (WorldInfo.Instance.IsInDungeon)
        {
            int dungeonNum = WorldInfo.Instance.DungeonNum;
            spawnLocation = GetDungeonEntranceRoomLocation(dungeonNum);
        }

        ReloadCurrentScene();
    }

    void ReloadCurrentScene()
    {
        _reloadingScene = SceneManager.GetActiveScene().name;
        LoadScene(EMPTY_SCENE_NAME);
    }


    public void WarpToOverworldDungeonEntrance(bool useShutters = true, bool closeShuttersInstantly = false)
    {
        int dungeonNum = WorldInfo.Instance.DungeonNum;
        if (dungeonNum == -1) { return; }

        WarpToOverworldDungeonEntrance(dungeonNum, useShutters, closeShuttersInstantly);
    }
    public void WarpToOverworldDungeonEntrance(int dungeonNum, bool useShutters = true, bool closeShuttersInstantly = false)
    {
        spawnLocation = GetOverworldDungeonEntranceLocation(dungeonNum);
        if (WorldInfo.Instance.IsOverworld)
        {
            WarpPlayerToLocation(spawnLocation, null, useShutters);
        }
        else
        {
            LoadScene(WorldInfo.GetSceneNameForOverworld(), useShutters, closeShuttersInstantly);
        }
    }

    public void WarpToDungeonEntranceStairs(int dungeonNum)
    {
        spawnLocation = GetDungeonEntranceStairsLocation(dungeonNum);
        if (WorldInfo.Instance.DungeonNum == dungeonNum)
        {
            WarpPlayerToLocation(spawnLocation);
        }
        else
        {
            LoadScene(WorldInfo.GetSceneNameForDungeon(dungeonNum), true);
        }
    }

    public void WarpToDungeonEntranceRoom(Action onCompleteCallback = null, bool useShutters = true)
    {
        int dungeonNum = WorldInfo.Instance.DungeonNum;
        if (dungeonNum == -1) { return; }

        WarpPlayerToLocation(GetDungeonEntranceRoomLocation(dungeonNum), onCompleteCallback, useShutters);
    }


    Transform _warpToLocation;
    void WarpPlayerToLocation(Transform location, Action onCompleteCallback = null, bool useShutters = false, bool closeShuttersInstantly = false)
    {
        _warpCompleteCallback = onCompleteCallback;
        _warpToLocation = location;

        if (useShutters)
        {
            PlayShutterSequence(DoWarpPlayerToLocation, closeShuttersInstantly);
        }
        else
        {
            DoWarpPlayerToLocation();
        }
    }
    void DoWarpPlayerToLocation()
    {
        SetPlayerPosition(_warpToLocation);
        _warpToLocation = null;

        if (_warpCompleteCallback != null)
        {
            _warpCompleteCallback();
            _warpCompleteCallback = null;
        }
    }
    void SetPlayerPosition(Transform t, bool setRotation = true)
    {
        Player player = CommonObjects.Player_C;
        player.Position = t.position;

        if (setRotation)
        {
            player.ForceNewForwardDirection(t.forward);
        }

        if (WorldInfo.Instance.IsOverworld)
        {
            OverworldTerrainEngine.Instance.RefreshActiveStatus();
        }
    }

    string _sceneToLoad;
    void LoadScene(string scene, bool useShutters = false, bool closeShuttersInstantly = false)
    {
        _sceneToLoad = scene;

        if (useShutters)
        {
            PlayShutterSequence(DoLoadScene, closeShuttersInstantly);
        }
        else
        {
            DoLoadScene();
        }
    }
    void DoLoadScene()
    {
        SceneManager.LoadScene(_sceneToLoad);
        _sceneToLoad = null;
    }

    void PlayShutterSequence(Action onCloseCompleteCallback, bool closeInstantly = false)
    {
        const float INTERMISSION_DURATION = 0.2f;

        LimitControls();

        OverlayShuttersViewController svc = OverlayShuttersViewController.Instance;
        svc._isReadyToOpen_Predicate = IsReadyToOpen;
        svc.PlayCloseAndOpenSequence(onCloseCompleteCallback, RestoreControls, INTERMISSION_DURATION, closeInstantly);
    }

    bool IsReadyToOpen(OverlayShuttersViewController sender)
    {
        if (!WorldInfo.Instance.IsOverworld)
        {
            return true;
        }

        OverworldChunkManager cm = OverworldTerrainEngine.ChunkManagerInstance as OverworldChunkManager;
        if (cm == null)
        {
            return true;
        }
        return cm.AreAllVoxelsDone;     // TODO: use correct param here (should signify when all chunks have loaded)
    }


    bool _controlsHaveBeenLimited;
    bool _storedGravityEnabledState;
    // TODO: shouldn't be public
    public void LimitControls()
    {
        if(_controlsHaveBeenLimited)
        {
            return;
        }
        _controlsHaveBeenLimited = true;

        PauseManager pm = PauseManager.Instance;
        pm.IsPauseAllowed_Inventory = false;
        pm.IsPauseAllowed_Options = false;

        CommonObjects.Player_C.IsParalyzed = true;

        ZeldaPlayerController pc = CommonObjects.PlayerController_C;
        _storedGravityEnabledState = pc.gravityEnabled;
        pc.gravityEnabled = false;
    }
    public void RestoreControls()
    {
        if (!_controlsHaveBeenLimited)
        {
            return;
        }
        _controlsHaveBeenLimited = false;

        CommonObjects.Player_C.IsParalyzed = false;
        CommonObjects.PlayerController_C.gravityEnabled = _storedGravityEnabledState;

        if (WorldInfo.Instance.IsPausingAllowedInCurrentScene())
        {
            PauseManager pm = PauseManager.Instance;
            pm.IsPauseAllowed_Inventory = true;
            pm.IsPauseAllowed_Options = true;
        }
    }
}