using UnityEngine;
using Immersio.Utility;

public class WorldInfo : Singleton<WorldInfo>
{
    public const int NumDungeons = 9;


    public int quest = 1;
    public Vector2 lostWoodsSector = new Vector2(1, 6);


    public bool IsEmptyScene { get { return Application.loadedLevelName == "_Empty"; } }
    public bool IsCommonScene { get { return Application.loadedLevelName == "Common"; } }
    public bool IsTitleScene { get { return Application.loadedLevelName == Locations.TitleSceneName; } }
    public bool IsSpecial { get { return Application.loadedLevelName == Locations.SpecialSceneName; } }
    public bool IsOverworld { get { return Application.loadedLevelName == "Overworld"; } }
    public bool IsInDungeon { get { return DungeonNum != -1; } }
    public int DungeonNum { 
        get {
            if (IsOverworld || IsSpecial || IsTitleScene || IsCommonScene || IsEmptyScene) { return -1; }
            else
            {
                if (Application.loadedLevelName.Contains("Info")) { return -1; }
                return int.Parse(Application.loadedLevelName.ToCharArray()[8].ToString());
            }
        }
    }

    public string GetSceneNameForDungeon(int dungeonNum)
    {
        return "Dungeon " + dungeonNum + " Q" + quest;
    }
    public string GetSceneNameForDungeonInfo(int dungeonNum)
    {
        return "Dungeon " + dungeonNum + " Q" + quest + " Info";
    }
    public string GetSceneNameForOverworld()
    {
        return "Overworld";
    }
    public string GetSceneNameForOverworldInfo()
    {
        return "Overworld Info";
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

}