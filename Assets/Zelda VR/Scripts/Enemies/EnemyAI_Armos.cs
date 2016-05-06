using UnityEngine;
using System.Collections;

public class EnemyAI_Armos : MonoBehaviour 
{
    const float ProximityThreshold = 1.5f;
    const float ProximityThresholdSq = ProximityThreshold * ProximityThreshold;
    const float SpeedSlow = 1.5f;
    const float SpeedFast = 3.5f;


    public enum StatueType
    {
        Red,
        Green,
        White
    }
    StatueType _type;
    public StatueType Type {
        get { return _type; }
        set {
            _type = value;
            switch (_type)
            {
                case StatueType.Red: _statue = redStatue; break;
                case StatueType.Green: _statue = greenStatue; break;
                case StatueType.White: _statue = whiteStatue; break;
            }
            if(_isInStatueMode)
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

    public bool StatueActive {
        get { return (_statue == null) ? false : _statue.activeSelf; }
        set { if (_statue != null) { _statue.SetActive(value); } } }

    void OnTriggerEnter(Collider otherCollider)
    {
        if (_isInStatueMode)
        {
            if (otherCollider.gameObject == CommonObjects.PlayerController_G)
            {
                StartCoroutine("ComeAlive");
            }
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

        yield return new WaitForSeconds(1.0f);

        if (linkedTilesWereRemoved)
        {
            SoundFx.Instance.PlayOneShot(SoundFx.Instance.secret);
        }

        if (HidesCollectibleItem)
        {
            ActivateHiddenCollectible();
        }

        GetComponent<EnemyMove>().Speed = Extensions.FlipCoin() ? SpeedFast : SpeedSlow;
        GetComponent<Enemy>().meleeDamage = _meleeDamage;
        _enemyMove.enabled = true;
    }

    void ActivateHiddenCollectible()
    {
        GameObject hiddenCollectible = null;

        Transform collectiblesContainer = GameObject.Find("Special Collectibles").transform;
        foreach (Transform child in collectiblesContainer)
        {
            Vector3 toCollectible = child.position - transform.position;
            float distanceToCollectibleSqr = Vector3.SqrMagnitude(toCollectible);
            if (distanceToCollectibleSqr < 1)
            {
                hiddenCollectible = child.gameObject;
            }
        }

        if (hiddenCollectible != null)
        {
            hiddenCollectible.SetActive(true);
            SoundFx.Instance.PlayOneShot(SoundFx.Instance.secret);
        }
    }

    void DestroyLinkedTiles()
    {
        for (int i = 0; i < linkedTiles.Length; i++)
        {
            Destroy(linkedTiles[i]);
        }
    }

}