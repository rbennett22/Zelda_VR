using UnityEngine;
using System.Collections.Generic;

public class Item : MonoBehaviour
{
    const string GuiSpriteGameObjectName = "GuiSprite";


    public int count, maxCount = 1;
    public bool AppearsInInventoryGUI;
    public GameObject weaponPrefab;

    public Item upgradesTo;         // The Item that this Item will upgrade to (if any)
    public Item UpgradedFrom { get; set; }

    public bool useImmediately;
    public bool consumable;      // Does using the item decrement it's count

    public Sprite hudSprite;


    Sprite _guiSprite;
    Texture _guiSpriteTexture, _hudSpriteTexture;


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


    /*public Sprite GetGuiSprite()
    {
        if (_guiSprite == null) { FindGuiSprite(); }
        return _guiSprite;
    }*/
    public Texture GetGuiTexture()
    {
        if (_guiSpriteTexture == null)
        {
            if (_guiSprite == null) { FindGuiSprite(); }
            _guiSpriteTexture = (_guiSprite == null) ? null : _guiSprite.GetTextureSegment();
        }
        return _guiSpriteTexture;
    }
    /*public Texture GetHudTexture()
    {
        if (_hudSpriteTexture == null)
        {
            _hudSpriteTexture = (hudSprite == null) ? GetGuiTexture() : hudSprite.GetTextureSegment();
        }
        return _hudSpriteTexture;
    }*/

    void FindGuiSprite()
    {
        foreach (var sr in transform.GetComponentsInChildren<SpriteRenderer>())
        {
            if (sr.name != GuiSpriteGameObjectName) { continue; }
            _guiSprite = sr.sprite;
            break;
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
        do {
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
        if (consumable)
        {
            count = Mathf.Max(count - 1, 0);
        }

        SendMessage("OnItemUsed", SendMessageOptions.DontRequireReceiver);
    }  
}