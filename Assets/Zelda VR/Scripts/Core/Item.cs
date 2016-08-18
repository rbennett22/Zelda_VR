using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Item : MonoBehaviour
{
    readonly static Vector3 OFFSCREEN_POSITION = 9999 * Vector3.one;


    public int count, maxCount = 1;
    public bool AppearsInInventoryGUI;
    public GameObject weaponPrefab, shieldPrefab;

    public Item upgradesTo;         // The Item that this Item will upgrade to (if any)
    public Item UpgradedFrom { get; private set; }

    public bool useImmediately;
    public bool consumable;      // Whether or not using the item decrements it's count


    List<Spell_Base> _triggerSpellsList;                // Spells to cast when item is used
    public List<Spell_Base> TriggerSpellsList {
        get { return _triggerSpellsList ?? (_triggerSpellsList = new List<Spell_Base>(GetComponentsInChildren<Spell_Base>())); }
    }

    Sprite _guiSprite;
    public Sprite GuiSprite { get { return _guiSprite ?? (_guiSprite = FindGuiSpriteInChildren()); } }
    Sprite FindGuiSpriteInChildren()
    {
        return GetComponentsInChildren<SpriteRenderer>()
            .Where(c => c.transform != this.transform)
            .Single().sprite;
    }

    Texture _guiSpriteTexture;
    public Texture GuiSpriteTexture { get { return _guiSpriteTexture ?? (_guiSpriteTexture = FindGuiSpriteTexture()); } }
    Texture FindGuiSpriteTexture()
    {
        return (GuiSprite == null) ? null : GuiSprite.GetTextureSegment();
    }


    void Start()
    {
        transform.position = OFFSCREEN_POSITION;     // TODO

        if (upgradesTo != null)
        {
            upgradesTo = GameObject.Find(upgradesTo.name).GetComponent<Item>();
            upgradesTo.UpgradedFrom = this;
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

    public bool IsMaxedOut { get { return count == maxCount; } }


    public void OnCollected(int amount = 1)
    {
        count += amount;
        count = Mathf.Clamp(count, 0, maxCount);
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

        CastTriggerSpells();
    }


    void CastTriggerSpells()
    {
        foreach (Spell_Base sp in TriggerSpellsList)
        {
            if (sp == null) { continue; }
            sp.Cast(CommonObjects.Player_G);
        }
    }
}