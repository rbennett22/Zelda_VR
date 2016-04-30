using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;

class ZeldaEditorMenu
{
    const string MENU_NAME = "ZeldaVR";
    const string SUBMENU_NAME_0 = "Open Scene";
    const string SUBMENU_NAME_1 = "Build";

    const string DEFAULT_APPLICATION_NAME = "ZeldaVR";
    const string SCENES_DIRECTORY = "Assets/Zelda VR/Scenes";

    static string[] SCENES = {
                GetPathForSceneName(Locations.COMMON_SCENE_NAME),
                GetPathForSceneName("Overworld Info"),
                GetPathForSceneName("Dungeon 1 Q1"),
                GetPathForSceneName("Dungeon 2 Q1"),
                GetPathForSceneName("Dungeon 3 Q1"),
                GetPathForSceneName("Dungeon 4 Q1"),
                GetPathForSceneName("Dungeon 5 Q1"),
                GetPathForSceneName("Dungeon 6 Q1"),
                GetPathForSceneName("Dungeon 7 Q1"),
                GetPathForSceneName("Dungeon 8 Q1"),
                GetPathForSceneName("Dungeon 9 Q1"),
                GetPathForSceneName(Locations.EMPTY_SCENE_NAME),
                GetPathForSceneName(Locations.TITLE_SCREEN_SCENE_NAME),
                GetPathForSceneName(Locations.OVERWORLD_SCENE_NAME),
                GetPathForSceneName("Dungeon 1 Q1 Info"),
                GetPathForSceneName("Dungeon 2 Q1 Info"),
                GetPathForSceneName("Dungeon 3 Q1 Info"),
                GetPathForSceneName("Dungeon 4 Q1 Info"),
                GetPathForSceneName("Dungeon 5 Q1 Info"),
                GetPathForSceneName("Dungeon 6 Q1 Info"),
                GetPathForSceneName("Dungeon 7 Q1 Info"),
                GetPathForSceneName("Dungeon 8 Q1 Info"),
                GetPathForSceneName("Dungeon 9 Q1 Info")
    };


    #region Open Scenes in Editor

    const string M0 = MENU_NAME + "/" + SUBMENU_NAME_0 + "/";

    [MenuItem(M0 + "Common %#HOME")]
    static void OpenScene_Common()
    {
        SaveAndOpenScene(GetPathForSceneName(Locations.COMMON_SCENE_NAME));
    }
    [MenuItem(M0 + "Title Screen %#END")]
    static void OpenScene_TitleScreen()
    {
        SaveAndOpenScene(GetPathForSceneName("TitleScreen"));
    }
    [MenuItem(M0 + "Special %#PGDN")]
    static void OpenScene_Special()
    {
        SaveAndOpenScene(GetPathForSceneName(Locations.SPECIAL_SCENE_NAME));
    }

    [MenuItem(M0 + "Overworld %#PGUP")]
    static void OpenScene_Overworld()
    {
        SaveAndOpenScene(GetPathForSceneName(Locations.OVERWORLD_SCENE_NAME));
    }
    [MenuItem(M0 + "Overworld Info %&PGUP")]
    static void OpenScene_OverworldInfo()
    {
        SaveAndOpenScene(GetPathForSceneName("Overworld Info"));
    }

    [MenuItem(M0 + "Dungeon 1 %#F1")]
    static void OpenScene_Dungeon1()
    {
        SaveAndOpenScene(GetPathForSceneName("Dungeon 1 Q1"));
    }
    [MenuItem(M0 + "Dungeon 2 %#F2")]
    static void OpenScene_Dungeon2()
    {
        SaveAndOpenScene(GetPathForSceneName("Dungeon 2 Q1"));
    }
    [MenuItem(M0 + "Dungeon 3 %#F3")]
    static void OpenScene_Dungeon3()
    {
        SaveAndOpenScene(GetPathForSceneName("Dungeon 3 Q1"));
    }
    [MenuItem(M0 + "Dungeon 4 %#F4")]
    static void OpenScene_Dungeon4()
    {
        SaveAndOpenScene(GetPathForSceneName("Dungeon 4 Q1"));
    }
    [MenuItem(M0 + "Dungeon 5 %#F5")]
    static void OpenScene_Dungeon5()
    {
        SaveAndOpenScene(GetPathForSceneName("Dungeon 5 Q1"));
    }
    [MenuItem(M0 + "Dungeon 6 %#F6")]
    static void OpenScene_Dungeon6()
    {
        SaveAndOpenScene(GetPathForSceneName("Dungeon 6 Q1"));
    }
    [MenuItem(M0 + "Dungeon 7 %#F7")]
    static void OpenScene_Dungeon7()
    {
        SaveAndOpenScene(GetPathForSceneName("Dungeon 7 Q1"));
    }
    [MenuItem(M0 + "Dungeon 8 %#F8")]
    static void OpenScene_Dungeon8()
    {
        SaveAndOpenScene(GetPathForSceneName("Dungeon 8 Q1"));
    }
    [MenuItem(M0 + "Dungeon 9 %#F9")]
    static void OpenScene_Dungeon9()
    {
        SaveAndOpenScene(GetPathForSceneName("Dungeon 9 Q1"));
    }

    [MenuItem(M0 + "Dungeon 1 Info %&F1")]
    static void OpenScene_Dungeon1Info()
    {
        SaveAndOpenScene(GetPathForSceneName("Dungeon 1 Q1 Info"));
    }
    [MenuItem(M0 + "Dungeon 2 Info %&F2")]
    static void OpenScene_Dungeon2Info()
    {
        SaveAndOpenScene(GetPathForSceneName("Dungeon 2 Q1 Info"));
    }
    [MenuItem(M0 + "Dungeon 3 Info %&F3")]
    static void OpenScene_Dungeon3Info()
    {
        SaveAndOpenScene(GetPathForSceneName("Dungeon 3 Q1 Info"));
    }
    [MenuItem(M0 + "Dungeon 4 Info %&F4")]
    static void OpenScene_Dungeon4Info()
    {
        SaveAndOpenScene(GetPathForSceneName("Dungeon 4 Q1 Info"));
    }
    [MenuItem(M0 + "Dungeon 5 Info %&F5")]
    static void OpenScene_Dungeon5Info()
    {
        SaveAndOpenScene(GetPathForSceneName("Dungeon 5 Q1 Info"));
    }
    [MenuItem(M0 + "Dungeon 6 Info %&F6")]
    static void OpenScene_Dungeon6Info()
    {
        SaveAndOpenScene(GetPathForSceneName("Dungeon 6 Q1 Info"));
    }
    [MenuItem(M0 + "Dungeon 7 Info %&F7")]
    static void OpenScene_Dungeon7Info()
    {
        SaveAndOpenScene(GetPathForSceneName("Dungeon 7 Q1 Info"));
    }
    [MenuItem(M0 + "Dungeon 8 Info %&F8")]
    static void OpenScene_Dungeon8Info()
    {
        SaveAndOpenScene(GetPathForSceneName("Dungeon 8 Q1 Info"));
    }
    [MenuItem(M0 + "Dungeon 9 Info %&F9")]
    static void OpenScene_Dungeon9Info()
    {
        SaveAndOpenScene(GetPathForSceneName("Dungeon 9 Q1 Info"));
    }

    static void SaveAndOpenScene(string scenePath)
    {
        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
        EditorSceneManager.OpenScene(scenePath);
    }

    #endregion


    #region Build Standalone

    const string M1 = MENU_NAME + "/" + SUBMENU_NAME_1 + "/";

    [MenuItem (M1 + "Entire Game" + " %DOWN")]	
	static void BuildEntireGame()
	{
        PerformBuild(SCENES);
    }

    // Build Overworld only
    [MenuItem(M1 + "Overworld" + " %ALPHA_0")]
    static void BuildOverworld()
    {
        string[] scenes = new string[] {
            GetPathForSceneName(Locations.COMMON_SCENE_NAME),
            GetPathForSceneName("Overworld Info"),
            GetPathForSceneName(Locations.EMPTY_SCENE_NAME),
            GetPathForSceneName(Locations.TITLE_SCREEN_SCENE_NAME),
            GetPathForSceneName(Locations.OVERWORLD_SCENE_NAME)
        };

        PerformBuild(scenes);
    }

    // Build single Dungeons
    [MenuItem(M1 + "Dungeon 1" + " %ALPHA_1")]
    static void BuildDungeon1()
    {
        BuildSingleDungeon(1);
    }
    [MenuItem(M1 + "Dungeon 2" + " %ALPHA_2")]
    static void BuildDungeon2()
    {
        BuildSingleDungeon(2);
    }
    [MenuItem(M1 + "Dungeon 3" + " %ALPHA_3")]
    static void BuildDungeon3()
    {
        BuildSingleDungeon(3);
    }
    [MenuItem(M1 + "Dungeon 4" + " %ALPHA_4")]
    static void BuildDungeon4()
    {
        BuildSingleDungeon(4);
    }
    [MenuItem(M1 + "Dungeon 5" + " %ALPHA_5")]
    static void BuildDungeon5()
    {
        BuildSingleDungeon(5);
    }
    [MenuItem(M1 + "Dungeon 6" + " %ALPHA_6")]
    static void BuildDungeon6()
    {
        BuildSingleDungeon(6);
    }
    [MenuItem(M1 + "Dungeon 7" + " %ALPHA_7")]
    static void BuildDungeon7()
    {
        BuildSingleDungeon(7);
    }
    [MenuItem(M1 + "Dungeon 8" + " %ALPHA_8")]
    static void BuildDungeon8()
    {
        BuildSingleDungeon(8);
    }
    [MenuItem(M1 + "Dungeon 9" + " %ALPHA_9")]
    static void BuildDungeon9()
    {
        BuildSingleDungeon(9);
    }

    static void BuildSingleDungeon(int dungeonNum)
    {
        string[] scenes = new string[] {
            GetPathForSceneName(Locations.COMMON_SCENE_NAME),
            GetPathForSceneName("Overworld Info"),
            GetPathForSceneName("Dungeon " + dungeonNum + " Q1"),
            GetPathForSceneName(Locations.EMPTY_SCENE_NAME),
            GetPathForSceneName(Locations.TITLE_SCREEN_SCENE_NAME),
            GetPathForSceneName(Locations.OVERWORLD_SCENE_NAME),               
            GetPathForSceneName("Dungeon " + dungeonNum + " Q1 Info")
        };

        PerformBuild(scenes);
    }

    static void PerformBuild(string[] scenes, bool autoRun = false)
    {
        // Get filename
        string buildPath = ObtainBuildPath();
        if(string.IsNullOrEmpty(buildPath))
        {
            return;
        }

        // Build player
        string error = BuildPipeline.BuildPlayer(scenes, buildPath, BuildTarget.StandaloneWindows, BuildOptions.None);
        if (!string.IsNullOrEmpty(error))
        {
            Debug.LogError(error);
            return;
        }

        CopyFilesToBuildDirectory(Path.GetDirectoryName(buildPath));

        if (autoRun)
        {
            // Run the game
            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            proc.StartInfo.FileName = buildPath;
            proc.Start();
        }
        else
        {
            // Open the build directory in file explorer
            ShowExplorer(buildPath);
        }
    }

    static string ObtainBuildPath()
    {
        string previousBuildDirectory = EditorPrefs.GetString("PreviousBuildDirectory");
        string previousApplicationName = EditorPrefs.GetString("PreviousApplicationName", DEFAULT_APPLICATION_NAME);

        string buildPath = EditorUtility.SaveFilePanel("Choose Location of Built Game", previousBuildDirectory, previousApplicationName, "exe");
        if (string.IsNullOrEmpty(buildPath))
        {
            return null;
        }

        EditorPrefs.SetString("PreviousBuildDirectory", Path.GetDirectoryName(buildPath));
        EditorPrefs.SetString("PreviousApplicationName", Path.GetFileNameWithoutExtension(buildPath));

        return buildPath;
    }

    #endregion


    static void CopyFilesToBuildDirectory(string buildDirectory)
    {
        // TODO: Avoid copying meta files

        string copyFromPath = Path.Combine("Assets", SaveManager.NEW_GAME_DATA_FOLDER_NAME);
        copyFromPath = copyFromPath.Replace(@"\", @"/");

        string copyToPath = Path.Combine(buildDirectory, SaveManager.NEW_GAME_DATA_FOLDER_NAME);
        copyToPath = copyToPath.Replace(@"\", @"/");

        FileUtil.CopyFileOrDirectory(copyFromPath, copyToPath);
    }

    static void ShowExplorer(string itemPath)
    {
        itemPath = itemPath.Replace(@"/", @"\");   // explorer doesn't like front slashes
        System.Diagnostics.Process.Start("explorer.exe", "/select," + itemPath);
    }

    static string GetPathForSceneName(string sceneName)
    {
        return Path.Combine(SCENES_DIRECTORY, sceneName + ".unity");
    }
}