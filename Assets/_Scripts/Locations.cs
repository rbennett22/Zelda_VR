using UnityEngine;
using System.Collections;
using Immersio.Utility;


public class Locations : Singleton<Locations>
{
    public const string EmptySceneName = "_Empty";
    public const string TitleSceneName = "TitleScreen";
    public const string SpecialSceneName = "Special";


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


    string _reloadingScene = null;


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
    }

    public void LoadTitleScreen()
    {
        spawnLocation = titleScreen;
        LoadScene(TitleSceneName);
    }

    public void LoadInitialScene()
    {
        WorldInfo w = WorldInfo.Instance;
        string intitialScene;

        if (startInSpecial)
        {
            intitialScene = SpecialSceneName;
            spawnLocation = special;
        }
        else if (startInDungeon == -1)
        {
            intitialScene = w.GetSceneNameForOverworld();
            if (spawnLocation == null)
            {
                spawnLocation = overworldStart;
            }
        }
        else
        {
            intitialScene = w.GetSceneNameForDungeon(startInDungeon);
            if (spawnLocation == null)
            {
                spawnLocation = GetDungeonEntranceStairsLocation(startInDungeon);
            }
        }

        LoadScene(intitialScene, true, true);
    }


    void LoadInfoScenes()
    {
        LoadScene(WorldInfo.Instance.GetSceneNameForOverworldInfo());
		
		for (int i = 0; i < WorldInfo.NumDungeons; i++)
        {
            LoadScene(WorldInfo.Instance.GetSceneNameForDungeonInfo(i + 1));
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
        _reloadingScene = Application.loadedLevelName;
        LoadScene(EmptySceneName);
    }


    public void WarpToOverworldDungeonEntrance(int dungeonNum, bool useShutters = true, bool onlyUseShutterOpen = false)
    {
        spawnLocation = GetOverworldDungeonEntranceLocation(dungeonNum);
        if (WorldInfo.Instance.IsOverworld)
        {
            WarpPlayerToLocation(spawnLocation, null, useShutters);
        }
        else
        {
            string sceneName = WorldInfo.Instance.GetSceneNameForOverworld();
            //LoadScene(sceneName, true);
            LoadScene(sceneName, useShutters, onlyUseShutterOpen);
        }
    }

    public void WarpToOverworldDungeonEntrance(bool useShutters = true, bool onlyUseShutterOpen = false)
    {
        int dungeonNum = WorldInfo.Instance.DungeonNum;
        if (dungeonNum == -1) { return; }

        WarpToOverworldDungeonEntrance(dungeonNum, useShutters, onlyUseShutterOpen);
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
            string sceneName = WorldInfo.Instance.GetSceneNameForDungeon(dungeonNum);
            LoadScene(sceneName, true);
        }
    }

    public void WarpToDungeonEntranceRoom(GameObject notifyOnFinish = null, bool useShutters = false)
    {
        int dungeonNum = WorldInfo.Instance.DungeonNum;
        if (dungeonNum == -1) { return; }

        WarpPlayerToLocation(GetDungeonEntranceRoomLocation(dungeonNum), notifyOnFinish, useShutters);
    }


    GameObject _warpDelegate;
    void WarpPlayerToLocation(Transform location, GameObject notifyOnFinish = null, bool useShutters = false)
    {
        _warpDelegate = notifyOnFinish;

        if (useShutters)
        {
            spawnLocation = location;
            _sceneToLoad = null;
            OverlayGui.Instance.PlayShutterCloseSequence(gameObject);
        }
        else
        {
            SetPlayerPosition(location);
        }
    }

    string _sceneToLoad;
    void LoadScene(string name, bool useShutters = false, bool onlyUseShutterOpen = false)
    {
        if (useShutters)
        {
            Pause.Instance.IsAllowed = false;
            CommonObjects.Player_C.IsParalyzed = true;

            _sceneToLoad = name;
            if (onlyUseShutterOpen)
            {
                StartCoroutine("ShuttersFinishedClosing");
            }
            else
            {
                OverlayGui.Instance.PlayShutterCloseSequence(gameObject);
            }
        }
        else
        {
            Application.LoadLevel(name);
        }
    }

    IEnumerator ShuttersFinishedClosing()
    {
        //print("ShuttersFinishedClosing: " + _sceneToLoad);
        if (_sceneToLoad != null)
        {
            Application.LoadLevel(_sceneToLoad);
            yield return new WaitForSeconds(0.1f);
            CommonObjects.Player_C.IsParalyzed = false;
        }
        else
        {
            SetPlayerPosition(spawnLocation);
            CommonObjects.Player_C.IsParalyzed = false;
        }

        OverlayGui.Instance.PlayShutterOpenSequence(gameObject);
    }
    void ShuttersFinishedOpening()
    {
        if (!WorldInfo.Instance.IsTitleScene)
        {
            Pause.Instance.IsAllowed = true;
        }
    }

    void SetPlayerPosition(Transform t, bool setRotation = true)
    {
        CommonObjects.PlayerController_G.transform.position = t.position;

        // Rotation
        if (setRotation)
        {
            CommonObjects.Player_C.ForceNewForwardDirection(t.forward);
        }

        if (_warpDelegate != null)
        {
            _warpDelegate.SendMessage("FinishedWarpingPlayer", SendMessageOptions.DontRequireReceiver);
            _warpDelegate = null;
        }
    }

}