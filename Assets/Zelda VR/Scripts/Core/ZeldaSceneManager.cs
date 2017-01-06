using UnityEngine;
using Immersio.Utility;

public class ZeldaSceneManager : Singleton<ZeldaSceneManager>
{
    const int QUEST = 1; // TODO

    public const string EMPTY_SCENE_NAME = "_Empty";
    public const string COMMON_SCENE_NAME = "Common";
    public const string ZELDA_PLAYER_SCENE_NAME = "ZeldaPlayer";
    public const string TITLE_SCREEN_SCENE_NAME = "TitleScreen";
    public const string SPECIAL_SCENE_NAME = "Special";
    public const string OVERWORLD_SCENE_NAME = "Overworld";
    public const string OVERWORLD_INFO_SCENE_NAME = "Overworld Info";


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
        return OVERWORLD_SCENE_NAME;      // TODO: Quest 2 will use different OW scene...
    }


    public string UniqueScene { get { return SceneManagerWrapper.Instance.UniqueScene; } }
    public bool IsCurrentUniqueSceneOverworld { get { return UniqueScene == OVERWORLD_SCENE_NAME; } }
    public bool IsCurrentUniqueSceneTitleScreen { get { return UniqueScene == TITLE_SCREEN_SCENE_NAME; } }
    public bool IsCurrentUniqueSceneSpecial { get { return UniqueScene == SPECIAL_SCENE_NAME; } }
    public bool IsCurrentUniqueSceneDungeon { get { return (UniqueScene == null) ? false : UniqueScene.Contains("Dungeon"); } }
    public int DungeonNum
    {
        get
        {
            if (!IsCurrentUniqueSceneDungeon)
            {
                return -1;
            }
            return int.Parse(UniqueScene.ToCharArray()[8].ToString());
        }
    }

    public void LoadCommonScene()
    {
        SceneManagerWrapper.Instance.LoadCommonScene(COMMON_SCENE_NAME);
    }
    public void LoadZeldaPlayerScene()
    {
        SceneManagerWrapper.Instance.LoadCommonScene(ZELDA_PLAYER_SCENE_NAME);
    }
    public void LoadInfoScenes()
    {
        SceneManagerWrapper.Instance.LoadCommonScene(OVERWORLD_INFO_SCENE_NAME);

        for (int i = 0; i < WorldInfo.NUM_DUNGEONS; i++)
        {
            SceneManagerWrapper.Instance.LoadCommonScene(GetSceneNameForDungeonInfo(i + 1));
        }
    }


    public void LoadTitleScene()
    {
        SceneManagerWrapper.Instance.LoadUniqueScene(TITLE_SCREEN_SCENE_NAME);
    }
    public void LoadSpecialScene()
    {
        SceneManagerWrapper.Instance.LoadUniqueScene(SPECIAL_SCENE_NAME);
    }
    public void LoadOverworldScene()
    {
        SceneManagerWrapper.Instance.LoadUniqueScene(OVERWORLD_SCENE_NAME);
    }


    public void LoadUniqueScene(string name)
    {
        SceneManagerWrapper.Instance.LoadUniqueScene(name);
    }
    public void ReloadCurrentUniqueScene()
    {
        SceneManagerWrapper.Instance.ReloadCurrentUniqueScene();
    }
}
