using UnityEngine;

public class Weapon_Melee_Sword : Weapon_Melee
{
    const iTween.EaseType STAB_TWEEN_EASE_TYPE = iTween.EaseType.linear;


    [SerializeField]
    Vector3 _attackDirection_Local = Vector3.up;
    

    [SerializeField]
    float _reach = 1;       // How far sword can extend when attacking
    [SerializeField]
    float _speed = 12;


    override public ControlStyleEnum ControlStyle
    {
        set
        {
            base.ControlStyle = value;

            switch (_controlStyle)
            {
                case ControlStyleEnum.Classic:
                    SetCollidesWithEnvironment(true);
                    break;
                case ControlStyleEnum.VR:
                    SetCollidesWithEnvironment(false);
                    break;
                default: break;
            }
        }
    }
    void SetCollidesWithEnvironment(bool value)
    {
        int swordLayer = LayerMask.NameToLayer("Sword");
        int blocksLayer = LayerMask.NameToLayer("Blocks");
        int wallLayer = LayerMask.NameToLayer("Walls");
        int groundLayer = LayerMask.NameToLayer("Ground");

        Physics.IgnoreLayerCollision(swordLayer, blocksLayer, !value);
        Physics.IgnoreLayerCollision(swordLayer, wallLayer, !value);
        Physics.IgnoreLayerCollision(swordLayer, groundLayer, !value);
    }


    bool _isExtending, _isRetracting;
    override public bool IsAttacking { get { return base.IsAttacking || _isExtending || _isRetracting; } }
    override public bool CanAttack { get { return base.CanAttack && !IsAttacking; } }

    override public Vector3 AttackDirection { get { return transform.TransformDirection(_attackDirection_Local); } }


    Weapon_Gun _swordGun;
    public bool ProjectilesEnabled { get; set; }

    bool _collisionIsAllowedWhenRetracted;
    public bool CollisionIsAllowedWhenRetracted {
        get { return _collisionIsAllowedWhenRetracted; }
        set {
            _collisionIsAllowedWhenRetracted = value;

            if (!IsAttacking)
            {
                CollisionEnabled = _collisionIsAllowedWhenRetracted;
            }
        }
    }


    override protected void Awake()
    {
        base.Awake();

        _swordGun = GetComponent<Weapon_Gun>();

        CollisionIsAllowedWhenRetracted = _collisionIsAllowedWhenRetracted;
    }


    override public void Attack()
    {
        if(_controlStyle == ControlStyleEnum.VR)
        {
            return;
        }

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

        Vector3 targetPosLocal = transform.localPosition + _attackDirection_Local * _reach;

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

        _swordGun.Attack(AttackDirection);
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

        CollisionEnabled = _collisionIsAllowedWhenRetracted;

        Rigidbody rb = GetComponent<Rigidbody>();
        rb.velocity = Vector3.zero;

        transform.localRotation = _origRotLocal;

        CollectAttachedCollectibles();
    }


    public override void OnHitCollectible(Collectible collectible)
    {
        base.OnHitCollectible(collectible);

        if (!canGatherCollectibles) { return; }

        Retract();
    }


    void OnCollisionEnter(Collision collision)
    {
        Retract();
    }
}