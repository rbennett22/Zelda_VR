using UnityEngine;
using System.Collections.Generic;


public class DungeonInfo : MonoBehaviour
{
    public int dungeonNum;


    #region Save/Load

    public class Serializable
    {
        public string[] roomNames;
        public DungeonRoomInfo.Serializable[] roomInfo;
    }

    public Serializable GetInfo()
    {
        Serializable s = new Serializable();

        int numRooms = transform.GetChildCount();
        s.roomNames = new string[numRooms];
        s.roomInfo = new DungeonRoomInfo.Serializable[numRooms];
        int i = 0;
        foreach (Transform child in transform)
        {
            DungeonRoomInfo drInfo = child.GetComponent<DungeonRoomInfo>();
            s.roomNames[i] = drInfo.name;
            s.roomInfo[i] = drInfo.GetSerializable();
            i++;
        }

        return s;
    }

    public void InitWithInfo(Serializable s)
    {
        int numItems = s.roomNames.Length;
        for (int i = 0; i < numItems; i++)
        {
            Transform room = transform.FindChild(s.roomNames[i]);
            if (room == null) { continue; }
            DungeonRoomInfo drInfo = room.GetComponent<DungeonRoomInfo>();
            drInfo.InitWithSerializable(s.roomInfo[i]);
        }
    }

    #endregion

}