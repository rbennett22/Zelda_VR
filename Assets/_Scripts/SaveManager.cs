﻿using UnityEngine;
using System.IO;
using Immersio.Utility;


public class SaveManager : Singleton<SaveManager>
{
    public const string NEW_GAME_DATA_FOLDER_NAME = "NewGame_Data";

    const string NEW_GAME_FILE_NAME = "NewGame";
    const string NEW_GAME_ENTRY_DATA_FILE_NAME = "NewGameEntryData";
    const string GAME_FILE_NAME_BASE = "SavedGame_";
    const string ENTRY_DATA_FILE_NAME_BASE = "SavedEntryData_";


    static string NewGameFilePath { get { return Application.dataPath + "/" + NEW_GAME_DATA_FOLDER_NAME + "/" + NEW_GAME_FILE_NAME + ".txt"; } }
    static string NewGameEntryDataFilePath { get { return Application.dataPath + "/" + NEW_GAME_DATA_FOLDER_NAME + "/" + NEW_GAME_ENTRY_DATA_FILE_NAME + ".txt"; } }

    static string EntryDataFilePathForID(int id)
    {
        return Application.persistentDataPath + "/" + ENTRY_DATA_FILE_NAME_BASE + id + ".txt";
    }
    static string GameFilePathForID(int id)
    {
        return Application.persistentDataPath + "/" + GAME_FILE_NAME_BASE + id + ".txt";
    }
    
    static bool DataExistsForID(int id)
    {
        return File.Exists(GameFilePathForID(id));
    }


    int ActiveSaveEntryID { get; set; }


    string ActiveGameFilePath { get { return GameFilePathForID(ActiveSaveEntryID); } }
    string ActiveEntryDataFilePath { get { return EntryDataFilePathForID(ActiveSaveEntryID); } }


    public void SaveGame()
    {
        print(" SaveGame :: Game:" + ActiveGameFilePath + "\n   EntryData: " + ActiveEntryDataFilePath);

        ZeldaSerializer.Instance.SaveGame(ActiveGameFilePath);
        ZeldaSerializer.Instance.SaveEntryData(ActiveEntryDataFilePath);  
    }

    public void LoadGame(int id)
    {
        string filePath = DataExistsForID(id) ? GameFilePathForID(id) : NewGameFilePath;

        print("LoadGame:: id: " + id + ", filePath: " + filePath);

        ActiveSaveEntryID = id;
        ZeldaSerializer.Instance.LoadGame(filePath);

        Locations.Instance.LoadInitialScene();
    }

    public ZeldaSerializer.EntryData LoadEntryData(int id)
    {
        /*if (!DataExistsForID(id))
        {
            //print(" No data exists for id: " + id + ", path: " + EntryDataFilePathForID(id));
            return null;
        }*/

        string filePath = DataExistsForID(id) ? EntryDataFilePathForID(id) : NewGameEntryDataFilePath;
        return ZeldaSerializer.EntryData.LoadFromFile(filePath);
    }

    public bool DeleteGame(int id)
    {
        print("DeleteGame: " + id);

        if (!DataExistsForID(id)) { return false; }

        File.Delete(EntryDataFilePathForID(id));
        File.Delete(GameFilePathForID(id));

        return true;
    }

}