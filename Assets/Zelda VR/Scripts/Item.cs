using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    const string GUI_SPRITE_GAME_OBJECT_NAME = "GuiSprite";


    public int count, maxCount = 1;
    public bool AppearsInInventoryGUI;
    public GameObject weaponPrefab, shieldPrefab;

    public Item upgradesTo;         // The Item that this Item will upgrade to (if any)
    public Item UpgradedFrom { get; private set; }

    public bool useImmediately;
    public bool consumable;      // Whether or not using the item decrements it's count

    public Sprite hudSprite;


    Sprite _guiSprite;
    Texture _guiSpriteTexture;


    void Awake()
    {
        FindGuiSprite();
    }

    void Start()
    {
        transform.position = new Vector3(9999, 9999, 9999);

        if (upgradesTo != null)
        {
            upgradesTo = GameObject.Find(upgradesTo.name).GetComponent<Item>();
            upgradesTo.UpgradedFrom = this;
        }
    }


    public Texture GetGuiTexture()
    {
        if (_guiSpriteTexture == null)
        {
            if (_guiSprite == null) { FindGuiSprite(); }
            _guiSpriteTexture = (_guiSprite == null) ? null : _guiSprite.GetTextureSegment();
        }
        return _guiSpriteTexture;
    }

    void FindGuiSprite()
    {
        foreach (var sr in transform.GetComponentsInChildren<SpriteRenderer>())
        {
            if (sr.name == GUI_SPRITE_GAME_OBJECT_NAME)
            {
                _guiSprite = sr.sprite;
                break;
            }
        }
    }


    public Item GetLeastUpgradedVersion()
    {
        Item item = this;
        while (item.UpgradedFrom != null)
        {
            item = item.UpgradedFrom;
        }
        return item;
    }

    public Item GetHighestUpgradedVersion()
    {
        Item item = this;
        while (item.upgradesTo != null)
        {
            item = item.upgradesTo;
        }
        return item;
    }

    public Item HighestUpgradedVersionInInventory()
    {
        Item item = GetHighestUpgradedVersion();
        do
        {
            if (item.count > 0) { break; }
            item = item.UpgradedFrom;
        } while (item != null);
        return item;
    }

    public bool IsTheHighestUpgradeInInventory()
    {
        return name == HighestUpgradedVersionInInventory().name;
    }

    public List<Item> GetUpgradeChain()
    {
        List<Item> chain = new List<Item>();
        Item item = GetLeastUpgradedVersion();
        do
        {
            chain.Add(item);
            item = item.upgradesTo;
        }
        while (item != null);
        return chain;
    }

    public bool IsItemInUpgradeChain(Item item)
    {
        return GetUpgradeChain().Contains(item);
    }


    public void OnCollected(int amount = 1)
    {
        count = Mathf.Min(count + amount, maxCount);
        count = Mathf.Max(count, 0);
    }

    public void Use()
    {
        if (count == 0)
        {
            return;
        }

        if (consumable)
        {
            count = Mathf.Max(count - 1, 0);
        }

        SendMessage("OnItemUsed", SendMessageOptions.DontRequireReceiver);
    }
}