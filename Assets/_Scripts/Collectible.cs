using UnityEngine;
using System.Collections;


public class Collectible : MonoBehaviour
{
    const float RiseAboveLinkDuration = 1.5f;
    const float RiseAboveLinkHeight = 0.5f;


    public Item itemPrefab;
    public int amount = 1;

    public AudioClip[] sounds;

    public bool riseUpWhenCollected;                     
    public bool isSpecialItem;                  // Special item instances won't respawn
    public bool appearsOnRoomClear;
    public bool destroyOnCollect = true;
    

    public Grotto Grotto { get; set; }
    public CollectibleSpawnPoint SpawnPoint { get; set; }
    public int price { get; set; }


    public void Start()
    {
        if (WorldInfo.Instance.IsInDungeon)
        {
            if (appearsOnRoomClear)
            {
                DungeonRoom dr = DungeonRoom.GetRoomForPosition(transform.position);
                //if (dr == null) { print(" ItemOnClear -> " + name + ": " + transform.position.x + ", " + transform.position.z); }
                dr.ItemOnClear = this;

                gameObject.SetActive(false);
            }

            if (isSpecialItem)
            {
                DungeonRoom dr = DungeonRoom.GetRoomForPosition(transform.position);
                //if (dr == null) { print(" SpecialItem -> " + name + ": " + transform.position.x + ", " + transform.position.z); }
                dr.SpecialItem = this;
            }
        }
    }


    void OnTriggerEnter(Collider otherCollider)
    {
        GameObject other = otherCollider.gameObject;
        //print("Collectible --> OnTriggerEnter: " + other.name);

        if (CommonObjects.IsPlayer(other))
        {
            Inventory inv = Inventory.Instance;
            if (inv.RupeeCount >= price)
            {
                inv.SpendRupees(price);
                Collect();
            }
        }
        else
        {
            if (price == 0)
            {
                Boomerang boomerang = other.GetComponent<Boomerang>();
                if (boomerang != null)
                {
                    boomerang.OnHitCollectible(this);
                }
            }
        }
        
    }

    public void Collect()
    {
        //print(" Collect: " + name);

        SoundFx sfx = SoundFx.Instance;
        foreach (var clip in sounds)
        {
            sfx.PlayOneShot(clip);
        }

        if (riseUpWhenCollected)
        {
            StartCoroutine("RiseAboveLink");
        }
        else
        {
            FinishCollectionProcess();
        }
    }

    IEnumerator RiseAboveLink()
    {
        CommonObjects.Player_C.ActivateParalyze(RiseAboveLinkDuration);

        iTween.MoveAdd(gameObject, new Vector3(0, RiseAboveLinkHeight, 0), RiseAboveLinkDuration);

        yield return new WaitForSeconds(RiseAboveLinkDuration);

        FinishCollectionProcess();
    }

    void FinishCollectionProcess()
    {
        Inventory.Instance.OnItemCollected(this, amount);

        if (WorldInfo.Instance.IsInDungeon)
        {
            DungeonRoom dr = DungeonRoom.GetRoomForPosition(transform.position);
            if (dr != null)
            {
                dr.OnItemCollectedWithinThisRoom(this);
            }
        }
        else if (WorldInfo.Instance.IsOverworld)
        {
            if (Grotto != null)
            {
                Grotto.OnGrottoItemCollected(this);
            }
            if (SpawnPoint != null)
            {
                SpawnPoint.OnItemCollected(this);
            }
        }

        if (isSpecialItem)
        {
            SaveManager.Instance.SaveGame();
        }

        if (destroyOnCollect)
        {
            Destroy(gameObject);
        }
    }

}
