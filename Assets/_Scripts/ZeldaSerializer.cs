using UnityEngine;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;
using Immersio.Utility;


public class ZeldaSerializer : Singleton<ZeldaSerializer>
{

    public class GameData
    {
        public Inventory.InventoryInfo invInfo;
        public OverworldInfo.Serializable owInfo;
        public DungeonInfo.Serializable[] dungInfo;


        public static void SaveToFile(GameData info, string filename)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(GameData));
            TextWriter writer = new StreamWriter(filename);

            serializer.Serialize(writer, info);

            writer.Close();
        }

        public static GameData LoadFromFile(string filename)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(GameData));
            FileStream fs = new FileStream(filename, FileMode.Open);
            
            GameData data = (GameData)serializer.Deserialize(fs);
            fs.Close();

            return data;
        }
    }

    public class EntryData
    {
        public string name = "Link";        // TODO
        public int deathCount;
        public int armorLevel, swordLevel, numHeartContainers, numHalfHearts;
        

        public static void SaveToFile(EntryData info, string filename)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(EntryData));
            TextWriter writer = new StreamWriter(filename);

            serializer.Serialize(writer, info);

            writer.Close();
        }

        public static EntryData LoadFromFile(string filename)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(EntryData));
            FileStream fs = new FileStream(filename, FileMode.Open);

            EntryData data = (EntryData)serializer.Deserialize(fs);
            fs.Close();

            return data;
        }
    }


    public void SaveGame(string filePath)
    {
        GameData data = new GameData();

        data.invInfo = Inventory.Instance.GetInfo();
        data.owInfo = GameObject.FindGameObjectWithTag("OverworldInfo").GetComponent<OverworldInfo>().GetInfo();
        data.dungInfo = new DungeonInfo.Serializable[WorldInfo.NumDungeons];
        for (int i = 0; i < WorldInfo.NumDungeons; i++)
		{
			 data.dungInfo[i] = WorldInfo.Instance.GetDungeon(i+1).GetComponent<DungeonInfo>().GetInfo();
		}

        GameData.SaveToFile(data, filePath);
    }

    public void SaveEntryData(string filePath)
    {
        EntryData data = new EntryData();

        Player player = CommonObjects.Player_C;
        Inventory inv = Inventory.Instance;

        data.name = player.Name;
        data.deathCount = player.DeathCount;
        data.armorLevel = inv.GetArmorLevel();
        data.swordLevel = inv.GetSwordLevel();
        data.numHeartContainers = inv.GetItem("HeartContainer").count;
        data.numHalfHearts = player.HealthInHalfHearts;

        EntryData.SaveToFile(data, filePath);
    }

    public void LoadGame(string filePath)
    {
        GameData data = GameData.LoadFromFile(filePath);

        Inventory.Instance.InitWithInfo(data.invInfo);
        GameObject.FindGameObjectWithTag("OverworldInfo").GetComponent<OverworldInfo>().InitWithInfo(data.owInfo);
        for (int i = 0; i < WorldInfo.NumDungeons; i++)
		{
			 WorldInfo.Instance.GetDungeon(i+1).GetComponent<DungeonInfo>().InitWithInfo(data.dungInfo[i]);
		}
    }

}