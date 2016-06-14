#pragma warning disable 0649 // variable is never assigned to

using UnityEngine;
using System;
using System.Collections.Generic;

public class Projectile_Base : MonoBehaviour, IDamageDealerDelegate
{
    public Action<Projectile_Base> onProjectileWillBeDestroyed_Callback;

    [SerializeField]
    protected DamageDealer_Base _damageDealer;      // The DamageDealer component of this projectile


    public float speed;
    public float maxLifeTime = 3;
    public bool destroyOnCollision = true;

    public bool woodenShieldBlocks, magicShieldBlocks;  


    [SerializeField]
    string[] _drillsThrough;        // Names of entities that this projectile can hit and pass through without stopping
    List<string> _drillsThroughList;
    public bool DrillsThrough(string objName) { return _drillsThroughList.Contains(objName); }


    virtual protected void Awake()
    {
        _drillsThroughList = new List<string>(_drillsThrough);

        if(_damageDealer == null)
        {
            _damageDealer = GetComponent<DamageDealer_Base>();
        }
        if (_damageDealer != null)
        {
            _damageDealer.Delegate = this;
        }
    }

    virtual protected void Start()
    {
        Destroy(gameObject, maxLifeTime);
    }


    // This projectile's length in the direction it is intended to travel
    public float Length
    {
        get
        {
            CapsuleCollider cc = GetComponent<CapsuleCollider>();
            if (cc != null)
            {
                return cc.height;
            }

            SphereCollider sc = GetComponent<SphereCollider>();
            if (sc != null)
            {
                return sc.radius * 2;
            }

            return 0;
        }
    }
    public float Radius { get { return Length * 0.5f; } }

    public Vector3 MoveDirection { get; set; }

    public void PointInDirection(Vector3 direction)
    {
        if(direction == Vector3.zero)
        {
            return;
        }

        Vector3 euler = Quaternion.FromToRotation(Vector3.forward, direction).eulerAngles;
        transform.rotation = Quaternion.Euler(90, euler.y, 0);
    }


    virtual protected void Update()
    {
        transform.position += MoveDirection * speed * Time.deltaTime;
    }


    void OnCollisionEnter(Collision collision)
    {
        if (destroyOnCollision)
        {
            GameObject other = collision.gameObject;
            if (!DrillsThrough(other.name))
            {
                Destroy(gameObject);
            }
        }
    }


    #region IDamageDealerDelegate

    bool IDamageDealerDelegate.CanAttackBeBlocked(DamageDealer_Base attacker, HealthController_Abstract victim)
    {
        Actor r = victim.GetComponent<Actor>();
        if (r == null)
        {
            return false;
        }
        
        return r.CanBlockAttack(woodenShieldBlocks, magicShieldBlocks, MoveDirection);     // TODO: remove the 2 "shieldCanBlock" params
    }

    void IDamageDealerDelegate.OnAttackBlocked(DamageDealer_Base attacker, HealthController_Abstract blocker) { OnAttackDeflected(attacker, blocker); }
    void IDamageDealerDelegate.OnHitAnInvulnerable(DamageDealer_Base attacker, HealthController_Abstract invulnerable) { OnAttackDeflected(attacker, invulnerable); }

    void IDamageDealerDelegate.OnDamageDealt(DamageDealer_Base attacker, HealthController_Abstract victim, uint amount) { }
    void IDamageDealerDelegate.OnInstaKilled(DamageDealer_Base attacker, HealthController_Abstract victim) { }

    #endregion IDamageDealerDelegate


    void OnAttackDeflected(DamageDealer_Base attacker, HealthController_Abstract deflector)
    {
        Actor r = deflector.GetComponent<Actor>();
        if (r == null)
        {
            return;
        }

        if (r.playSoundWhenBlockingAttack)
        {
            PlayDeflectionSound();
        }
    }

    protected void PlayDeflectionSound()
    {
        SoundFx.Instance.PlayOneShot(SoundFx.Instance.shield);
    }


    virtual protected void OnDestroy()
    {
        if (onProjectileWillBeDestroyed_Callback != null)
        {
            onProjectileWillBeDestroyed_Callback(this);
        }
    }
}