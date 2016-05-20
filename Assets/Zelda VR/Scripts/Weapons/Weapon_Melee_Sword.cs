using UnityEngine;

public class Weapon_Melee_Sword : Weapon_Melee
{
    const iTween.EaseType STAB_TWEEN_EASE_TYPE = iTween.EaseType.linear;


    [SerializeField]
    Vector3 _stabDirection_Local = Vector3.up;
    Vector3 StabDirection_World { get { return transform.TransformDirection(_stabDirection_Local); } }

    [SerializeField]
    float _reach = 1;       // How far sword can extend when attacking
    [SerializeField]
    float _speed = 12;


    bool _isExtending, _isRetracting;
    override public bool IsAttacking { get { return base.IsAttacking || _isExtending || _isRetracting; } }
    override public bool CanAttack { get { return (base.CanAttack && !IsAttacking); } }

    Weapon_Gun _swordGun;
    public bool ProjectilesEnabled { get; set; }


    override protected void Awake()
    {
        base.Awake();

        _swordGun = GetComponent<Weapon_Gun>();
    }


    override public void Attack()
    {
        if (!CanAttack)
        {
            return;
        }
        base.Attack();

        Extend();
    }

    void Extend()
    {
        if(_isExtending)
        {
            return;
        }

        Vector3 targetPosLocal = transform.localPosition + _stabDirection_Local * _reach;

        iTween.MoveTo(gameObject, iTween.Hash(
            "islocal", true,
            "position", targetPosLocal,
            "speed", _speed,
            "easetype", STAB_TWEEN_EASE_TYPE,
            "oncomplete", "OnExtendedCompletely")
        );

        CollisionEnabled = true;

        _isExtending = true;
        _isRetracting = false;
    }
    void OnExtendedCompletely()
    {
        _isExtending = false;

        if (ProjectilesEnabled)
        {
            FireProjectile();
        }

        Retract();
    }

    void FireProjectile()
    {
        if(_swordGun == null || !_swordGun.CanAttack)
        {
            return;
        }

        _swordGun.Attack(StabDirection_World);
    }

    void Retract()
    {
        if (_isRetracting)
        {
            return;
        }

        iTween.MoveTo(gameObject, iTween.Hash(
            "islocal", true,
            "position", _originLocal,
            "speed", _speed,
            "easetype", STAB_TWEEN_EASE_TYPE,
            "oncomplete", "OnRetractedCompletely")
        );

        _isExtending = false;
        _isRetracting = true;
    }
    void OnRetractedCompletely()
    {
        _isExtending = _isRetracting = false;

        CollisionEnabled = false;
    }


    void OnCollisionEnter(Collision collision)
    {
        Retract();
    }
}