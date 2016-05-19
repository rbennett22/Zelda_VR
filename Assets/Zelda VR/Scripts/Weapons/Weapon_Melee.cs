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
    }


    #region IDamageDealerDelegate

    bool IDamageDealerDelegate.CanAttackBeBlocked(DamageDealer_Base attacker, HealthController_Abstract victim)
    {
        // TODO

        Player player = victim.GetComponent<Player>();
        if (player != null)
        {
            return player.CanBlockAttack(woodenShieldBlocks, magicShieldBlocks, transform.up);     // TODO: remove the 2 "shieldCanBlock" params
        }

        Enemy enemy = victim.GetComponent<Enemy>();
        if (enemy != null)
        {
            return enemy.CanBlockAttack(transform.up);
        }

        return false;
    }

    void IDamageDealerDelegate.OnAttackBlocked(DamageDealer_Base attacker, HealthController_Abstract blocker) { OnAttackDeflected(attacker, blocker); }
    void IDamageDealerDelegate.OnHitAnInvulnerable(DamageDealer_Base attacker, HealthController_Abstract invulnerable) { OnAttackDeflected(attacker, invulnerable); }
    void OnAttackDeflected(DamageDealer_Base attacker, HealthController_Abstract invulnerable)
    {
        // TODO

        Player player = invulnerable.GetComponent<Player>();
        if (player != null)
        {
            PlayDeflectionSound();
            return;
        }

        Enemy enemy = invulnerable.GetComponent<Enemy>();
        if (enemy != null)
        {
            if (enemy.playSoundWhenBlockingAttack)
            {
                PlayDeflectionSound();
            }
            return;
        }
    }

    void IDamageDealerDelegate.OnDamageDealt(DamageDealer_Base attacker, HealthController_Abstract victim, uint amount) { }
    void IDamageDealerDelegate.OnInstaKilled(DamageDealer_Base attacker, HealthController_Abstract victim) { }

    #endregion IDamageDealerDelegate


    public void OnHitCollectible(Collectible collectible)
    {
        if (!canGatherCollectibles) { return; }

        collectible.transform.parent = transform;
    }

    protected void CollectAttachedCollectibles()
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


    protected void PlayDeflectionSound()
    {
        SoundFx.Instance.PlayOneShot(SoundFx.Instance.shield);
    }
}