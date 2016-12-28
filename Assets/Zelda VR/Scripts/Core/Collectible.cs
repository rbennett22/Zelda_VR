using System.Collections;
using UnityEngine;

public class Collectible : MonoBehaviour
{
    const float RISE_ABOVE_LINK_DURATION = 1.5f;
    const float RISE_ABOVE_LINK_HEIGHT = 0.5f;


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

    public bool MustBePurchased { get { return price > 0; } }


    Light _light;
    Renderer _renderer;


    void Awake()
    {
        _light = GetComponentInChildren<Light>();
        _renderer = GetComponentInChildren<Renderer>();
    }

    void Start()
    {
        if (WorldInfo.Instance.IsInDungeon)
        {
            DungeonRoom dr = DungeonRoom.GetRoomForPosition(transform.position);

            if (appearsOnRoomClear)
            {
                dr.ItemOnClear = this;

                gameObject.SetActive(false);
            }

            if (isSpecialItem)
            {
                dr.SpecialItem = this;
            }
        }
    }


    bool _isCollectible;
    public bool IsCollectible
    {
        get { return _isCollectible; }
        set
        {
            _isCollectible = value;

            _light.enabled = _isCollectible;
            _renderer.enabled = _isCollectible;
        }
    }


    void OnTriggerEnter(Collider otherCollider)
    {
        GameObject other = otherCollider.gameObject;
        //print("Collectible --> OnTriggerEnter: " + other.name);

        if (CommonObjects.IsPlayer(other))
        {
            if (!MustBePurchased || Inventory.Instance.TrySpendRupees(price))
            {
                Collect();
            }
        }
        else
        {
            if (!MustBePurchased)
            {
                Weapon_Melee w = other.GetComponent<Weapon_Melee>();
                if (w != null)
                {
                    w.OnHitCollectible(this);
                }
            }
        }
    }

    public void Collect()
    {
        //print(" Collect: " + name);

        PlayCollectionSounds();

        if (riseUpWhenCollected)
        {
            StartCoroutine("RiseAboveLink");
        }
        else
        {
            FinishCollectionProcess();
        }
    }

    void PlayCollectionSounds()
    {
        SoundFx sfx = SoundFx.Instance;
        foreach (var clip in sounds)
        {
            sfx.PlayOneShot(clip);
        }
    }

    IEnumerator RiseAboveLink()
    {
        CommonObjects.Player_C.ActivateParalyze(RISE_ABOVE_LINK_DURATION);

        PlayRisingTween();

        yield return new WaitForSeconds(RISE_ABOVE_LINK_DURATION);

        FinishCollectionProcess();
    }
    public void PlayRisingTween()
    {
        Vector3 vec = RISE_ABOVE_LINK_HEIGHT * Vector3.up;
        iTween.MoveAdd(gameObject, vec, RISE_ABOVE_LINK_DURATION);
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