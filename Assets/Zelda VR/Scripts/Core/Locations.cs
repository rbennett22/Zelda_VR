using Immersio.Utility;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Uniblocks;

public class Locations : Singleton<Locations>
{
    public bool skipTitleScreen = false;
    public bool startInSpecial;
    public int startInDungeon = -1;
    public Transform spawnLocation = null;

    // Spawn Locations
    public Transform titleScreen;
    public Transform special;
    public Transform overworldStart;
    public Transform[] overworldDungeonEntrance;
    public Transform[] grottoWarpDestinations;
    public Transform[] dungeonEntranceStairs;
    public Transform[] dungeonEntranceRoom;
    

    [SerializeField]
    Transform _overworldLocationsContainer;
    [SerializeField]
    Transform _dungeonLocationsContainer;


    Action _warpCompleteCallback;


    public Transform GetOverworldDungeonEntranceLocation(int dungeonNum)
    {
        return overworldDungeonEntrance[dungeonNum - 1];
    }
    public Transform GetGrottoWarpDestinationLocation(int warpNum)
    {
        if(warpNum < 0 || warpNum > 3)
        {
            return null;
        }
        return grottoWarpDestinations[warpNum - 1];
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

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void Start()
    {
        ZeldaSceneManager.Instance.LoadInfoScenes();

        if (skipTitleScreen)
        {
            LoadInitialScene();
        }
        else
        {
            GoToTitleScreen();
        }
    }

    void OnSceneLoaded(Scene s, LoadSceneMode mode)
    {
        //print(" OnLevelWasLoaded: " + level);

        if(s.name != ZeldaSceneManager.Instance.UniqueScene)
        {
            return;
        }

        if (spawnLocation != null)
        {
            WarpPlayerToLocation(spawnLocation);
            spawnLocation = null;
        }

        // TODO: do this elsewhere
        //ZeldaFog.Instance.enabled = WorldInfo.Instance.IsOverworld;
        ZeldaFog.Instance.enabled = false;
    }


    public void GoToTitleScreen()
    {
        spawnLocation = titleScreen;
        ZeldaSceneManager.Instance.LoadTitleScene();

        CommonObjects.Player_C.IsParalyzed = true;
        CommonObjects.PlayerController_C.gravityEnabled = false;
    }

    public void LoadInitialScene()
    {
        string intitialScene;

        if (startInSpecial)
        {
            intitialScene = ZeldaSceneManager.SPECIAL_SCENE_NAME;
            spawnLocation = special;
        }
        else if (startInDungeon == -1)
        {
            intitialScene = ZeldaSceneManager.GetSceneNameForOverworld();
            if (spawnLocation == null)
            {
                spawnLocation = overworldStart;
            }
        }
        else
        {
            intitialScene = ZeldaSceneManager.GetSceneNameForDungeon(startInDungeon);
            if (spawnLocation == null)
            {
                spawnLocation = GetDungeonEntranceStairsLocation(startInDungeon);
            }
        }

        LoadScene(intitialScene, true, true);
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

        ZeldaSceneManager.Instance.ReloadCurrentUniqueScene();
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
            LoadScene(ZeldaSceneManager.GetSceneNameForOverworld(), useShutters, closeShuttersInstantly);
        }
    }

    public void WarpToGrottoWarpDestination(int warpNum, bool useShutters = true, bool closeShuttersInstantly = false)
    {
        spawnLocation = GetGrottoWarpDestinationLocation(warpNum);
        if (WorldInfo.Instance.IsOverworld)
        {
            WarpPlayerToLocation(spawnLocation, null, useShutters);
        }
        else
        {
            LoadScene(ZeldaSceneManager.GetSceneNameForOverworld(), useShutters, closeShuttersInstantly);
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
            LoadScene(ZeldaSceneManager.GetSceneNameForDungeon(dungeonNum), true);
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
            WorldInfo.TryGetWorld().DoUpdate(true);
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
        ZeldaSceneManager.Instance.LoadUniqueScene(_sceneToLoad);
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