using UnityEngine;
using UnityEditor;
using System.IO;


class ZeldaBuild
{
    const string BuiltApplicationName = "ZeldaVR";
    //const string BuiltApplicationName_Minimal = "ZeldaVR_minimal";
    const string ScenesDirectory = "Assets/__Scenes/";
    const string Ext = ".unity";

    static string[] _scenes = { 
                        ScenesDirectory + "Common" + Ext, 
                        ScenesDirectory + "Overworld Info" + Ext, 
                        ScenesDirectory + "Dungeon 1 Q1" + Ext,
                        ScenesDirectory + "Dungeon 2 Q1" + Ext,
                        ScenesDirectory + "Dungeon 3 Q1" + Ext,
                        ScenesDirectory + "Dungeon 4 Q1" + Ext,
                        ScenesDirectory + "Dungeon 5 Q1" + Ext,
                        ScenesDirectory + "Dungeon 6 Q1" + Ext,
                        ScenesDirectory + "Dungeon 7 Q1" + Ext,
                        ScenesDirectory + "Dungeon 8 Q1" + Ext,
                        ScenesDirectory + "Dungeon 9 Q1" + Ext,
                        ScenesDirectory + "_Empty" + Ext, 
                        ScenesDirectory + "TitleScreen" + Ext, 
                        ScenesDirectory + "Special" + Ext,
                        ScenesDirectory + "Overworld" + Ext, 
                        ScenesDirectory + "Dungeon 1 Q1 Info" + Ext,
                        ScenesDirectory + "Dungeon 2 Q1 Info" + Ext,
                        ScenesDirectory + "Dungeon 3 Q1 Info" + Ext,
                        ScenesDirectory + "Dungeon 4 Q1 Info" + Ext,
                        ScenesDirectory + "Dungeon 5 Q1 Info" + Ext,
                        ScenesDirectory + "Dungeon 6 Q1 Info" + Ext,
                        ScenesDirectory + "Dungeon 7 Q1 Info" + Ext,
                        ScenesDirectory + "Dungeon 8 Q1 Info" + Ext,
                        ScenesDirectory + "Dungeon 9 Q1 Info" + Ext,
                    };


    static public string BuiltApplicationPath { get { return Directory.GetParent(Application.dataPath).ToString() + @"\" + BuiltApplicationName + ".exe"; } }

    static string NewGameDataPath_Editor { get { return "Assets/NewGame_Data"; } }
    public static string DataFolderName { get { return BuiltApplicationName + "_Data"; } }
    public static string NewGameDataPath_Standalone { get { return DataFolderName + "/NewGame_Data"; } }
    public static string NewGameDataPath { get { return Application.isEditor ? NewGameDataPath_Editor : NewGameDataPath_Standalone; } }

   
    // Build the standalone Windows game and place into main project folder
	[MenuItem ("Zelda/Build/StandaloneWindows")]	
	static void PerformBuildStandaloneWindows ()
	{
        BuildPipeline.BuildPlayer(_scenes, BuiltApplicationPath, BuildTarget.StandaloneWindows, BuildOptions.None);
        CopyFilesToDataDirectory();
	}
	
	// Build the standalone Windows game, place into main project folder, and then run
    [MenuItem("Zelda/Build/StandaloneWindows - Run %DOWN")]	
	static void PerformBuildStandaloneWindowsRun ()
	{
        EditorApplication.SaveScene();
        BuildPipeline.BuildPlayer(_scenes, BuiltApplicationPath, BuildTarget.StandaloneWindows, BuildOptions.AutoRunPlayer);
        CopyFilesToDataDirectory();
    }


    [MenuItem("Zelda/Build/StandaloneWindows - Run %UP+ALPHA_0")]
    static void Dungeon0BuildAndRun()
    {
        SingleDungeonBuildAndRun(0);
    }
    [MenuItem("Zelda/Build/StandaloneWindows - Run %UP+ALPHA_1")]
    static void Dungeon1BuildAndRun()
    {
        SingleDungeonBuildAndRun(1);
    }
    [MenuItem("Zelda/Build/StandaloneWindows - Run %UP+ALPHA_2")]
    static void Dungeon2BuildAndRun()
    {
        SingleDungeonBuildAndRun(2);
    }
    [MenuItem("Zelda/Build/StandaloneWindows - Run %UP+ALPHA_3")]
    static void Dungeon3BuildAndRun()
    {
        SingleDungeonBuildAndRun(3);
    }
    [MenuItem("Zelda/Build/StandaloneWindows - Run %UP+ALPHA_4")]
    static void Dungeon4BuildAndRun()
    {
        SingleDungeonBuildAndRun(4);
    }
    [MenuItem("Zelda/Build/StandaloneWindows - Run %UP+ALPHA_5")]
    static void Dungeon5BuildAndRun()
    {
        SingleDungeonBuildAndRun(5);
    }
    [MenuItem("Zelda/Build/StandaloneWindows - Run %UP+ALPHA_6")]
    static void Dungeon6BuildAndRun()
    {
        SingleDungeonBuildAndRun(6);
    }
    [MenuItem("Zelda/Build/StandaloneWindows - Run %UP+ALPHA_7")]
    static void Dungeon7BuildAndRun()
    {
        SingleDungeonBuildAndRun(7);
    }
    [MenuItem("Zelda/Build/StandaloneWindows - Run %UP+ALPHA_8")]
    static void Dungeon8BuildAndRun()
    {
        SingleDungeonBuildAndRun(8);
    }
    [MenuItem("Zelda/Build/StandaloneWindows - Run %UP+ALPHA_9")]
    static void Dungeon9BuildAndRun()
    {
        SingleDungeonBuildAndRun(9);
    }

    static void SingleDungeonBuildAndRun(int dungeonNum)
    {
        EditorApplication.SaveScene();

        string[] scenes;

        if (dungeonNum == 0)
        {
            scenes = new string[] { 
                        ScenesDirectory + "Common" + Ext, 
                        ScenesDirectory + "Overworld Info" + Ext, 
                        ScenesDirectory + "_Empty" + Ext, 
                        ScenesDirectory + "TitleScreen" + Ext, 
                        ScenesDirectory + "Overworld" + Ext, 
                    };
        }
        else
        {
            scenes = new string[] { 
                        ScenesDirectory + "Common" + Ext, 
                        ScenesDirectory + "Overworld Info" + Ext, 
                        ScenesDirectory + "Dungeon " + dungeonNum + " Q1" + Ext,
                        ScenesDirectory + "_Empty" + Ext, 
                        ScenesDirectory + "TitleScreen" + Ext, 
                        ScenesDirectory + "Overworld" + Ext, 
                        ScenesDirectory + "Dungeon " + dungeonNum + " Q1 Info" + Ext,
                    };
        }

        BuildPipeline.BuildPlayer(scenes, BuiltApplicationPath, BuildTarget.StandaloneWindows, BuildOptions.AutoRunPlayer);

        CopyFilesToDataDirectory();
    }


    static void CopyFilesToDataDirectory()
    {
        FileUtil.CopyFileOrDirectory(NewGameDataPath_Editor, NewGameDataPath_Standalone);   
    }

}