using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Immersio.Utility;


public class Inventory : Singleton<Inventory> 
{
    public const int DungeonCount = 9;

    const string ItemPrefabsPath = "ZeldaItemPrefabs";


    public InventoryGUI inventoryGui;
    public Transform itemsContainer;


    Dictionary<string, Item> _items;
    public Dictionary<string, Item> Items { get { return _items; } private set { _items = value; } }
    public Item GetItem(string name) { return (name == null) ? null : Items[name]; }


    public int GetArmorLevel()
    {
        int level = 0;
        if (GetItem("BlueRing").count > 0) { level = 2; }
        else if (GetItem("RedRing").count > 0) { level = 1; }
        return level;
    }

    public int GetSwordLevel()
    {
        int level = 0;
        if (GetItem("MagicSword").count > 0) { level = 3; }
        else if (GetItem("WhiteSword").count > 0) { level = 2; }
        else if (GetItem("WoodenSword").count > 0) { level = 1; }
        return level;
    }


    bool[] _hasCompassForDungeon = new bool[DungeonCount];
    public bool HasCompassForDungeon(int dungeon)
    {
        return _hasCompassForDungeon[dungeon - 1];
    }
    public void SetHasCompassForDungeon(int dungeon, bool hasCompass)
    {
        _hasCompassForDungeon[dungeon - 1] = hasCompass;
    }

    bool[] _hasMapForDungeon = new bool[DungeonCount];
    public bool HasMapForDungeon(int dungeon)
    {
        return _hasMapForDungeon[dungeon - 1];
    }
    public void SetHasMapForDungeon(int dungeon, bool hasMap)
    {
        _hasMapForDungeon[dungeon - 1] = hasMap;
    }

    bool[] _hasTriforcePieceForDungeon = new bool[DungeonCount - 1];
    public bool HasTriforcePieceForDungeon(int dungeon)
    {
        if (dungeon == 9) { return false; }     // (9th Dungeon doesn't have a Triforce piece)
        return _hasTriforcePieceForDungeon[dungeon - 1];
    }
    public void SetHasTriforcePieceForDungeon(int dungeon, bool hasTri)
    {
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
                inventoryGui.SetCursorIndex(GetCursorIndexForItem(_equippedItemB));
                CommonObjects.Player_C.EquipSecondaryItem(_equippedItemB.name);
            }
            else
            {
                CommonObjects.Player_C.DeequipSecondaryItem();
            }
        }
    }


    Item[, ,] _equippableSecondaryItems;
    
    int _targetRupeeCount;
    bool _animateRupeeCount;
    public int RupeeCount { get { return GetItem("Rupee").count; } }
    public bool SpendRupees(int amount)
    {
        if (amount > RupeeCount) { return false; }

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

	
	void Awake ()
    {
        InitItems();
	}

    void InitItems()
    {
        Items = new Dictionary<string, Item>();

        foreach (var itemPrefab in Resources.LoadAll<GameObject>(ItemPrefabsPath))
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

    public Vector2 GetCursorIndexForItem(Item item)
    {
        if(item == null) 
        { 
            return Vector2.zero; 
        }

        for (int i = 0; i < 3; i++)
		{
            for (int y = 0; y < 2; y++)
            {
                for (int x = 0; x < 4; x++)
                {
                    if (_equippableSecondaryItems[i, y, x] != item) { continue; }
                    return new Vector2(x, y);
                }
            }
		}
        
        return Vector2.zero;
    }


    void SyncPlayerHealthWithHeartContainers(bool restoreHealth = true)
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
        }
        else if (item.name == "TriforcePiece")
        {
            SetHasTriforcePieceForDungeon(WorldInfo.Instance.DungeonNum, true);
        }
        else if (item.name == "BombUpgrade")
        {
            ApplyBombUpgrades();
            GetItem("Bomb").OnCollected(4);
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


    public void UseItem(string itemName)
    {
        Item item = GetItem(itemName);
        if (item.count == 0) { return; }

        item.Use();

        if (item.count == 0)
        {
            if (EquippedItemB == item)
            {
                EquippedItemB = null;
            }
        }

        if (item.name == "Heart")
        {
            CommonObjects.Player_C.RestoreHalfHearts(2);
        }
        else if (item.name == "Fairy")
        {
            CommonObjects.Player_C.RestoreHalfHearts(6);
        }
        else if (item.name == "Compass")
        {
            SetHasCompassForDungeon(WorldInfo.Instance.DungeonNum, true);
        }
        else if (item.name == "Map")
        {
            SetHasMapForDungeon(WorldInfo.Instance.DungeonNum, true);
        }
        else if (item.name == "RedMedicine")
        {
            CommonObjects.Player_C.GetComponent<HealthController>().RestoreHealth();
            Item blueMedicine = GetItem("BlueMedicine");
            blueMedicine.count = 1;
            EquippedItemB = blueMedicine;
        }
        else if (item.name == "BlueMedicine")
        {
            CommonObjects.Player_C.GetComponent<HealthController>().RestoreHealth();
            Item letter = GetItem("Letter");
            letter.count = 1;
            EquippedItemB = letter;
        }
        else if (item.name == "Letter")
        {
            if (!HasDeliveredLetterToOldWoman)
            {
                Grotto grotto = Grotto.OccupiedGrotto;
                if (grotto != null && grotto.GrottoType == GrottoSpawnPoint.GrottoType.Medicine)
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
        GetItem(swordName).count = 1;
        EquippedItemA = GetItem(swordName);
    }


    public void MaxOutEverything()
    {
        foreach (KeyValuePair<string, Item> entry in Items)
        {
            Item item = entry.Value;
            item.count = item.maxCount;
        }

        ApplyBombUpgrades();
        GetItem("Bomb").count = GetItem("Bomb").maxCount;

        for (int i = 1; i <= DungeonCount; i++)
        {
            SetHasCompassForDungeon(i, true);
            SetHasMapForDungeon(i, true);
            SetHasTriforcePieceForDungeon(i, true);
        }

        HasDeliveredLetterToOldWoman = true;

        EquippedItemA = GetItem("MagicSword");
        EquippedItemB = GetItem("SilverBow");

        SyncPlayerHealthWithHeartContainers();
    }


    void ApplyBombUpgrades()
    {
        GetItem("Bomb").maxCount = 8 + GetItem("BombUpgrade").count * 4;
    }


    #region Save/Load

    public class InventoryInfo
    {
        public string[] itemNames;
        public int[] itemCounts;

        public string equippedItemA, equippedItemB;

        public bool[] hasCompassForDungeon = new bool[DungeonCount];
        public bool[] hasMapForDungeon = new bool[DungeonCount];
        public bool[] hasTriforcePieceForDungeon = new bool[DungeonCount - 1];
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

        string a = info.equippedItemA;
        if (a == "null") { a = null; }
        string b = info.equippedItemB;
        if (b == "null") { b = null; }
        EquippedItemA = GetItem(a);
        EquippedItemB = GetItem(b);

        _hasCompassForDungeon = info.hasCompassForDungeon;
        _hasMapForDungeon = info.hasMapForDungeon;
        _hasTriforcePieceForDungeon = info.hasTriforcePieceForDungeon;
        HasDeliveredLetterToOldWoman = info.hasDeliveredLetterToOldWoman;


        ApplyBombUpgrades();
        SyncPlayerHealthWithHeartContainers();
    }

    #endregion


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