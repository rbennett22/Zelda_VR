using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


public class DungeonRoomInfo : MonoBehaviour 
{

    public enum WallDirection
    {
        North, East, South, West, None
    }
    public static bool IsNorthOrSouth(WallDirection dir)
    {
        return (dir == WallDirection.North || dir == WallDirection.South);
    }

    public static WallDirection GetOppositeDirection(WallDirection dir)
    {
        WallDirection opp;
        switch (dir)
        {
            case WallDirection.North: opp = WallDirection.South; break;
            case WallDirection.East: opp = WallDirection.West; break;
            case WallDirection.South: opp = WallDirection.North; break;
            case WallDirection.West: opp = WallDirection.East; break;
            default: opp = WallDirection.None; break;
        }
        return opp;
    }

    public static Vector3 NormalForWallDirection(WallDirection dir)
    {
        Vector3 vec;
        switch (dir)
        {
            case WallDirection.North: vec = new Vector3(0, 0, -1); break;
            case WallDirection.East: vec = new Vector3(-1, 0, 0); break;
            case WallDirection.South: vec = new Vector3(0, 0, 1); break;
            case WallDirection.West: vec = new Vector3(1, 0, 0); break;
            default: vec = Vector3.zero; break;
        }
        return vec;
    }

    public static Vector3 TangentForWallDirection(WallDirection dir)
    {
        Vector3 vec;
        switch (dir)
        {
            case WallDirection.North: vec = new Vector3(1, 0, 0); break;
            case WallDirection.East: vec = new Vector3(0, 0, -1); break;
            case WallDirection.South: vec = new Vector3(-1, 0, 0); break;
            case WallDirection.West: vec = new Vector3(0, 0, 1); break;
            default: vec = Vector3.zero; break;
        }
        return vec;
    }


    public enum WallType
    {
        Closed,
        DoorOpen,
        DoorSealed,
        DoorLocked,
        Bombed,
        Top
    }
    public WallType northWallType, eastWallType, southWallType, westWallType;


    public WallDirection[] bombableWalls;
    public Material floorMaterial;

    public Collectible ItemOnClear { get; set; }    // Collectible that appears when all enemies in room have been defeated
    public Collectible SpecialItem { get; set; }    // Collectible that sits in room and can only ever be collected once  
      
    public WallDirection[] doorsOpenOnClear;    // List of sealed doors that open when all enemies in room have been defeated
    public WallDirection pushBlockDoorDirection = WallDirection.None;   // A sealed door that opens after pushing a block in the room
    public bool pushBlockSpawnsSubDungeon;
    public Material PushBlockChangeFloorMaterial { get; set; }

    public bool containsBoss;
    public bool containsTriforce;
    public bool containsGoriyaNPC;
    public bool containsBombUpgrade;
    public bool needTriforceToPass;
    
    public bool isLit = true;
    public string npcText;
    public GameObject npcPrefab;
    public bool hideOnMap;

    public SubDungeonSpawnPoint subDungeonSpawnPoint { get; set; }
    public bool EnemiesHaveSpawned { get; set; }

    public bool PlayerHasVisited { get; set; }
    public bool SpecialDropItemHasBeenCollected { get; set; }
    public bool ItemOnClearHasBeenCollected { get; set; }
    public bool SpecialItemHasBeenCollected { get; set; }
    public bool BossHasBeenDefeated { get; set; }
    public bool GoriyaNpcHasBeenFed { get; set; }
    public bool BombUpgradeHasBeenPurchased { get; set; }


    void OnLevelWasLoaded(int level)
    {
        if (!containsBoss || !BossHasBeenDefeated)
        {
            EnemiesHaveSpawned = false;
        }
    }


    public WallType GetWallTypeForDirection(WallDirection direction)
    {
        WallType t = WallType.Closed;
        switch (direction)
        {
            case WallDirection.North: t = northWallType; break;
            case WallDirection.East: t = eastWallType; break;
            case WallDirection.South: t = southWallType; break;
            case WallDirection.West: t = westWallType; break;
            default: break;
        }
        return t;
    }
    public void SetWallTypeForDirection(WallDirection direction, WallType type)
    {
        switch (direction)
        {
            case WallDirection.North: northWallType = type; break;
            case WallDirection.East: eastWallType = type; break;
            case WallDirection.South: southWallType = type; break;
            case WallDirection.West: westWallType = type; break;
            default: break;
        }
    }

    public bool CanPassThrough(WallDirection direction)
    {
        WallType wallType = GetWallTypeForDirection(direction);
        return (wallType == WallType.DoorOpen || wallType == WallType.Bombed);
    }

    public bool IsBombable(WallDirection direction)
    {
        if (bombableWalls == null) { return false; }

        bool isBombable = false;
        foreach (var wallDir in bombableWalls)
	    {
		    if (wallDir == direction) { isBombable = true; break; }
	    }
        return isBombable;
    }

    public WallDirection GetDirectionOfSealedDoor()
    {
        WallDirection dir = WallDirection.North;

        if (northWallType == WallType.DoorSealed) { dir = WallDirection.North; }
        else if (eastWallType == WallType.DoorSealed) { dir = WallDirection.East; }
        else if (southWallType == WallType.DoorSealed) { dir = WallDirection.South; }
        else if (westWallType == WallType.DoorSealed) { dir = WallDirection.West; }
        else { Debug.LogError("No Sealed doors exist in this room: " + name); }

        return dir;
    }


    #region Save/Load

    public class Serializable
    {
        public WallType northWallType, eastWallType, southWallType, westWallType;

        public bool playerHasVisited;
        public bool specialDropItemHasBeenCollected;
        public bool itemOnClearHasBeenCollected;
        public bool specialItemHasBeenCollected;
        public bool bossHasBeenDefeated;
        public bool goriyaNpcHasBeenFed;
        public bool bombUpgradeHasBeenPurchased;
    }

    public Serializable GetSerializable()
    {
        Serializable info = new Serializable();

        info.northWallType = northWallType;
        info.eastWallType = eastWallType;
        info.southWallType = southWallType;
        info.westWallType = westWallType;

        info.playerHasVisited = PlayerHasVisited;
        info.specialDropItemHasBeenCollected = SpecialDropItemHasBeenCollected;
        info.itemOnClearHasBeenCollected = ItemOnClearHasBeenCollected;
        info.specialItemHasBeenCollected = SpecialItemHasBeenCollected;
        info.bossHasBeenDefeated = BossHasBeenDefeated;
        info.goriyaNpcHasBeenFed = GoriyaNpcHasBeenFed;
        info.bombUpgradeHasBeenPurchased = BombUpgradeHasBeenPurchased;

        return info;
    }

    public void InitWithSerializable(Serializable info)
    {
        northWallType = info.northWallType;
        eastWallType = info.eastWallType;
        southWallType = info.southWallType;
        westWallType = info.westWallType;

        PlayerHasVisited = info.playerHasVisited;
        SpecialDropItemHasBeenCollected = info.specialDropItemHasBeenCollected;
        ItemOnClearHasBeenCollected = info.itemOnClearHasBeenCollected;
        SpecialItemHasBeenCollected = info.specialItemHasBeenCollected;
        BossHasBeenDefeated = info.bossHasBeenDefeated;
        GoriyaNpcHasBeenFed = info.goriyaNpcHasBeenFed;
        BombUpgradeHasBeenPurchased = info.bombUpgradeHasBeenPurchased;
    }

    #endregion

}
