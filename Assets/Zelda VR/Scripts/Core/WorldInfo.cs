using Immersio.Utility;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WorldInfo : Singleton<WorldInfo>
{
    public const int QUEST = 1;
    public const int NUM_DUNGEONS = 9;
    public const int NUM_WARPABLE_DUNGEONS = 8;

    public static Vector3 OVERWORLD_OFFSET = new Vector3(-0.5f, -0.5f, -0.5f);
    public static Vector3 DUNGEON_OFFSET = new Vector3(0, 0, 0);


    [SerializeField]
    Index2 _lostWoodsSector = new Index2(1, 1);

    public Index2 LostWoodsSector { get { return _lostWoodsSector; } }


    public bool IsEmptyScene { get { return SceneManager.GetActiveScene().name == Locations.EMPTY_SCENE_NAME; } }
    public bool IsCommonScene { get { return SceneManager.GetActiveScene().name == Locations.COMMON_SCENE_NAME; } }
    public bool IsTitleScene { get { return SceneManager.GetActiveScene().name == Locations.TITLE_SCREEN_SCENE_NAME; } }
    public bool IsSpecial { get { return SceneManager.GetActiveScene().name == Locations.SPECIAL_SCENE_NAME; } }
    public bool IsOverworld { get { return SceneManager.GetActiveScene().name == Locations.OVERWORLD_SCENE_NAME; } }
    public bool IsInDungeon { get { return DungeonNum != -1; } }
    public int DungeonNum
    {
        get
        {
            if (IsOverworld || IsSpecial || IsTitleScene || IsCommonScene || IsEmptyScene) { return -1; }
            else
            {
                if (SceneManager.GetActiveScene().name.Contains("Info")) { return -1; }
                return int.Parse(SceneManager.GetActiveScene().name.ToCharArray()[8].ToString());
            }
        }
    }


    public static string GetSceneNameForDungeon(int dungeonNum)
    {
        return "Dungeon " + dungeonNum + " Q" + QUEST;
    }
    public static string GetSceneNameForDungeonInfo(int dungeonNum)
    {
        return "Dungeon " + dungeonNum + " Q" + QUEST + " Info";
    }
    public static string GetSceneNameForOverworld()
    {
        return Locations.OVERWORLD_SCENE_NAME;      // TODO: Quest 2 will use different OW scene...
    }
    public static string GetSceneNameForOverworldInfo()
    {
        return "Overworld Info";
    }


    public static IWorld TryGetWorld()
    {
        GameObject g = GameObject.FindGameObjectWithTag("World");
        IWorld world = (g == null) ? null : g.GetComponent<IWorld>();
        return world;
    }


    public GameObject GetDungeon(int dungeonNum)
    {
        string dungeonInfoName = "Dungeon Info " + dungeonNum;
        return GameObject.Find(dungeonInfoName);
    }

    public GameObject GetPrimaryCamera()
    {
        GameObject cam = null;

        if (Camera.main != null)
        {
            cam = Camera.main.gameObject;
        }
        else
        {
            GameObject[] cams = GameObject.FindGameObjectsWithTag("MainCamera");
            if (cams != null && cams.Length > 0)
            {
                cam = cams[0];
            }
        }

        return cam;
    }

    public bool ShouldShowGameplayHUDInCurrentScene()
    {
        return IsOverworld || IsInDungeon || IsSpecial;
    }

    public bool IsPausingAllowedInCurrentScene()
    {
        return IsOverworld || IsInDungeon || IsSpecial;
    }


    public Vector3 WorldOffset
    {
        get
        {
            Vector3 offset = Vector3.zero;
            if (IsOverworld) { offset = OVERWORLD_OFFSET; }
            else if (IsInDungeon) { offset = DUNGEON_OFFSET; }
            return offset;
        }
    }
}