using UnityEngine;
using System.Collections.Generic;

public class DungeonRoom : MonoBehaviour 
{
    const bool ALWAYS_SEAL_DOORS_FOR_BOSS = false;       // <-- Design Decision

    const int BombUpgradeCost = 100;
    public const float TilesWide = 12, TilesLong = 7;
    public const float TilesWide_WithHalls = 16, TilesLong_WithHalls = 11;
    

    public GameObject wall_N, wall_E, wall_S, wall_W, floor, ceiling;
	public GameObject wall_N2, wall_E2, wall_S2, wall_W2;
    public GameObject wall_bombedInner_N, wall_bombedInner_E, wall_bombedInner_S, wall_bombedInner_W;
    public GameObject doorTrigger_N, doorTrigger_E, doorTrigger_S, doorTrigger_W;
	public GameObject hall_N, hall_E, hall_S, hall_W;
    public GameObject entranceBlock_N, entranceBlock_E, entranceBlock_S, entranceBlock_W;
    public GameObject torchLights;
    public Light torchLight_A, torchLight_B;
    public Transform npcContainer;
    public GameObject rupeeTrigger;


    public DungeonRoomInfo Info { get; set; }

    public bool IsLit { get { return Info.isLit; } }
    public bool IsNpcRoom { get { return Info.npcPrefab != null; } }
    public bool LightsOffOnExit { get { return (!IsLit && !IsNpcRoom); } }
    public bool ContainsBoss { get { return Info.containsBoss; } }
    public bool ContainsGoriyaNpc { get { return Info.containsGoriyaNPC; } }
    public bool ContainsTriforce { get { return Info.containsTriforce; } }
    public bool ContainsBombUpgrade { get { return Info.containsBombUpgrade; } }
    public bool NeedTriforceToPass { get { return Info.needTriforceToPass; } }
    public bool ContainsSubDungeonSpawnPoint { get { return Info.subDungeonSpawnPoint != null; } }
    public bool HideOnMap { get { return Info.hideOnMap; } }
    public bool PlayerHasVisited { get { return Info.PlayerHasVisited; } set { Info.PlayerHasVisited = value; } }
    public bool EnemiesHaveSpawned { get { return Info.EnemiesHaveSpawned; } set { Info.EnemiesHaveSpawned = value; } }
    public bool SpecialDropItemHasBeenCollected { get { return Info.SpecialDropItemHasBeenCollected; } set { Info.SpecialDropItemHasBeenCollected = value; } }

    public bool WillShowNpcText
    {
        get
        {
            if (!IsNpcRoom) { return false; }
            if (ContainsGoriyaNpc && Info.GoriyaNpcHasBeenFed) { return false; }
            if (ContainsBombUpgrade && Info.BombUpgradeHasBeenPurchased) { return false; }
            if (NeedTriforceToPass && Inventory.Instance.HasAllTriforcePieces()) { return false; }
            return true;
        }
    }
    public bool WillShowNpc
    {
        get
        {
            if (!IsNpcRoom) { return false; }
            if (ContainsGoriyaNpc && Info.GoriyaNpcHasBeenFed) { return false; }
            if (NeedTriforceToPass && Inventory.Instance.HasAllTriforcePieces()) { return false; }
            return true;
        }
    }

    public Collectible ItemOnClear { get { return Info.ItemOnClear; } set { Info.ItemOnClear = value; } }
    public Collectible SpecialItem { get { return Info.SpecialItem; } set { Info.SpecialItem = value; } }

    public List<Enemy> Enemies { get; private set; }
    public List<Enemy> KillableEnemies { get; private set; }

    public Color TorchColor { set { torchLight_A.color = value; torchLight_B.color = value; } }
    public float TorchRange { set { torchLight_A.range = value; torchLight_B.range = value; } }

    public DungeonRoom NorthRoom { get; set; }
    public DungeonRoom EastRoom { get; set; }
    public DungeonRoom SouthRoom { get; set; }
    public DungeonRoom WestRoom { get; set; }

    public Rect Bounds
    {
        get
        {
            Vector3 p = transform.position;
            float w = TilesWide, h = TilesLong;
            return new Rect(p.x - w * 0.5f, p.z - h * 0.5f, w, h);
        }
    }


    List<EnemySpawnPoint> _enemySpawnPoints = new List<EnemySpawnPoint>();
    List<GameObject> _blocks = new List<GameObject>();
    GameObject[] _walls;
    GameObject _npc;
    Pushable _pushBlock;

    bool _hasPushBlockBeenPushed;


    void Awake()
    {
        Enemies = new List<Enemy>();
        KillableEnemies = new List<Enemy>();

        _walls = new GameObject[] { wall_N, wall_E, wall_S, wall_W, floor, ceiling, wall_N2, wall_E2, wall_S2, wall_W2 };
        foreach (var wall in _walls)
        {
            wall.AddComponent<LightsOnOffMaterial>();
        }
    }

    void Start()
    {
        if (LightsOffOnExit)
        {
            ActivateTorchLights(false);
        }

        if (ItemOnClear != null)
        {
            ItemOnClear.gameObject.SetActive(false);
        }

        if (SpecialItem != null && Info.SpecialItemHasBeenCollected)
        {
            SpecialItem.gameObject.SetActive(false);
        }
    }


    public void AddEnemySpawnPoint(EnemySpawnPoint sp) { _enemySpawnPoints.Add(sp); }

    public void AddEnemy(Enemy enemy)
    {
        if (enemy == null) { return; }

        Enemies.Add(enemy);
        enemy.DungeonRoomRef = this;
        if (!enemy.GetComponent<HealthController>().isIndestructible)
        {
            KillableEnemies.Add(enemy);
        }
    }

    public void AddBlock(GameObject block)
    {
        if (block == null) { return; }

        _blocks.Add(block);
        block.AddComponent<LightsOnOffMaterial>();

        Pushable pushable = block.GetComponent<Pushable>();
        if (pushable != null)
        {
            _pushBlock = pushable;
        }
    }


    public void InstantiateNpc()
    {
        if (Info.npcPrefab == null) { return; }

        _npc = Instantiate(Info.npcPrefab) as GameObject;
        _npc.transform.parent = npcContainer;
        _npc.transform.localPosition = Vector3.zero;

        _npc.SetActive(WillShowNpc);

        rupeeTrigger.SetActive(ContainsBombUpgrade && !Info.BombUpgradeHasBeenPurchased);
    }

    void ShowNpcText(bool doShow = true)
    {
        //print(_info.npcText);
        if (doShow)
        {
            MessageBoard.Instance.Display(Info.npcText, npcContainer.position);
        }
        else
        {
            MessageBoard.Instance.Hide();
        }
    }

    public void ActivateTorchLights(bool doActivate = true)
    {
        torchLights.SetActive(doActivate);

        if (!IsNpcRoom)
        {
            foreach (var wall in _walls)
            {
                wall.GetComponent<LightsOnOffMaterial>().TurnLightsOn(doActivate);
            }
            foreach (var block in _blocks)
            {
                block.GetComponent<LightsOnOffMaterial>().TurnLightsOn(doActivate);
            }

            if (doActivate)
            {
                CommonObjects.CurrentDungeonFactory.AssignWallMaterials(this);
            }
        }
    }
    

    void SpawnEnemies()
    {
        foreach (var sp in _enemySpawnPoints)
        {
            GameObject g = sp.SpawnEnemy();
            AddEnemy(g.GetComponent<Enemy>());
        }

        if (_pushBlock != null)
        {
            _pushBlock.PushingEnabled = (KillableEnemies.Count == 0);
        }

        EnemiesHaveSpawned = true;
    }

    void SpawnSubDungeon()
    {
        Info.subDungeonSpawnPoint.gameObject.SetActive(true);
        Info.subDungeonSpawnPoint.SpawnSubDungeon();
    }
    void DestroySubDungeon()
    {
        Info.subDungeonSpawnPoint.DestroySubDungeon();
    }


    public void FeedGoriyaNpc()
    {
        if (Info.GoriyaNpcHasBeenFed) { return; }

        Inventory.Instance.UseItem("Bait");
        Info.GoriyaNpcHasBeenFed = true;

        _npc.SetActive(false);
        ShowNpcText(false);
        UnsealDoors();
    }

    void OnRupeeTrigger(RupeeTrigger rupeeTrigger)
    {
        PurchaseBombUpgrade();
    }
    void PurchaseBombUpgrade()
    {
        if (Info.BombUpgradeHasBeenPurchased) { return; }

        Inventory inv = Inventory.Instance;
        if (!inv.SpendRupees(BombUpgradeCost)) { return; }

        inv.OnItemCollected("BombUpgrade");
        Info.BombUpgradeHasBeenPurchased = true;

        rupeeTrigger.SetActive(false);
        ShowNpcText(false);
    }


    public DungeonRoom GetAdjoiningRoom(DungeonRoomInfo.WallDirection dir)
    {
        DungeonRoom dr;
        switch (dir)
        {
            case DungeonRoomInfo.WallDirection.North: dr = NorthRoom; break;
            case DungeonRoomInfo.WallDirection.East: dr = EastRoom; break;
            case DungeonRoomInfo.WallDirection.South: dr = SouthRoom; break;
            case DungeonRoomInfo.WallDirection.West: dr = WestRoom; break;
            default: dr = null; break;
        }
        return dr;
    }

    public GameObject GetWallForDirection(DungeonRoomInfo.WallDirection direction)
    {
        GameObject wall = null;
        switch (direction)
        {
            case DungeonRoomInfo.WallDirection.North: wall = wall_N; break;
            case DungeonRoomInfo.WallDirection.East: wall = wall_E; break;
            case DungeonRoomInfo.WallDirection.South: wall = wall_S; break;
            case DungeonRoomInfo.WallDirection.West: wall = wall_W; break;
            default: break;
        }
        return wall;
    }
    public DungeonRoomInfo.WallType GetWallTypeForDirection(DungeonRoomInfo.WallDirection direction)
    {
        return Info.GetWallTypeForDirection(direction);
    }
    public void SetWallTypeForDirection(DungeonRoomInfo.WallDirection direction, DungeonRoomInfo.WallType type)
    {
        Info.SetWallTypeForDirection(direction, type);

        // Now update the wall's Material
        GameObject wall = GetWallForDirection(direction);
        wall.GetComponent<Renderer>().material = CommonObjects.CurrentDungeonFactory.GetWallMaterial(direction, type);

        wall.GetComponent<LightsOnOffMaterial>().OnMaterialChanged();

    }
    public DungeonRoomInfo.WallDirection GetWallDirectionForWall(GameObject wall)
    {
        DungeonRoomInfo.WallDirection wallDir = DungeonRoomInfo.WallDirection.None;

        if (wall == wall_N) { wallDir = DungeonRoomInfo.WallDirection.North; }
        else if (wall == wall_E) { wallDir = DungeonRoomInfo.WallDirection.East; }
        else if (wall == wall_S) { wallDir = DungeonRoomInfo.WallDirection.South; }
        else if (wall == wall_W) { wallDir = DungeonRoomInfo.WallDirection.West; }

        return wallDir;
    }

    public bool IsDoorOpen(DungeonRoomInfo.WallDirection dir)
    {
        return GetWallTypeForDirection(dir) == DungeonRoomInfo.WallType.DoorOpen;
    }
    public bool CanPassThrough(DungeonRoomInfo.WallDirection direction)
    {
        return Info.CanPassThrough(direction);
    }
    public bool IsWallBombedOpen(DungeonRoomInfo.WallDirection dir)
    {
        return GetWallTypeForDirection(dir) == DungeonRoomInfo.WallType.Bombed;
    }
    public bool IsDoorLocked(DungeonRoomInfo.WallDirection dir)
    {
        return GetWallTypeForDirection(dir) == DungeonRoomInfo.WallType.DoorLocked;
    }
    public bool IsDoorSealed(DungeonRoomInfo.WallDirection dir)
    {
        return GetWallTypeForDirection(dir) == DungeonRoomInfo.WallType.DoorSealed;
    }

    public void UnlockDoor(DungeonRoomInfo.WallDirection dir)
    {
        if (!IsDoorLocked(dir))
        {
            //Debug.LogError("Trying to Unlock door that is not Locked.");
            return;
        }
        SetWallTypeForDirection(dir, DungeonRoomInfo.WallType.DoorOpen);

        DungeonRoom adjoiningRoom = GetAdjoiningRoom(dir);
        if (adjoiningRoom != null) { adjoiningRoom.UnlockDoor(DungeonRoomInfo.GetOppositeDirection(dir)); }

        SoundFx.Instance.PlayOneShot(SoundFx.Instance.unlock);
    }
    public void SealDoor(DungeonRoomInfo.WallDirection dir)
    {
        if (!IsDoorOpen(dir))
        {
            //Debug.LogError("Trying to Seal door that is not Open.");
            return;
        }
        SetWallTypeForDirection(dir, DungeonRoomInfo.WallType.DoorSealed);

        //DungeonRoom adjoiningRoom = GetAdjoiningRoom(dir);
        //if (adjoiningRoom != null) { adjoiningRoom.SealDoor(DungeonRoomInfo.GetOppositeDirection(dir)); }

        SoundFx.Instance.PlayOneShot(SoundFx.Instance.unlock);
    }
    public void UnsealDoor(DungeonRoomInfo.WallDirection dir)
    {
        if (!IsDoorSealed(dir))
        {
            //Debug.LogError("Trying to Unseal door that is not Sealed.");
            return;
        }
        SetWallTypeForDirection(dir, DungeonRoomInfo.WallType.DoorOpen);

        //DungeonRoom adjoiningRoom = GetAdjoiningRoom(dir);
        //if (adjoiningRoom != null) { adjoiningRoom.UnsealDoor(DungeonRoomInfo.GetOppositeDirection(dir)); }

        SoundFx.Instance.PlayOneShot(SoundFx.Instance.unlock);
    }
    public void SealDoors()
    {
        SealDoor(DungeonRoomInfo.WallDirection.North);
        SealDoor(DungeonRoomInfo.WallDirection.East);
        SealDoor(DungeonRoomInfo.WallDirection.South);
        SealDoor(DungeonRoomInfo.WallDirection.West);
    }
    public void UnsealDoors()
    {
        UnsealDoor(DungeonRoomInfo.WallDirection.North);
        UnsealDoor(DungeonRoomInfo.WallDirection.East);
        UnsealDoor(DungeonRoomInfo.WallDirection.South);
        UnsealDoor(DungeonRoomInfo.WallDirection.West);
    }
    public void BlowHoleInWall(DungeonRoomInfo.WallDirection dir)
    {
        if (IsWallBombedOpen(dir))
        {
            //Debug.LogError("Trying to Blow Hole In Wall that is already bombed open.");
            return;
        }
        if (!Info.IsBombable(dir))
        {
            Debug.LogError("Trying to Blow Hole In Wall that is not bombable.");
            return;
        }
        SetWallTypeForDirection(dir, DungeonRoomInfo.WallType.Bombed);

        DungeonRoom adjoiningRoom = GetAdjoiningRoom(dir);
        if (adjoiningRoom != null) { adjoiningRoom.BlowHoleInWall(DungeonRoomInfo.GetOppositeDirection(dir)); }
    }


    public void onPlayerEnteredRoom(DungeonRoomInfo.WallDirection direction = DungeonRoomInfo.WallDirection.None)
    {
        PlayerHasVisited = true;

        if (!EnemiesHaveSpawned)
        {
            SpawnEnemies();

            if (ContainsBoss && ALWAYS_SEAL_DOORS_FOR_BOSS)
            {
                SealDoors();
            }
        }

        if (NeedTriforceToPass && Inventory.Instance.HasAllTriforcePieces())
        {
            UnsealDoors();
        }

        if(direction != DungeonRoomInfo.WallDirection.None)
        {
            if(GetWallTypeForDirection(direction) == DungeonRoomInfo.WallType.DoorSealed)
            {
                SoundFx.Instance.PlayOneShot(SoundFx.Instance.sealDoor);
            }
        }

        if (WillShowNpcText)
        {
            ShowNpcText();
        }

        if (ContainsSubDungeonSpawnPoint)
        {
            if (!Info.pushBlockSpawnsSubDungeon || _hasPushBlockBeenPushed)
            {
                SpawnSubDungeon();
            }
        }
    }
    public void onPlayerExitedRoom()
    {
        if (LightsOffOnExit)
        {
            ActivateTorchLights(false);
        }

        if (IsNpcRoom)
        {
            ShowNpcText(false);
        }

        if (ContainsSubDungeonSpawnPoint)
        {
            DestroySubDungeon();
        }
    }

    public void OnPlayerDiedInThisRoom()
    {
        /*if (ContainsBoss)
        {
            UnsealDoors();
        }*/
    }

    public void OnRoomEnemyDied(Enemy enemy)
    {
        //print("OnRoomEnemyDied: " + enemy.name);

        Enemies.Remove(enemy);
        KillableEnemies.Remove(enemy);
        if (KillableEnemies.Count == 0)
        {
            OnRoomCleared();
        }
    }
    void OnRoomCleared()
    {
        //print("OnRoomCleared: " + name);

        // Item appear
        if (!Info.ItemOnClearHasBeenCollected && ItemOnClear != null)
        {
            GameObject item = ItemOnClear.gameObject;
            item.SetActive(true);

            SoundFx sfx = SoundFx.Instance;
            sfx.PlayOneShot3D(item.transform.position, sfx.key);
        }

        // UnsealDoors after boss fight
        if (ContainsBoss)
        { 
            Info.BossHasBeenDefeated = true;
            UnsealDoors();
        }

        // Unseal doors
        foreach (var d in Info.doorsOpenOnClear)
        {
            if (d != DungeonRoomInfo.WallDirection.None)
            {
                UnsealDoor(d);
            }
        }

        // Enable Pushable block
        if (_pushBlock)
        {
            _pushBlock.PushingEnabled = true;
        }
    }

    public void OnPushableWasPushedIntoPosition()
    {
        //print("OnPushableWasPushedIntoPosition" );
        if (Info.pushBlockDoorDirection != DungeonRoomInfo.WallDirection.None)
        {
            UnsealDoor(Info.pushBlockDoorDirection);
        }

        if (Info.pushBlockSpawnsSubDungeon)
        {
            SpawnSubDungeon();
            floor.GetComponent<Renderer>().material = Info.PushBlockChangeFloorMaterial;
        }

        SoundFx.Instance.PlayOneShot(SoundFx.Instance.secret);

        _hasPushBlockBeenPushed = true;
    }

    public void OnItemCollectedWithinThisRoom(Collectible c)
    {
        if (c == SpecialItem)
        {
            Info.SpecialItemHasBeenCollected = true;
        }
        else if (c == ItemOnClear)
        {
            Info.ItemOnClearHasBeenCollected = true;
        }
        else
        {
            foreach (var sp in _enemySpawnPoints)
            {
                if (c == sp.specialDrop)
                {
                    SpecialDropItemHasBeenCollected = true;
                    break;
                }
            }
        }
    }


    public Vector2 GetSectorIndices()
    {
        Vector3 pos = transform.position;
        int x = (int)(pos.x / TilesWide_WithHalls);
        int z = (int)(pos.z / TilesLong_WithHalls);

        return new Vector2(x, z);
    }

    public Vector2 WorldPointToTile(Vector3 worldPos)
    {
        Vector3 pos = transform.position;
        int x = (int)(worldPos.x - pos.x + TilesWide * 0.5f);
        int z = (int)(worldPos.z - pos.z + TilesLong * 0.5f);

        return new Vector2(x, z);
    }

    public Rect TileToFloorTextureArea(Vector2 tile)
    {
        Texture2D floorTexture = floor.GetComponent<Renderer>().material.mainTexture as Texture2D;
        float tileWidth_pixels = floorTexture.width / TilesWide;
        float tileHeight_pixels = floorTexture.height / TilesLong;

        return new Rect(
            tile.x * tileWidth_pixels, 
            tile.y * tileHeight_pixels,
            tileWidth_pixels,
            tileHeight_pixels);
    }


    public static DungeonRoom GetRoomForPosition(Vector3 position)
    {
        int x = (int)(position.x / TilesWide_WithHalls);
        int z = (int)(position.z / TilesLong_WithHalls);

        return CommonObjects.CurrentDungeonFactory.GetRoomAtGridPosition(x, z);
    }
}