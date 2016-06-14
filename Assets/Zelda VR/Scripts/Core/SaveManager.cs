using Immersio.Utility;
using System.IO;
using UnityEngine;


public class SaveManager : Singleton<SaveManager>
{
    public const string NEW_GAME_DATA_FOLDER_NAME = "NewGame_Data";

    const string NEW_GAME_FILE_NAME = "NewGame";
    const string NEW_GAME_ENTRY_DATA_FILE_NAME = "NewGameEntryData";
    const string SAVED_GAME_FILE_NAME_BASE = "SavedGame_";
    const string SAVED_GAME_ENTRY_DATA_FILE_NAME_BASE = "SavedEntryData_";


    static string NewGameFilePath           { get { return Application.dataPath + "/" + NEW_GAME_DATA_FOLDER_NAME + "/" + NEW_GAME_FILE_NAME + ".txt"; } }
    static string NewGameEntryDataFilePath  { get { return Application.dataPath + "/" + NEW_GAME_DATA_FOLDER_NAME + "/" + NEW_GAME_ENTRY_DATA_FILE_NAME + ".txt"; } }

    static string SavedGameEntryDataFilePathForID(int id)
    {
        return Application.persistentDataPath + "/" + SAVED_GAME_ENTRY_DATA_FILE_NAME_BASE + id + ".txt";
    }
    static string SavedGameFilePathForID(int id)
    {
        return Application.persistentDataPath + "/" + SAVED_GAME_FILE_NAME_BASE + id + ".txt";
    }

    static bool SavedGameDataExistsForID(int id)
    {
        return File.Exists(SavedGameFilePathForID(id));
    }


    int ActiveSaveEntryID { get; set; }


    string ActiveSavedGameFilePath { get { return SavedGameFilePathForID(ActiveSaveEntryID); } }
    string ActiveSavedGameEntryDataFilePath { get { return SavedGameEntryDataFilePathForID(ActiveSaveEntryID); } }


    public void SaveGame()
    {
        print(" SaveGame ::  Game:" + ActiveSavedGameFilePath + "\n   EntryData: " + ActiveSavedGameEntryDataFilePath);

        ZeldaSerializer s = ZeldaSerializer.Instance;
        s.SaveGame(ActiveSavedGameFilePath);
        s.SaveEntryData(ActiveSavedGameEntryDataFilePath);
    }

    public void LoadGame(int id)
    {
        string filePath = SavedGameDataExistsForID(id) ? SavedGameFilePathForID(id) : NewGameFilePath;

        print("LoadGame ::  ID: " + id + ", filePath: " + filePath);

        ActiveSaveEntryID = id;
        ZeldaSerializer.Instance.LoadGame(filePath);

        Locations.Instance.LoadInitialScene();
    }

    public ZeldaSerializer.EntryData LoadEntryData(int id)
    {
        /*if (!SavedGameDataExistsForID(id))
        {
            //print(" No data exists for id: " + id + ", path: " + SavedGameEntryDataFilePathForID(id));
            return null;
        }*/

        string filePath = SavedGameDataExistsForID(id) ? SavedGameEntryDataFilePathForID(id) : NewGameEntryDataFilePath;
        return ZeldaSerializer.EntryData.LoadFromFile(filePath);
    }

    public bool DeleteGame(int id)
    {
        print("DeleteGame: " + id);

        if (!SavedGameDataExistsForID(id)) { return false; }

        File.Delete(SavedGameEntryDataFilePathForID(id));
        File.Delete(SavedGameFilePathForID(id));

        return true;
    }
}