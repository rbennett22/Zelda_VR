using UnityEngine;
using System.Collections.Generic;

public class Weapon_Melee : Weapon_Base, IDamageDealerDelegate
{
    public bool canGatherCollectibles;
    public bool woodenShieldBlocks, magicShieldBlocks;      // TODO
    //public bool appliesPushbackForce = true;              // TODO


    [SerializeField]
    DamageDealer_Base[] _damageDealers;
    protected List<DamageDealer_Base> _damageDealersList = new List<DamageDealer_Base>();


    Collider _collider;
    protected bool CollisionEnabled { get { return _collider.enabled; } set { _collider.enabled = value; } }

    protected Vector3 _originLocal;
    protected Quaternion _origRotLocal;


    virtual public Vector3 AttackDirection { get { return transform.up; } }


    virtual protected void Awake()
    {
        _damageDealersList = new List<DamageDealer_Base>(_damageDealers);
        foreach (DamageDealer_Base dd in _damageDealersList)
        {
            dd.Delegate = this;
        }

        _collider = GetComponent<Collider>();
        CollisionEnabled = false;
    }

    virtual protected void Start()
    {
        _originLocal = transform.localPosition;
        _origRotLocal = transform.localRotation;
    }


    public virtual void OnHitCollectible(Collectible collectible)
    {
        if (!canGatherCollectibles) { return; }

        collectible.transform.parent = transform;
    }

    protected void CollectAttachedCollectibles()
    {
        foreach (Collectible c in GetComponentsInChildren<Collectible>())
        {
            c.Collect();
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

        return r.CanBlockAttack(woodenShieldBlocks, magicShieldBlocks, AttackDirection);     // TODO: remove the 2 "shieldCanBlock" params
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
}