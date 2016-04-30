using UnityEngine;
using System.Collections;


public class EnemyItemDrop : MonoBehaviour 
{
    const float ChanceToDropSomething = 0.5f;
    const float DropHeight = 0.25f;


    public enum ItemDropGroup
    {
        A, B, C, D, NoDrop
    }
    public ItemDropGroup dropGroup = ItemDropGroup.NoDrop;

    public Collectible specialDrop = null;      // This item will be dropped with %100 certainty if its not null


    Collectible[,] _itemDropChart;


    void Awake()
    {
        InitItemDropChart();
    }

    void InitItemDropChart()
    {
        EnemyDroppedCollectibles c = EnemyDroppedCollectibles.Instance;

        _itemDropChart = new Collectible[10, 4] {
            { c.rupeeYellow, c.bomb,        c.rupeeYellow,  c.heart },
            { c.heart,       c.rupeeYellow, c.heart,        c.fairy },
            { c.rupeeYellow, c.clock,       c.rupeeYellow,  c.rupeeYellow },
            { c.fairy,       c.rupeeYellow, c.rupeeBlue,    c.heart },
            { c.rupeeYellow, c.heart,       c.heart,        c.fairy },

            { c.heart,       c.bomb,        c.clock,        c.heart },
            { c.heart,       c.rupeeYellow, c.rupeeYellow,  c.heart },
            { c.rupeeYellow, c.bomb,        c.rupeeYellow,  c.heart },
            { c.rupeeYellow, c.heart,       c.rupeeYellow,  c.rupeeYellow },
            { c.heart,       c.heart,       c.rupeeBlue,    c.heart }
        };
    }

    public void DropRandomItem()
    {
        Collectible forceDroppedItem = Cheats.Instance.forceDroppedItem;
        if (forceDroppedItem != null)
        {
            DropItemFromPrefab(forceDroppedItem.gameObject);
            return;
        }

        if (specialDrop != null)
        {
            DropItem(specialDrop);
            return;
        }

        if (dropGroup == ItemDropGroup.NoDrop)
        {
            return;
        }

        float r = Random.Range(0.0f, 1.0f);
        if (r < ChanceToDropSomething)
        {
            int col = (int)dropGroup;
            int row = Enemy.EnemiesKilled % 10;
            if (Enemy.EnemiesKilledWithoutTakingDamage >= 10)
            {
                row = Enemy.EnemiesKilledWithoutTakingDamage % 10;
            }
            DropItemFromPrefab(_itemDropChart[row, col].gameObject);
        }
    }

    void DropItemFromPrefab(GameObject prefabItem)
    {
        GameObject g = GameObject.Instantiate(prefabItem) as GameObject;
        DropItem(g.GetComponent<Collectible>());
    }

    void DropItem(Collectible item)
    {
        //print(" DropItem: " + item.name);

        GameObject g = item.gameObject;
        g.transform.parent = EnemyDroppedCollectibles.Instance.transform;
        g.transform.position = transform.position;
        g.transform.SetLocalY(DropHeight);
    }
	
}
