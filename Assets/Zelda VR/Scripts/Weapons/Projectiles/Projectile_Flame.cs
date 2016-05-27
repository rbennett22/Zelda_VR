using UnityEngine;

public class Projectile_Flame : Projectile_Base 
{
    const float ATTACK_RATE = 0.5f;


    [SerializeField]
    BurningFlame _burningFlame;


    public DamageDealer_AOE DamageDealerAOE
    {
        get { return _damageDealer as DamageDealer_AOE; }
        set { _damageDealer = value; }
    }


    override protected void Awake()
    {
        base.Awake();

        _burningFlame.radius = Radius;

        if (DamageDealerAOE != null)
        {
            DamageDealerAOE.radius = Radius;
        }
    }
    override protected void Start()
    {
        base.Start();

        InvokeRepeating("Tick", 0.01f, ATTACK_RATE);

        Destroy(gameObject, maxLifeTime);
    }


    void Tick()
    {
        if (DamageDealerAOE != null)
        {
            DamageDealerAOE.AttackAllWithinReach();
        }
    }
}