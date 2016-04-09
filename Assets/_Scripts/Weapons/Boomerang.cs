using UnityEngine;
using System.Collections.Generic;


public class Boomerang : MonoBehaviour
{
    const float returnDistanceThreshold = 0.5f;

    public bool isEnemyBoomerang = false;
    public float speed;
    public int damage = 1;
    public float maxDistance = 5;
    public string[] kills;          // Enemies that can be killed by boomerang
    public string[] invulnerables;  // Enemies that cannot be stunned (or hurt) by boomerang 

    public AudioClip flySound;
    public Vector3 boomerangPositionOffset = new Vector3(0, 0.15f, 0);
    public bool woodenShieldBlocks, magicShieldBlocks;


    public bool CanUse { get { return !_isDeparting && !_isReturning; } }


    Vector3 _origin;
    Vector3 _direction;
    Transform _thrower;         // Where the boomerang will return to
    bool _isDeparting = false;
    bool _isReturning = false;

    Transform _projectilesContainer;
    List<string> _kills;
    List<string> _invulnerables;


    void Awake()
    {
        _projectilesContainer = GameObject.Find("Projectiles").transform;

        _kills = new List<string>(kills);
        _invulnerables = new List<string>(invulnerables);

        GetComponent<Collider>().enabled = false;
        GetComponent<Renderer>().enabled = false;

        transform.localRotation = Quaternion.Euler(90, 0, 0);
    }

    void Start()
    {
        transform.localPosition += boomerangPositionOffset;

        transform.up = transform.forward;
        Vector3 euler = transform.rotation.eulerAngles;
        euler.x = 90;
        transform.rotation = Quaternion.Euler(euler);
    }


    public void Throw(Transform thrower, Vector3 direction)
    {
        if(!CanUse) { return; }

        _thrower = thrower;
        _origin = _thrower.position + boomerangPositionOffset;
        _direction = direction;
        //_direction.y = 0;
        direction.Normalize();

        transform.parent = _projectilesContainer.parent;
        GetComponent<Collider>().enabled = true;
        GetComponent<Renderer>().enabled = true;
        _isDeparting = true;

        if (flySound != null)
        {
            GetComponent<AudioSource>().clip = flySound;
            GetComponent<AudioSource>().loop = true;
            GetComponent<AudioSource>().Play();
        }
    }

	void Update() 
    {
        if (_isDeparting)
        {
            float distSq = (_origin - transform.position).sqrMagnitude;
            if (distSq >= maxDistance * maxDistance)
            {
                ReturnToThrower();
            }
        }
        else if (_isReturning)
        {
            Vector3 toThrower = (_thrower.position + boomerangPositionOffset) - transform.position;
            if (toThrower.sqrMagnitude < returnDistanceThreshold * returnDistanceThreshold)
            {
                OnReturnedToThrower();
            }
            else
            {
                _direction = toThrower;
                _direction.Normalize();
            }
        }

        transform.position += _direction * speed * Time.deltaTime;
	}

    void CollectAttachedCollectibles()
    {
        foreach (Transform child in transform)
        {
            Collectible c = child.GetComponent<Collectible>();
            if (c != null)
            {
                c.Collect();
            }
        }
    }


    void OnCollisionEnter(Collision collision)
    {
        GameObject other = collision.gameObject;
        //print("Boomerang --> OnCollisionEnter: " + other.name);

        HealthController hc = other.GetComponent<HealthController>();
        if (hc == null)
        {
            hc = other.transform.parent.GetComponent<HealthController>();
        }
        if (hc != null)
        {
            OnHitActor(hc);
        }

        ReturnToThrower();
    }

    void OnHitActor(HealthController actorHealth)
    {
        if (isEnemyBoomerang)
        {
            Player player = actorHealth.GetComponent<Player>();
            if (player != null)
            {
                bool blocked = player.CanBlockAttack(woodenShieldBlocks, magicShieldBlocks, _direction);
                if (blocked)
                {
                    SoundFx sfx = SoundFx.Instance;
                    sfx.PlayOneShot(sfx.shield);
                }
                else
                {
                    actorHealth.TakeDamage((uint)damage, gameObject);
                }
            }
        }
        else
        {
            if (_kills.Contains(actorHealth.name))
            {
                actorHealth.Kill(gameObject);
            }
            else
            {
                Enemy enemy = actorHealth.GetComponent<Enemy>();
                if (enemy != null)
                {
                    bool blocked = enemy.isBoss || _invulnerables.Contains(actorHealth.name) || enemy.CanBlockAttack(_direction);
                    if (blocked)
                    {
                        if (enemy.playSoundWhenBlockingAttack)
                        {
                            SoundFx sfx = SoundFx.Instance;
                            sfx.PlayOneShot(sfx.shield);
                        }
                    }
                    else
                    {
                        enemy.Stun();
                    }
                }
            }
        }
    }

    public void OnHitCollectible(Collectible collectible)
    {
        if (isEnemyBoomerang) { return; }

        collectible.transform.parent = transform;
        //collectible.transform.localPosition = Vector3.zero;
        ReturnToThrower();
    }

    void ReturnToThrower()
    {
        _isDeparting = false;
        _isReturning = true;
    }

    void OnReturnedToThrower()
    {
        transform.parent = _thrower;
        _direction = Vector3.zero;
        GetComponent<Collider>().enabled = false;
        GetComponent<Renderer>().enabled = false;

        _isDeparting = false;
        _isReturning = false;

        CollectAttachedCollectibles();

        GetComponent<AudioSource>().Stop();
    }

}