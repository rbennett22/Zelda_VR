using UnityEngine;

public class Weapon_Melee_Boomerang : Weapon_Melee, IDamageDealerDelegate
{
    const float RETURN_DISTANCE_THRESHOLD = 0.5f;


    public float speed;
    public float maxDistance = 5;

    public AudioClip flySound;


    Vector3 _origin;
    Vector3 _direction;
    Transform _thrower;         // Where the boomerang will return to

    bool _isDeparting, _isReturning;
    override public bool IsAttacking { get { return base.IsAttacking || _isDeparting || _isReturning; } }
    override public bool CanAttack { get { return (base.CanAttack && !IsAttacking); } }

    [SerializeField]
    Renderer _renderer;
    protected bool RendererEnabled { get { return _renderer.enabled; } set { _renderer.enabled = value; } }

    AudioSource _audioSource;


    override protected void Awake()
    {
        base.Awake();

        _audioSource = GetComponent<AudioSource>();

        RendererEnabled = false;
    }

    public override void Attack(Vector3 direction)
    {
        if (!CanAttack)
        {
            return;
        }
        base.Attack(direction);

        Vector3 dir = (direction == Vector3.zero) ? transform.forward : direction;

        _thrower = transform.parent;
        _origin = _thrower.position;
        _direction = dir.normalized;

        transform.SetParent(null);

        RendererEnabled = true;
        CollisionEnabled = true;
        PlayFlySoundLoop();

        _isDeparting = true;
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
            Vector3 toThrower = _thrower.position - transform.position;
            if (toThrower.sqrMagnitude <= RETURN_DISTANCE_THRESHOLD * RETURN_DISTANCE_THRESHOLD)
            {
                OnReturnedToThrower();
            }
            else
            {
                _direction = toThrower.normalized;
            }
        }

        transform.position += _direction * speed * Time.deltaTime;
    }


    public override void OnHitCollectible(Collectible collectible)
    {
        base.OnHitCollectible(collectible);

        if (!canGatherCollectibles) { return; }

        ReturnToThrower();
    }

    void OnCollisionEnter(Collision collision)
    {
        ReturnToThrower();
    }

    void IDamageDealerDelegate.OnDamageDealt(DamageDealer_Base attacker, HealthController_Abstract victim, uint amount)
    {
        // TODO

        Enemy enemy = victim.GetComponent<Enemy>();
        if(enemy != null)
        {
            enemy.Stun();
        }
    }


    void ReturnToThrower()
    {
        _isDeparting = false;
        _isReturning = true;
    }

    void OnReturnedToThrower()
    {
        _isDeparting = _isReturning = false;

        transform.SetParent(_thrower);
        _direction = Vector3.zero;
        RendererEnabled = false;
        CollisionEnabled = false;

        StopFlySound();

        CollectAttachedCollectibles();
    }

    
    void PlayFlySoundLoop()
    {
        if(_audioSource == null || flySound == null)
        {
            return;
        }

        _audioSource.clip = flySound;
        _audioSource.loop = true;
        _audioSource.Play();
    }
    void StopFlySound()
    {
        if (_audioSource == null)
        {
            return;
        }

        _audioSource.Stop();
    }
}