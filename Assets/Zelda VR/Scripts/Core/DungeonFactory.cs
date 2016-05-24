using Immersio.Utility;
using System.Collections.Generic;
using UnityEngine;

public class DungeonFactory : Singleton<DungeonFactory>
{
    const float LightPixelValue = 229 / 255.0f;
    const float MidPixelValue = 117 / 255.0f;
    const float DarkPixelValue = 60 / 255.0f;
    const float DarkerPixelValue = 40 / 255.0f;

    const float EastWestMaterialTiling = 0.5875f;
    const float EastWestMaterialOffset = 0.205f;
    public const int MaxDungeonWidthInRooms = 8, MaxDungeonLengthInRooms = 8;


    public GameObject dungeonRoomPrefab;

    public Material defaultFloor_Gray;
    public Material wall_Gray;
    public Material wallOpen_Gray;
    public Material wallSealed_Gray;
    public Material wallLocked_Gray;
    public Material wallBombed_Gray;
    public Material wallTop_Gray;

    public Material defaultFloor;
    public Material wall;
    public Material wallOpen;
    public Material wallSealed;
    public Material wallLocked;
    public Material wallBombed;
    public Material wallTop;

    public Color torchColor;
    public float torchRange = 10;
    public bool centralLighting;

    public float shortBlockHeight = 1.0f;
    public Transform blocksContainer;
    public Transform holeMarkersContainer;
    public Transform subDungeonContainer;

    public Color lightPixelColor, midPixelColor, darkPixelColor;


    Material wall_EW;
    Material wallOpen_EW;
    Material wallSealed_EW;
    Material wallLocked_EW;
    Material wallBombed_EW;
    Material wallTop_EW;

    bool _coloredWallMaterialsHaveBeenInitialized = false;
    bool _eastWestMaterialsInitialized = false;

    List<DungeonRoom> _dungeonRoomsList;
    DungeonRoom[,] _dungeonRoomsGrid;

    public List<DungeonRoom> Rooms { get { return _dungeonRoomsList; } }


    void Start()
    {
        //if (holeMarkersContainer != null) { holeMarkersContainer.gameObject.SetActive(false); }

        CreateDungeonRooms();

        ProcessBlocks();
        AssignEnemySpawnPointsToRooms();
        CreateHolesInFloor();

        foreach (var dr in _dungeonRoomsList)
        {
            AssignAdjoiningRooms(dr);

            if (dr.IsNpcRoom)
            {
                dr.InstantiateNpc();
            }

            InitLighting(dr);
        }
    }

    void CreateDungeonRooms()
    {
        if (_dungeonRoomsList != null) { return; }

        if (!_coloredWallMaterialsHaveBeenInitialized) { CreateColorizedMaterials(); }
        if (!_eastWestMaterialsInitialized) { CreateEastWestVersionOfMaterials(); }

        _dungeonRoomsList = new List<DungeonRoom>();

        int dungeonNum = WorldInfo.Instance.DungeonNum;
        string dungeonInfoName = "Dungeon Info " + dungeonNum;
        Transform dungeonInfo = GameObject.Find(dungeonInfoName).transform;

        // Create Rooms
        foreach (Transform child in dungeonInfo)
        {
            DungeonRoomInfo info = child.GetComponent<DungeonRoomInfo>();
            if (info == null) { continue; }

            CreateRoom(info);
        }

        // Assign SubDungeonSpawnPoints to DungeonRooms
        if (subDungeonContainer != null)
        {
            foreach (Transform child in subDungeonContainer)
            {
                SubDungeonSpawnPoint sdsp = child.GetComponent<SubDungeonSpawnPoint>();
                if (sdsp == null) { continue; }

                DungeonRoom dr = DungeonRoom.GetRoomForPosition(sdsp.marker.transform.position);
                dr.Info.subDungeonSpawnPoint = sdsp;
                sdsp.ParentDungeonRoom = dr;
            }
        }
    }

    void CreateColorizedMaterials()
    {
        if (_coloredWallMaterialsHaveBeenInitialized) { return; }

        ColorizeMaterial(defaultFloor_Gray, defaultFloor);
        ColorizeMaterial(wall_Gray, wall);
        ColorizeMaterial(wallOpen_Gray, wallOpen);
        ColorizeMaterial(wallSealed_Gray, wallSealed);
        ColorizeMaterial(wallLocked_Gray, wallLocked);
        ColorizeMaterial(wallBombed_Gray, wallBombed);
        ColorizeMaterial(wallTop_Gray, wallTop);

        _coloredWallMaterialsHaveBeenInitialized = true;
    }

    void ColorizeMaterial(Material srcMaterial, Material destMaterial)
    {
        Texture2D srcTexture = srcMaterial.mainTexture as Texture2D;

        Texture2D destTexture = new Texture2D(srcTexture.width, srcTexture.height, srcTexture.format, true);
        destTexture.filterMode = srcTexture.filterMode;

        float epsilon = 0.01f;

        int y = 0;
        while (y < destTexture.height)
        {
            int x = 0;
            while (x < destTexture.width)
            {
                Color srcColor = srcTexture.GetPixel(x, y);
                Color destColor;
                if (Mathf.Abs(srcColor.r - LightPixelValue) < epsilon) { destColor = lightPixelColor; }
                else if (Mathf.Abs(srcColor.r - MidPixelValue) < epsilon) { destColor = midPixelColor; }
                else if (Mathf.Abs(srcColor.r - DarkPixelValue) < epsilon) { destColor = darkPixelColor; }
                else if (Mathf.Abs(srcColor.r - DarkerPixelValue) < epsilon) { destColor = darkPixelColor; }
                else { destColor = srcColor; }

                destColor.a = srcColor.a;

                destTexture.SetPixel(x, y, destColor);
                ++x;
            }
            ++y;
        }
        destTexture.Apply();

        destMaterial.mainTexture = destTexture;
    }

    void CreateEastWestVersionOfMaterials()
    {
        Vector2 scale = new Vector2(EastWestMaterialTiling, 1);
        Vector2 offset = new Vector2(EastWestMaterialOffset, 0);

        wall_EW = new Material(wall);
        wall_EW.SetTextureScale("_MainTex", scale);
        wall_EW.SetTextureOffset("_MainTex", offset);

        wallOpen_EW = new Material(wallOpen);
        wallOpen_EW.SetTextureScale("_MainTex", scale);
        wallOpen_EW.SetTextureOffset("_MainTex", offset);

        wallSealed_EW = new Material(wallSealed);
        wallSealed_EW.SetTextureScale("_MainTex", scale);
        wallSealed_EW.SetTextureOffset("_MainTex", offset);

        wallLocked_EW = new Material(wallLocked);
        wallLocked_EW.SetTextureScale("_MainTex", scale);
        wallLocked_EW.SetTextureOffset("_MainTex", offset);

        wallBombed_EW = new Material(wallBombed);
        wallBombed_EW.SetTextureScale("_MainTex", scale);
        wallBombed_EW.SetTextureOffset("_MainTex", offset);

        wallTop_EW = new Material(wallTop);
        wallTop_EW.SetTextureScale("_MainTex", scale);
        wallTop_EW.SetTextureOffset("_MainTex", offset);

        _eastWestMaterialsInitialized = true;
    }


    public void CreateRoom(DungeonRoomInfo info)
    {
        GameObject g = Instantiate(dungeonRoomPrefab);
        g.name = info.name;
        g.transform.SetParent(transform.parent);
        g.transform.position = info.transform.position;

        DungeonRoom dr = g.GetComponent<DungeonRoom>();
        dr.Info = info;

        AssignWallMaterials(dr);
        RemoveUnusedObjects(dr);

        _dungeonRoomsList.Add(dr);
    }

    public void AssignWallMaterials(DungeonRoom dr)
    {
        DungeonRoomInfo info = dr.Info;

        // Set wall materials according to walltype
        dr.wall_N.GetComponent<Renderer>().material = GetWallMaterial(DungeonRoomInfo.WallDirection.North, info.northWallType);
        dr.wall_E.GetComponent<Renderer>().material = GetWallMaterial(DungeonRoomInfo.WallDirection.East, info.eastWallType);
        dr.wall_S.GetComponent<Renderer>().material = GetWallMaterial(DungeonRoomInfo.WallDirection.South, info.southWallType);
        dr.wall_W.GetComponent<Renderer>().material = GetWallMaterial(DungeonRoomInfo.WallDirection.West, info.westWallType);

        dr.wall_N2.GetComponent<Renderer>().material = GetWallMaterial(DungeonRoomInfo.WallDirection.North, DungeonRoomInfo.WallType.Top);
        dr.wall_E2.GetComponent<Renderer>().material = GetWallMaterial(DungeonRoomInfo.WallDirection.East, DungeonRoomInfo.WallType.Top);
        dr.wall_S2.GetComponent<Renderer>().material = GetWallMaterial(DungeonRoomInfo.WallDirection.South, DungeonRoomInfo.WallType.Top);
        dr.wall_W2.GetComponent<Renderer>().material = GetWallMaterial(DungeonRoomInfo.WallDirection.West, DungeonRoomInfo.WallType.Top);

        if (info.floorMaterial == null)
        {
            info.floorMaterial = new Material(defaultFloor);
        }

        dr.floor.GetComponent<Renderer>().material = info.floorMaterial;
        dr.ceiling.GetComponent<Renderer>().material = defaultFloor;
    }

    void RemoveUnusedObjects(DungeonRoom dr)
    {
        DungeonRoomInfo info = dr.Info;

        // DoorTriggers and Halls
        if (info.northWallType == DungeonRoomInfo.WallType.Closed)
        {
            if (!info.IsBombable(DungeonRoomInfo.WallDirection.North))
            {
                Destroy(dr.hall_N);
                Destroy(dr.doorTrigger_N);
                Destroy(dr.entranceBlock_N);
            }
        }
        if (info.eastWallType == DungeonRoomInfo.WallType.Closed)
        {
            if (!info.IsBombable(DungeonRoomInfo.WallDirection.East))
            {
                Destroy(dr.hall_E);
                Destroy(dr.doorTrigger_E);
                Destroy(dr.entranceBlock_E);
            }
        }
        if (info.southWallType == DungeonRoomInfo.WallType.Closed)
        {
            if (!info.IsBombable(DungeonRoomInfo.WallDirection.South))
            {
                Destroy(dr.hall_S);
                Destroy(dr.doorTrigger_S);
                Destroy(dr.entranceBlock_S);
            }
        }
        if (info.westWallType == DungeonRoomInfo.WallType.Closed)
        {
            if (!info.IsBombable(DungeonRoomInfo.WallDirection.West))
            {
                Destroy(dr.hall_W);
                Destroy(dr.doorTrigger_W);
                Destroy(dr.entranceBlock_W);
            }
        }

        // InnerBombable Walls
        if (!info.IsBombable(DungeonRoomInfo.WallDirection.North))
        {
            Destroy(dr.wall_bombedInner_N);
        }
        if (!info.IsBombable(DungeonRoomInfo.WallDirection.East))
        {
            Destroy(dr.wall_bombedInner_E);
        }
        if (!info.IsBombable(DungeonRoomInfo.WallDirection.South))
        {
            Destroy(dr.wall_bombedInner_S);
        }
        if (!info.IsBombable(DungeonRoomInfo.WallDirection.West))
        {
            Destroy(dr.wall_bombedInner_W);
        }

        // NpcContainer
        if (dr.IsNpcRoom)
        {
            if (dr.ContainsBombUpgrade && info.BombUpgradeHasBeenPurchased)
            {
                Destroy(dr.rupeeTrigger);
            }
        }
        else
        {
            Destroy(dr.npcContainer.gameObject);
        }
    }


    void ProcessBlocks()
    {
        foreach (Transform block in blocksContainer)
        {
            // Let the DungeonRoom know about the blocks it contains
            DungeonRoom dr = DungeonRoom.GetRoomForPosition(block.position);
            if (dr != null)
            {
                dr.AddBlock(block.gameObject);
            }

            // Scale blocks accordingly, and position so they touch ground
            if (block.gameObject.layer != LayerMask.NameToLayer("InvisibleBlocks"))
            {
                block.localScale = new Vector3(block.localScale.x, shortBlockHeight, block.localScale.z);
                block.SetLocalY(shortBlockHeight * 0.5f);
                block.GetComponent<Renderer>().material.mainTextureScale = new Vector2(1, shortBlockHeight);
            }
        }
    }

    void AssignEnemySpawnPointsToRooms()
    {
        foreach (Transform child in transform)
        {
            EnemySpawnPoint sp = child.GetComponent<EnemySpawnPoint>();
            if (sp != null)
            {
                DungeonRoom dr = DungeonRoom.GetRoomForPosition(sp.transform.position);
                if (dr == null)
                {
                    //print(" sp -> " + sp.name + ": " + sp.transform.position.x + ", " + sp.transform.position.z);
                    continue;
                }
                dr.AddEnemySpawnPoint(sp);
            }
        }
    }

    void CreateHolesInFloor()
    {
        if (holeMarkersContainer == null) { return; }

        foreach (Transform hole in holeMarkersContainer)
        {
            HoleMarker holeMarker = hole.GetComponent<HoleMarker>();
            if (holeMarker == null) { continue; }

            DungeonRoom dr = DungeonRoom.GetRoomForPosition(hole.position);
            if (dr == null)
            {
                //print(" hole -> " + hole.name + ": " + hole.transform.position.x + ", " + hole.transform.position.z);
                continue;
            }

            Vector2 tile = dr.WorldPointToTile(hole.position);
            Rect floorTextureArea = dr.TileToFloorTextureArea(tile);

            //print("tile: " + tile.ToString());
            //print("floorTextureArea: " + floorTextureArea.ToString());

            // Create a new texture
            Texture2D origTexture;
            if (holeMarker.appearsOnPushBlock && dr.Info.PushBlockChangeFloorMaterial != null)
            {
                origTexture = dr.Info.PushBlockChangeFloorMaterial.mainTexture as Texture2D;
            }
            else
            {
                origTexture = dr.floor.GetComponent<Renderer>().material.mainTexture as Texture2D;
            }

            Texture2D newTexture = new Texture2D(origTexture.width, origTexture.height, origTexture.format, true);
            newTexture.filterMode = origTexture.filterMode;

            // Copy the pixels and cut the hole
            Color[] pixels = origTexture.GetPixels();
            int y = (int)floorTextureArea.yMin;
            while (y < (int)floorTextureArea.yMax)
            {
                int x = (int)floorTextureArea.xMin;
                while (x < (int)floorTextureArea.xMax)
                {
                    int i = newTexture.width * y + x;
                    pixels[i].a = 0;
                    ++x;
                }
                ++y;
            }
            newTexture.SetPixels(pixels);
            newTexture.Apply();

            // Assign the new texture
            if (holeMarker.appearsOnPushBlock)
            {
                dr.Info.PushBlockChangeFloorMaterial = new Material(dr.floor.GetComponent<Renderer>().material);
                dr.Info.PushBlockChangeFloorMaterial.mainTexture = newTexture;
            }
            else
            {
                dr.Info.floorMaterial.mainTexture = newTexture;
                dr.floor.GetComponent<Renderer>().material.mainTexture = newTexture;
            }
        }
    }

    void AssignAdjoiningRooms(DungeonRoom dr)
    {
        Vector3 pos = dr.transform.position;
        int x = (int)(pos.x / 16);
        int y = (int)(pos.z / 11);

        foreach (var room in _dungeonRoomsList)
        {
            Vector3 rPos = room.transform.position;
            int rX = (int)(rPos.x / 16);
            int rY = (int)(rPos.z / 11);

            if (rX == x && rY == y + 1) { dr.NorthRoom = room; }
            else if (rX == x && rY == y - 1) { dr.SouthRoom = room; }
            else if (rX == x + 1 && rY == y) { dr.EastRoom = room; }
            else if (rX == x - 1 && rY == y) { dr.WestRoom = room; }
        }
    }

    void InitLighting(DungeonRoom dr)
    {
        // Torches
        if (centralLighting)
        {
            dr.torchLight_A.transform.parent.SetLocalZ(0);
            dr.torchLight_A.transform.parent.SetLocalY(5);
            dr.torchLight_B.transform.parent.SetLocalY(5);
            dr.torchLight_B.transform.parent.SetLocalZ(0);
        }

        dr.TorchColor = torchColor;
        dr.TorchRange = torchRange;
        dr.ActivateTorchLights(dr.Info.isLit);
    }

    /*void CutHoleInTexture(Texture2D texture, Rect area, bool doApply = true)
    {
        int y = (int)area.yMin;
        while (y < (int)area.yMax)
        {
            int x = (int)area.xMin;
            while (x < (int)area.xMax)
            {
                Color pixel = texture.GetPixel(x, y);
                pixel.a = 0;
                texture.SetPixel(x, y, pixel);
                ++x;
            }
            ++y;
        }

        if (doApply)
        {
            texture.Apply();
        }
    }*/


    public Material GetWallMaterial(DungeonRoomInfo.WallDirection direction, DungeonRoomInfo.WallType wallType)
    {
        bool northSouthWall = DungeonRoomInfo.IsNorthOrSouth(direction);
        Material mat = null;
        switch (wallType)
        {
            case DungeonRoomInfo.WallType.Closed:
                mat = northSouthWall ? wall : wall_EW;
                break;
            case DungeonRoomInfo.WallType.DoorOpen:
                mat = northSouthWall ? wallOpen : wallOpen_EW;
                break;
            case DungeonRoomInfo.WallType.DoorSealed:
                mat = northSouthWall ? wallSealed : wallSealed_EW;
                break;
            case DungeonRoomInfo.WallType.DoorLocked:
                mat = northSouthWall ? wallLocked : wallLocked_EW;
                break;
            case DungeonRoomInfo.WallType.Bombed:
                mat = northSouthWall ? wallBombed : wallBombed_EW;
                break;
            case DungeonRoomInfo.WallType.Top:
                mat = northSouthWall ? wallTop : wallTop_EW;
                break;
            default:
                break;
        }

        return mat;
    }


    void CreateDungeonRoomsGrid()
    {
        if (_dungeonRoomsList == null) { CreateDungeonRooms(); }

        _dungeonRoomsGrid = new DungeonRoom[MaxDungeonLengthInRooms, MaxDungeonWidthInRooms];

        foreach (var room in _dungeonRoomsList)
        {
            Vector2 gridPos = room.GetSectorIndices();
            int x = (int)(gridPos.x + float.Epsilon);
            int y = (int)(gridPos.y + float.Epsilon);
            _dungeonRoomsGrid[y, x] = room;
        }

        //PrintDungeonRoomsGrid();
    }

    public DungeonRoom GetRoomAtGridPosition(int x, int y)
    {
        if (_dungeonRoomsGrid == null) { CreateDungeonRoomsGrid(); }

        if (x < 0 || x >= MaxDungeonWidthInRooms) { return null; }
        if (y < 0 || y >= MaxDungeonLengthInRooms) { return null; }

        return _dungeonRoomsGrid[y, x];
    }


    void PrintDungeonRoomsGrid()
    {
        string output = " ---  DungeonRoomsGrid  ---\n\n";
        for (int y = 0; y < MaxDungeonLengthInRooms; y++)
        {
            for (int x = 0; x < MaxDungeonWidthInRooms; x++)
            {
                DungeonRoom dr = _dungeonRoomsGrid[MaxDungeonLengthInRooms - 1 - y, x];
                output += (dr == null) ? "O" : "X";
            }
            output += "\n";
        }
        output += "\n -------------------------";

        print(output);
    }
}