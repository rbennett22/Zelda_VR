using Immersio.Utility;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WorldInfo : Singleton<WorldInfo>
{
    public const int NUM_DUNGEONS = 9;
    public const int NUM_WARPABLE_DUNGEONS = 8;

    public static Vector3 OVERWORLD_OFFSET = new Vector3(-0.5f, -0.5f, -0.5f);
    public static Vector3 DUNGEON_OFFSET = new Vector3(0, 0, 0);


    [SerializeField]
    Index2 _lostWoodsSector = new Index2(1, 1);

    public Index2 LostWoodsSector { get { return _lostWoodsSector; } }

    public bool IsTitleScreen { get { return ZeldaSceneManager.Instance.IsCurrentUniqueSceneTitleScreen; } }
    public bool IsSpecial { get { return ZeldaSceneManager.Instance.IsCurrentUniqueSceneSpecial; } }
    public bool IsOverworld { get { return ZeldaSceneManager.Instance.IsCurrentUniqueSceneOverworld; } }
    public bool IsInDungeon { get { return ZeldaSceneManager.Instance.IsCurrentUniqueSceneDungeon; } }

    public int DungeonNum { get { return ZeldaSceneManager.Instance.DungeonNum; } }


    public static IWorld TryGetWorld()
    {
        GameObject g = GameObject.FindGameObjectWithTag(ZeldaTags.WORLD);
        IWorld world = (g == null) ? null : g.GetComponent<IWorld>();
        return world;
    }


    public GameObject GetDungeon(int dungeonNum)
    {
        string dungeonInfoName = "Dungeon Info " + dungeonNum;
        return GameObject.Find(dungeonInfoName);
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