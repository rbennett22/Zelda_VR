using UnityEngine;

public class EnemyAI_Gannon : EnemyAI
{
    const float VISIBILE_DURATION = 1.5f;
    const float BOUNDS_WIDTH = 8;           // TODO: Define custom bounds through EnemySpawnPoint
    const float BOUNDS_HEIGHT = 5;


    public int swordHitsNeededToKill = 4;
    public float attackCooldown = 1.0f;


    int _swordHitsTaken;
    FlashColorsSimple _flash;


    public bool Visible
    {
        get { return AnimatorInstance.GetComponent<Renderer>().enabled; }
        private set { AnimatorInstance.GetComponent<Renderer>().enabled = value; }
    }

    public bool VulnerableToArrow { get { return _swordHitsTaken >= swordHitsNeededToKill; } }


    public void FreezeForIntro()
    {
        Visible = true;
    }
    public void UnfreezeForIntro()
    {
        Disappear();
    }

    public void FlashBeforeDeath()
    {
        if(_flash != null)
        {
            Destroy(_flash);
        }
        _flash = gameObject.AddComponent<FlashColorsSimple>();

    }
    public void ExplodeIntoPieces()
    {
        Destroy(_flash);

        // TODO: Gannon explodes for 1 sec, creating dust that falls into a pile, and sending out 8 red "sword hits" 
    }


    void Start()
    {
        InitBounds();
        Disappear();

        InvokeRepeating("Tick", 0, attackCooldown);
    }

    void InitBounds()
    {
        Vector3 center = _enemy.DungeonRoomRef.Center;
        Boundary = new Rect(
            center.x - BOUNDS_WIDTH * 0.5f,
            center.z - BOUNDS_HEIGHT * 0.5f,
            BOUNDS_WIDTH, BOUNDS_HEIGHT
            );
    }


    void Tick()
    {
        if (!_doUpdate) { return; }
        if (IsPreoccupied) { return; }

        if (!Visible)
        {
            Attack();
            MoveToRandomLocation();
        }
    }

    void Attack()
    {
        _enemy.Attack(DirectionToPlayer);
    }

    void MoveToRandomLocation()
    {
        Vector3 p = GetRandomAllowedPositionInsideBoundary();
        _enemy.PositionXZ = new Vector2(p.x, p.z);
    }


    void OnHitWithSword(Weapon_Melee_Sword sword)
    {
        if (Visible) { return; }

        _swordHitsTaken++;

        UpdatePose();
        Appear();
    }

    void OnHitWithSilverArrow()
    {
        if (!Visible) { return; }

        if (VulnerableToArrow)
        {
            _healthController.Kill(gameObject, true);
        }
    }


    void UpdatePose()
    {
        string animName = VulnerableToArrow ? "NextHurtPose" : "NextPose";
        AnimatorInstance.SetTrigger(animName);
    }

    void Appear()
    {
        Visible = true;
        Invoke("Disappear", VISIBILE_DURATION);
    }
    void Disappear()
    {
        Visible = false;
        MoveToRandomLocation();
    }
}