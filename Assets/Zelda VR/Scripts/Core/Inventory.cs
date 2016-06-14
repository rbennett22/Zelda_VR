using Immersio.Utility;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : Singleton<Inventory>
{
    const int BOMB_CAPACITY_BASE = 8;
    const int BOMBS_PER_UPGRADE = 4;

    const string ITEM_PREFABS_PATH = "Item Prefabs";        // (relative to a Resources folder)


    public Transform itemsContainer;


    Dictionary<string, Item> _items;
    public Dictionary<string, Item> Items { get { return _items; } private set { _items = value; } }
    public Item GetItem(string name) { return (name == null) ? null : Items[name]; }
    public bool HasItem(string name)
    {
        Item item = GetItem(name);
        return (item != null) && (item.count > 0);
    }
    public void RemoveItem(string name) { if (HasItem(name)) { GetItem(name).count = 0; } }


    public int GetArmorLevel()
    {
        int level = 0;
        if (HasItem("BlueRing")) { level = 2; }
        else if (HasItem("RedRing")) { level = 1; }
        return level;
    }

    public int GetShieldLevel()
    {
        int level = 0;
        if (HasItem("MagicShield")) { level = 2; }
        else if (HasItem("WoodenShield")) { level = 1; }
        return level;
    }

    public int GetSwordLevel()
    {
        int level = 0;
        if (HasItem("MagicSword")) { level = 3; }
        else if (HasItem("WhiteSword")) { level = 2; }
        else if (HasItem("WoodenSword")) { level = 1; }
        return level;
    }

    void EquipHighestLevelShieldInPossesion()
    {
        if (HasItem("MagicShield"))
        {
            EquippedShield = GetItem("MagicShield");
        }
        else if (HasItem("WoodenShield"))
        {
            EquippedShield = GetItem("WoodenShield");
        }
        else
        {
            EquippedShield = null;
        }
    }
    void EquipHighestLevelSwordInPossesion()
    {
        if (HasItem("MagicSword"))
        {
            EquippedItemA = GetItem("MagicSword");
        }
        else if (HasItem("WhiteSword"))
        {
            EquippedItemA = GetItem("WhiteSword");
        }
        else if (HasItem("WoodenSword"))
        {
            EquippedItemA = GetItem("WoodenSword");
        }
        else
        {
            EquippedItemA = null;
        }
    }


    bool[] _hasCompassForDungeon = new bool[WorldInfo.NUM_DUNGEONS];
    public bool HasCompassForDungeon(int dungeon)
    {
        if (!IsDungeonNumValid(dungeon))
            return false;
        return _hasCompassForDungeon[dungeon - 1];
    }
    public void SetHasCompassForDungeon(int dungeon, bool hasCompass)
    {
        if (!IsDungeonNumValid(dungeon))
            return;
        _hasCompassForDungeon[dungeon - 1] = hasCompass;
    }

    bool[] _hasMapForDungeon = new bool[WorldInfo.NUM_DUNGEONS];
    public bool HasMapForDungeon(int dungeon)
    {
        if (!IsDungeonNumValid(dungeon))
            return false;
        return _hasMapForDungeon[dungeon - 1];
    }
    public void SetHasMapForDungeon(int dungeon, bool hasMap)
    {
        if (!IsDungeonNumValid(dungeon))
            return;
        _hasMapForDungeon[dungeon - 1] = hasMap;
    }
    bool IsDungeonNumValid(int n)
    {
        return n > 0 && n <= WorldInfo.NUM_DUNGEONS;
    }

    bool[] _hasTriforcePieceForDungeon = new bool[WorldInfo.NUM_DUNGEONS - 1];
    public bool HasTriforcePieceForDungeon(int dungeon)
    {
        if (!IsDungeonNumValid(dungeon)) { return false; }
        if (dungeon == 9) { return false; }     // (9th Dungeon doesn't have a Triforce piece)
        return _hasTriforcePieceForDungeon[dungeon - 1];
    }
    public void SetHasTriforcePieceForDungeon(int dungeon, bool hasTri)
    {
        if (!IsDungeonNumValid(dungeon)) { return; }
        if (dungeon == 9) { return; }           // (9th Dungeon doesn't have a Triforce piece)
        _hasTriforcePieceForDungeon[dungeon - 1] = hasTri;
    }

    public bool HasAllTriforcePieces()
    {
        for (int i = 1; i <= 8; i++)
        {
            if (!HasTriforcePieceForDungeon(i)) { return false; }
        }
        return true;
    }

    public bool HasDeliveredLetterToOldWoman { get; set; }


    Item _equippedItemA;
    public Item EquippedItemA
    {
        get { return _equippedItemA; }
        set
        {
            _equippedItemA = value;
            if (_equippedItemA != null)
            {
                CommonObjects.Player_C.EquipSword(_equippedItemA.name);
            }
            else
            {
                CommonObjects.Player_C.DeequipSword();
            }
        }
    }

    Item _equippedItemB;
    public Item EquippedItemB
    {
        get { return _equippedItemB; }
        set
        {
            _equippedItemB = value;
            if (_equippedItemB != null)
            {
                CommonObjects.Player_C.EquipItem(_equippedItemB.name);
            }
            else
            {
                CommonObjects.Player_C.DeequipItem();
            }
        }
    }

    Item _equippedShield;
    public Item EquippedShield
    {
        get { return _equippedShield; }
        set
        {
            _equippedShield = value;
            if (_equippedShield != null)
            {
                CommonObjects.Player_C.EquipShield(_equippedShield.name);
            }
            else
            {
                CommonObjects.Player_C.DeequipShield();
            }
        }
    }


    Item[,,] _equippableSecondaryItems;

    int _targetRupeeCount;
    bool _animateRupeeCount;
    public int RupeeCount { get { return GetItem("Rupee").count; } }
    public bool CanAfford(int price) { return RupeeCount >= price; }
    public bool SpendRupees(int amount)
    {
        if (!CanAfford(amount)) { return false; }

        if (amount < 10)
        {
            GetItem("Rupee").count -= amount;
        }
        else
        {
            int targetCount = RupeeCount - amount;
            //StartCoroutine("AnimateRupeeCount", targetCount);
            _targetRupeeCount = targetCount;
            _animateRupeeCount = true;
        }
        return true;
    }
    public void ReceiveRupees(int amount)
    {
        if (amount < 10)
        {
            GetItem("Rupee").OnCollected(amount);
        }
        else
        {
            int targetCount = Mathf.Min(RupeeCount + amount, GetItem("Rupee").maxCount);
            //StartCoroutine("AnimateRupeeCount", targetCount);
            _targetRupeeCount = targetCount;
            _animateRupeeCount = true;
        }
    }


    void Update()
    {
        if (_animateRupeeCount)
        {
            UpdateRupeeCountAnimation();
        }
    }

    void UpdateRupeeCountAnimation()
    {
        Item rupees = GetItem("Rupee");
        SoundFx sfx = SoundFx.Instance;

        if (_targetRupeeCount > rupees.count) { rupees.count++; }
        else { rupees.count--; }

        sfx.PlayOneShot(sfx.text);

        if (rupees.count == _targetRupeeCount)
        {
            _animateRupeeCount = false;
        }
    }


    override protected void Awake()
    {
        base.Awake();

        InitItems();
    }

    void InitItems()
    {
        Items = new Dictionary<string, Item>();

        foreach (var itemPrefab in Resources.LoadAll<GameObject>(ITEM_PREFABS_PATH))
        {
            //print(itemPrefab.name);
            GameObject g = Instantiate(itemPrefab) as GameObject;
            g.name = itemPrefab.name;
            g.transform.parent = itemsContainer;
            Items.Add(g.name, g.GetComponent<Item>());
        }

        _equippableSecondaryItems = new Item[3, 2, 4] {
            {{ null,                        null,               null,                       null },
             { null,                        null,               GetItem("Letter"),          null }},
            {{ GetItem("WoodenBoomerang"),  null,               GetItem("WoodenBow"),       GetItem("BlueCandle") },
             { null,                        null,               GetItem("BlueMedicine"),    null }},
            {{ GetItem("MagicBoomerang"),   GetItem("Bomb"),    GetItem("SilverBow"),       GetItem("RedCandle") },
             { GetItem("Whistle"),          GetItem("Bait"),    GetItem("RedMedicine"),     GetItem("MagicWand") }}};
    }

    public Item GetEquippableSecondaryItem(int xIdx, int yIdx)
    {
        //print("GetEquippableSecondaryItem(" + xIdx + ", " + yIdx + ")");

        Item item = _equippableSecondaryItems[2, yIdx, xIdx];
        if (item == null || item.count == 0)
        {
            item = _equippableSecondaryItems[1, yIdx, xIdx];
        }
        if (item == null || item.count == 0)
        {
            item = _equippableSecondaryItems[0, yIdx, xIdx];
        }
        if (item == null || item.count == 0)
        {
            item = null;
        }
        return item;
    }

    public bool IsItemAnEquippableSecondaryItem(string itemName)
    {
        for (int i = 0; i < 3; i++)
        {
            for (int y = 0; y < 2; y++)
            {
                for (int x = 0; x < 4; x++)
                {
                    Item item = _equippableSecondaryItems[i, y, x];
                    if (item == null || item.name != itemName) { continue; }
                    return true;
                }
            }
        }

        return false;
    }


    void SyncPlayerHealthWithHeartContainers(bool restoreHealth = false)
    {
        HealthController hc = CommonObjects.Player_C.GetComponent<HealthController>();
        hc.maxHealth = PlayerHealthDelegate.HalfHeartsToHealth(GetItem("HeartContainer").count * 2);
        if (restoreHealth) { hc.RestoreHealth(); }
    }


    public void OnItemCollected(Collectible collectible, int amount = 1)
    {
        string itemName = collectible.itemPrefab.name;
        OnItemCollected(itemName, amount);

        if (itemName == "TriforcePiece")
        {
            collectible.GetComponent<Triforce>().Fanfare();
        }
    }

    public void OnItemCollected(string itemName, int amount = 1)
    {
        Item item = GetItem(itemName);

        int prevCount = item.count;
        item.OnCollected(amount);

        if (IsItemASword(item))
        {
            if (prevCount == 0)
            {
                if (item.IsTheHighestUpgradeInInventory())
                {
                    EquippedItemA = item;
                }
            }
        }
        else if (IsItemAShield(item))
        {
            if (prevCount == 0)
            {
                if (item.IsTheHighestUpgradeInInventory())
                {
                    EquippedShield = item;
                }
            }
        }
        else if (IsItemAnEquippableSecondaryItem(itemName))
        {
            if (_equippedItemB == null || item.IsItemInUpgradeChain(_equippedItemB))
            {
                if (prevCount == 0)
                {
                    if (item.IsTheHighestUpgradeInInventory())
                    {
                        EquippedItemB = item;
                    }
                }
            }

            foreach (var subItem in item.GetUpgradeChain())
            {
                print("subItem: " + subItem.name);
                if (subItem == item) { break; }
                subItem.OnCollected();
            }
        }

        if (item.name == "HeartContainer")
        {
            SyncPlayerHealthWithHeartContainers();
            CommonObjects.Player_C.RestoreHearts(1);
        }
        else if (item.name == "TriforcePiece")
        {
            SetHasTriforcePieceForDungeon(WorldInfo.Instance.DungeonNum, true);
        }
        else if (item.name == "BombUpgrade")
        {
            ApplyBombUpgrades();
            GetItem("Bomb").OnCollected(BOMBS_PER_UPGRADE);
        }
        else if (item.name == "SilverArrow")
        {
            OnItemCollected("SilverBow");
        }


        if (item.useImmediately)
        {
            UseItem(item.name);
        }
    }

    public bool IsItemASword(Item item)
    {
        if (item == null) { return false; }

        return (
            (item.name == "WoodenSword") ||
            (item.name == "WhiteSword") ||
            (item.name == "MagicSword"));
    }
    public bool IsItemAShield(Item item)
    {
        if (item == null) { return false; }

        return (
            (item.name == "WoodenShield") ||
            (item.name == "MagicShield"));
    }


    public void UseItem(string itemName)
    {
        Item item = GetItem(itemName);
        if (!HasItem(itemName)) { return; }

        item.Use();

        if (!HasItem(itemName))
        {
            Item upgradesFrom = item.UpgradedFrom;
            if (upgradesFrom != null)
            {
                upgradesFrom.count = 1;
                if (EquippedItemB == item)
                {
                    EquippedItemB = upgradesFrom;
                }
            }

            if (EquippedItemB == item)
            {
                EquippedItemB = null;
            }
        }

        // TODO: Store a compass/map/triforce item for each dungeon
        if (itemName == "Compass")
        {
            SetHasCompassForDungeon(WorldInfo.Instance.DungeonNum, true);
        }
        else if (itemName == "Map")
        {
            SetHasMapForDungeon(WorldInfo.Instance.DungeonNum, true);
        }

        // TODO: Use events instead
        if (itemName == "Letter")
        {
            if (!HasDeliveredLetterToOldWoman)
            {
                Grotto grotto = Grotto.OccupiedGrotto;
                if (grotto != null && grotto.Type == Grotto.GrottoType.Medicine)
                {
                    grotto.DeliverLetter();
                }
            }
        }
    }

    public void UseItemB()
    {
        if (_equippedItemB == null) { return; }

        UseItem(EquippedItemB.name);
    }


    public void EquipSword_Cheat(string swordName)
    {
        Item sword = GetItem(swordName);
        sword.count = 1;
        EquippedItemA = sword;
    }

    public void MaxOutEverything()
    {
        foreach (Item item in Items.Values)
        {
            item.count = item.maxCount;
        }

        ApplyBombUpgrades();
        GetItem("Bomb").count = GetItem("Bomb").maxCount;

        for (int i = 1; i <= WorldInfo.NUM_DUNGEONS; i++)
        {
            SetHasCompassForDungeon(i, true);
            SetHasMapForDungeon(i, true);
            SetHasTriforcePieceForDungeon(i, true);
        }

        HasDeliveredLetterToOldWoman = true;

        EquippedItemA = GetItem("MagicSword");
        EquippedItemB = GetItem("MagicBoomerang");
        EquippedShield = GetItem("MagicShield");

        SyncPlayerHealthWithHeartContainers(true);
    }


    void ApplyBombUpgrades()
    {
        GetItem("Bomb").maxCount = BOMB_CAPACITY_BASE + GetItem("BombUpgrade").count * BOMBS_PER_UPGRADE;
    }


    #region Serialization

    public class InventoryInfo
    {
        public string[] itemNames;
        public int[] itemCounts;

        public string equippedItemA, equippedItemB, equippedShield;

        public bool[] hasCompassForDungeon = new bool[WorldInfo.NUM_DUNGEONS];
        public bool[] hasMapForDungeon = new bool[WorldInfo.NUM_DUNGEONS];
        public bool[] hasTriforcePieceForDungeon = new bool[WorldInfo.NUM_DUNGEONS - 1];
        public bool hasDeliveredLetterToOldWoman;
    }


    public InventoryInfo GetInfo()
    {
        InventoryInfo info = new InventoryInfo();

        int numItems = _items.Keys.Count;
        info.itemNames = new string[numItems];
        info.itemCounts = new int[numItems];
        int i = 0;
        foreach (KeyValuePair<string, Item> entry in _items)
        {
            Item item = entry.Value;
            info.itemNames[i] = item.name;
            info.itemCounts[i] = item.count;
            i++;
        }

        info.equippedItemA = (_equippedItemA == null) ? "null" : _equippedItemA.name;
        info.equippedItemB = (_equippedItemB == null) ? "null" : _equippedItemB.name;
        info.equippedShield = (_equippedShield == null) ? "null" : _equippedShield.name;

        info.hasCompassForDungeon = _hasCompassForDungeon;
        info.hasMapForDungeon = _hasMapForDungeon;
        info.hasTriforcePieceForDungeon = _hasTriforcePieceForDungeon;
        info.hasDeliveredLetterToOldWoman = HasDeliveredLetterToOldWoman;

        return info;
    }

    public void InitWithInfo(InventoryInfo info)
    {
        int numItems = info.itemNames.Length;
        for (int i = 0; i < numItems; i++)
        {
            string itemName = info.itemNames[i];
            int itemCount = info.itemCounts[i];
            _items[itemName].count = itemCount;
        }

        _hasCompassForDungeon = info.hasCompassForDungeon;
        _hasMapForDungeon = info.hasMapForDungeon;
        _hasTriforcePieceForDungeon = info.hasTriforcePieceForDungeon;
        HasDeliveredLetterToOldWoman = info.hasDeliveredLetterToOldWoman;

        string b = info.equippedItemB;
        if (b == "null") { b = null; }
        EquippedItemB = GetItem(b);

        EquipHighestLevelSwordInPossesion();
        EquipHighestLevelShieldInPossesion();

        ApplyBombUpgrades();
        SyncPlayerHealthWithHeartContainers();
    }

    #endregion Serialization


    void Print()
    {
        string output = " ~~~  INVENTORY  ~~~\n";
        foreach (var item in _items.Values)
        {
            output += item.name + ": " + item.count + "\n";
        }
        print(output);
    }
}