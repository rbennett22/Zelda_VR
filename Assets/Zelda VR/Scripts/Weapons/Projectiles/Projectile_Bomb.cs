using UnityEngine;

[RequireComponent(typeof(BombExplosion))]

public class Projectile_Bomb : Projectile_Base
{
    [SerializeField]
    float _countdownDuration = 1;
    [SerializeField]
    BombExplosion _explosion;


    public DamageDealer_AOE DamageDealerAOE {
        get { return _damageDealer as DamageDealer_AOE; }
        set { _damageDealer = value; }
    }


    override protected void Awake()
    {
        base.Awake();

        _explosion = GetComponent<BombExplosion>();

        if (DamageDealerAOE == null)
        {
            DamageDealerAOE = GetComponent<DamageDealer_AOE>();
        }
        if (DamageDealerAOE != null)
        {
            DamageDealerAOE.Delegate = this;
            DamageDealerAOE.radius = Radius;
        }
    }

    override protected void Start()
    {
        base.Start();

        maxLifeTime = _countdownDuration + _explosion.duration + 0.1f;
        Invoke("Detonate", _countdownDuration);
    }


    void Detonate()
    {
        if (DamageDealerAOE != null)
        {
            DamageDealerAOE.AttackAllWithinReach();
        }

        _explosion.Detonate();
    }
}