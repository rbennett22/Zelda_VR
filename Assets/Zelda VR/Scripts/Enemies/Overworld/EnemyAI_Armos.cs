using System.Collections;
using UnityEngine;

public class EnemyAI_Armos : MonoBehaviour
{
    const float PROXIMITY_THRESHOLD = 1.5f;
    const float PROXIMITY_THRESHOLD_SQ = PROXIMITY_THRESHOLD * PROXIMITY_THRESHOLD;
    const float SPEED_SLOW = 1.5f;
    const float SPEED_FAST = 3.5f;
    const float FLASH_DURATION = 1.0f;


    public enum StatueType
    {
        Red,
        Green,
        White
    }
    StatueType _type;
    public StatueType Type
    {
        get { return _type; }
        set
        {
            _type = value;
            switch (_type)
            {
                case StatueType.Red: _statue = redStatue; break;
                case StatueType.Green: _statue = greenStatue; break;
                case StatueType.White: _statue = whiteStatue; break;
            }
            if (_isInStatueMode)
            {
                StatueActive = true;
            }
        }
    }


    public Animator animator;
    public GameObject redStatue, greenStatue, whiteStatue;
    public GameObject[] linkedTiles;


    bool _isInStatueMode = true;
    EnemyMove _enemyMove;
    GameObject _statue;
    int _meleeDamage;


    public bool HidesCollectibleItem { get; set; }


    void Awake()
    {
        _enemyMove = GetComponent<EnemyMove>();
        _enemyMove.enabled = false;

        animator.gameObject.SetActive(false);
        GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;

        Enemy enemy = GetComponent<Enemy>();
        _meleeDamage = enemy.meleeDamage;
        enemy.meleeDamage = 0;

        redStatue.SetActive(false);
        greenStatue.SetActive(false);
        whiteStatue.SetActive(false);
    }

    void Start()
    {
        StatueActive = true;
    }

    public bool StatueActive
    {
        get { return (_statue == null) ? false : _statue.activeSelf; }
        set { if (_statue != null) { _statue.SetActive(value); } }
    }

    void OnTriggerEnter(Collider otherCollider)
    {
        if (!CommonObjects.IsPlayer(otherCollider.gameObject))
        {
            return;
        }

        if (_isInStatueMode)
        {
            StartCoroutine("ComeAlive");
        }
    }

    IEnumerator ComeAlive()
    {
        _isInStatueMode = false;
        GetComponent<HealthController>().isIndestructible = false;

        StatueActive = false;
        animator.gameObject.SetActive(true);
        GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;

        bool linkedTilesWereRemoved = false;
        if (linkedTiles.Length > 0)
        {
            DestroyLinkedTiles();
            linkedTilesWereRemoved = true;
        }

        StartFlashingColors();

        yield return new WaitForSeconds(FLASH_DURATION);

        StopFlashingColors();

        if (linkedTilesWereRemoved)
        {
            PlaySecretSound();
        }

        if (HidesCollectibleItem)
        {
            ActivateHiddenCollectible();
        }

        GetComponent<EnemyMove>().speed = Extensions.FlipCoin() ? SPEED_FAST : SPEED_SLOW;
        GetComponent<Enemy>().meleeDamage = _meleeDamage;
        _enemyMove.enabled = true;
    }

    void ActivateHiddenCollectible()
    {
        Collectible hiddenCollectible = null;   // TODO: What if multiple Collectibles?

        Transform collectiblesContainer = GameObject.Find("Special Collectibles").transform;
        foreach (Transform child in collectiblesContainer)
        {
            Vector3 toCollectible = child.position - transform.position;
            toCollectible.y = 0;
            float distanceToCollectibleSq = Vector3.SqrMagnitude(toCollectible);
            if (distanceToCollectibleSq < 1)
            {
                hiddenCollectible = child.GetComponent<Collectible>();
                if (hiddenCollectible != null)
                {
                    RevealCollectible(hiddenCollectible);
                    break;
                }
            }
        }
    }

    void RevealCollectible(Collectible c)
    {
        if (c == null || c.IsCollectible)    // Already been revealed?
        {
            return;
        }

        c.IsCollectible = true;
        PlaySecretSound();
    }

    void PlaySecretSound()
    {
        SoundFx.Instance.PlayOneShot(SoundFx.Instance.secret);
    }

    void DestroyLinkedTiles()
    {
        for (int i = 0; i < linkedTiles.Length; i++)
        {
            Destroy(linkedTiles[i]);
        }
    }


    FlashColors _flash;
    public void StartFlashingColors()
    {
        if (_flash != null)
        {
            return;
        }
        _flash = gameObject.AddComponent<FlashColors>();
    }
    public void StopFlashingColors()
    {
        Destroy(_flash);
        _flash = null;
    }
}