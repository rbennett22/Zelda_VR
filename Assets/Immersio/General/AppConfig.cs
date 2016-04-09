using UnityEngine;
using System;
using System.Xml.Serialization;
using System.IO;

public class AppConfig 
{
    const string DefaultConfigFileName = "config.xml";


    public bool hotKeysEnabled = false;
    public bool guiEnabled = false;

    public string serverIP = "127.0.0.1";
    public int serverPort = 7100;

    public string assetBundleName = "";
    public string assetBundleURL_http = "";
    public string assetBundleURL_file = "";


    #region Properties

    public static bool HotKeysEnabled   { get { return Instance.hotKeysEnabled; } }
    public static bool GuiEnabled       { get { return Instance.guiEnabled; } }

    public static string ServerIP       { get { return Instance.serverIP; } }
    public static int ServerPort        { get { return Instance.serverPort; } }

    public static string AssetBundleFullURL_http { 
        get {
            string http = Instance.assetBundleURL_http;
            if (string.IsNullOrEmpty(http)) { return AssetBundleFullURL_file; }
            return http + Instance.assetBundleName + ".unity3d"; 
        } 
    }
    public static string AssetBundleFullURL_file { get { return @"file:///" + Instance.assetBundleURL_file + Instance.assetBundleName + ".unity3d"; } }

    #endregion


    #region Init

    protected static AppConfig _instance;
    public static AppConfig Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = LoadFromFile(ConfigFilePath);
            }
            return _instance;
        }
    }

    public static string ConfigFilePath 
    { 
        get {
            string path = Application.dataPath;
            path = path.Remove(path.LastIndexOf(@"/"));

            string configFileName = GetConfigFileNameFromCommandLineArg();
            if (string.IsNullOrEmpty(configFileName)) 
            {
                configFileName = DefaultConfigFileName;
            }

            path += @"/" + configFileName;
            return path; 
        } 
    }

    static string GetConfigFileNameFromCommandLineArg()
    {
        string fileName = "";
        string[] args = System.Environment.GetCommandLineArgs();

        for (int i = 0; i < args.Length; i++)
        {
            string arg = args[i];
            if (arg == "-config")
            {
                if (i + 1 < args.Length)
                {
                    fileName = args[i + 1];
                    break;
                }
            }
        }
        return fileName;
    }

    #endregion


    #region Save/Load

    public const string TestFilePath = "Assets/config_TEST.xml";

    public static void PerformTestSave()
    {
        AppConfig config = new AppConfig();
        SaveToFile(config);
    }
    public static void PerformTestLoad()
    {
        AppConfig config = AppConfig.LoadFromFile();
        Debug.Log(config);
    }


    public static void SaveToFile(AppConfig config) { SaveToFile(config, TestFilePath); }
    public static void SaveToFile(AppConfig config, string filePath)
    {
        XmlSerializer serializer = new XmlSerializer(typeof(AppConfig));
        TextWriter writer = new StreamWriter(filePath);

        serializer.Serialize(writer, config);

        writer.Close();
    }

    public static AppConfig LoadFromFile() { return LoadFromFile(TestFilePath); }
    public static AppConfig LoadFromFile(string filePath)
    {
        /*if (!Directory.Exists(filePath)) 
        { 
            Debug.LogError("Could not load config file.  No file was found at path: " + filePath);
            return null;
        }*/

        XmlSerializer serializer = new XmlSerializer(typeof(AppConfig));
        FileStream fs = new FileStream(filePath, FileMode.Open);

        return (AppConfig)serializer.Deserialize(fs);
    }

    #endregion


    public override string ToString()
    {
        string s = " -----  AppConfig  -----\n\n";

        s += " HotKeysEnabled: " + hotKeysEnabled + "\n";
        s += " GuiEnabled: " + guiEnabled + "\n";
        s += " ServerIP: " + serverIP + "\n";
        s += " ServerPort: " + serverPort + "\n";
        s += " AssetBundleName: " + assetBundleName + "\n";
        s += " AssetBundleURL_http: " + assetBundleURL_http + "\n";
        s += " AssetBundleURL_file: " + assetBundleURL_file + "\n";

        s += "\n -----------------------";

        return s;
    }

}
