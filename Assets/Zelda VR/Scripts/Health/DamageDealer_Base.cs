using UnityEngine;
using System.Collections.Generic;

public interface IDamageDealerDelegate
{
    //bool CanAttackBeDodged(DamageDealer_Base attacker, HealthController_Abstract victim);
    bool CanAttackBeBlocked(DamageDealer_Base attacker, HealthController_Abstract victim);
    void OnAttackBlocked(DamageDealer_Base attacker, HealthController_Abstract blocker);
    void OnHitAnInvulnerable(DamageDealer_Base attacker, HealthController_Abstract invulnerable);
    void OnDamageDealt(DamageDealer_Base attacker, HealthController_Abstract victim, uint amount);
    void OnInstaKilled(DamageDealer_Base attacker, HealthController_Abstract victim);
}

public class DamageDealer_Base : MonoBehaviour
{
    public IDamageDealerDelegate Delegate { get; set; }


    public int damage;
    public bool instaKills;     // If true, this DamageDealer instantly kills entities regardless of their health

    [SerializeField]            // Names of entities that cannot be hurt by DamageDealer
    string[] invulnerables;
    List<string> _invulnerables;      
    public bool CanHarm(string objName) { return !_invulnerables.Contains(objName); }

    [SerializeField]
    string[] kills;             // Names of entities that can be killed in one hit by this DamageDealer 
    List<string> _kills;
    public bool Kills(string objName) { return _kills.Contains(objName); }


    virtual protected void Awake()
    {
        _invulnerables = new List<string>(invulnerables);
        _kills = new List<string>(kills);
    }


    protected void Attack(HealthController_Abstract victim)
    {
        if (victim == null)
        {
            return;
        }

        if (!CanHarm(victim.name))
        {
            OnHitAnInvulnerable(victim);
            return;
        }
        if (CanAttackBeBlocked(victim))
        {
            OnAttackBlocked(victim);
            return;
        }

        if (instaKills || Kills(victim.name))
        {
            InstaKill(victim);
        }
        else
        {
            DealDamage(victim, (uint)damage);
        }
    }

    protected void DealDamage(HealthController_Abstract victim, uint amount)
    {
        if (victim == null)
        {
            return;
        }

        victim.TakeDamage(amount, gameObject);      // TODO
        OnDamageDealt(victim, amount);
    }

    protected void InstaKill(HealthController_Abstract victim)
    {
        if (victim == null)
        {
            return;
        }

        victim.Kill(gameObject);      // TODO
        OnInstaKilled(victim);
    }


    #region Delegated Methods
    
    /*protected bool CanAttackBeDodged(HealthController_Abstract victim)
    {
        if (Delegate == null)
            return false;
        return Delegate.CanAttackBeDodged(this, victim);
    }*/
    protected bool CanAttackBeBlocked(HealthController_Abstract victim)
    {
        if (Delegate == null)
            return false;
        return Delegate.CanAttackBeBlocked(this, victim);
    }
    protected void OnAttackBlocked(HealthController_Abstract blocker)
    {
        if (Delegate == null)
            return;
        Delegate.OnAttackBlocked(this, blocker);
    }
    protected void OnHitAnInvulnerable(HealthController_Abstract invulnerable)
    {
        if (Delegate == null)
            return;
        Delegate.OnHitAnInvulnerable(this, invulnerable);
    }
    protected void OnDamageDealt(HealthController_Abstract victim, uint amount)
    {
        if (Delegate == null)
            return;
        Delegate.OnDamageDealt(this, victim, amount);
    }
    protected void OnInstaKilled(HealthController_Abstract victim)
    {
        if (Delegate == null)
            return;
        Delegate.OnInstaKilled(this, victim);
    }

    #endregion Delegated Methods


    protected static HealthController_Abstract TryGetHealthController(GameObject g)
    {
        HealthController_Abstract hc = g.GetComponent<HealthController_Abstract>();
        if (hc == null)
        {
            Transform otherParent = g.transform.parent;
            if (otherParent != null)
            {
                hc = otherParent.GetComponent<HealthController>();
            }
        }
        return hc;
    }
}