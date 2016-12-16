using System.Collections;
using UnityEngine;
using Immersio.Utility;

public class Enemy : Actor
{
    const float RemovalDistanceThreshold = 32;      // How far away Enemy must be from player before it is destroyed (Overworld only)
    const float StunDuration = 3.0f;


    public static int EnemiesKilled { get; set; }       // TODO: Put this elsewhere
    public static int EnemiesKilledWithoutTakingDamage { get; set; }


    public EnemyAnimation enemyAnim;
    public void PauseAnimation()
    {
        if (enemyAnim != null)
        {
            enemyAnim.Pause();
        }
    }
    public void ResumeAnimation()
    {
        if (enemyAnim != null)
        {
            enemyAnim.Resume();
        }
    }


    public int meleeDamage = 1;
    public float jumpPower = 1;
    public float jumpUpFactor = 1.75f;
    public bool respondsToBait = true;
    public bool isBoss;
    public bool pushBackOnhit = true;
    

    public EnemySpawnPoint SpawnPoint { get; set; }
    public Index2 Sector { get; set; }                  // (Will be meaningless in dungeons)
    public DungeonRoom DungeonRoomRef { get; set; }     // (Will be null in overworld)

    public Player Player { get { return CommonObjects.Player_C; } }


    public bool IsAttacking { get { return HasWeapon && weapon.IsCooldownActive; } }
    public bool IsSpawning { get { return (enemyAnim != null && enemyAnim.IsSpawning); } }
    public bool IsJumping { get; private set; }
    public bool IsStunned { get; private set; }
    public bool IsParalyzed { get; private set; }

    public bool IsPreoccupied { get { return IsAttacking || IsJumping || IsSpawning || IsParalyzed || IsStunned; } }


    public void Jump(Vector3 direction)
    {
        direction.Normalize();
        Vector3 force = (direction + jumpUpFactor * Vector3.up) * jumpPower;
        GetComponent<Rigidbody>().AddForce(force);

        IsJumping = true;

        StartCoroutine("CheckForLanding");

        //print("Enemy::Jump: " + force);
    }

    public void Attack()
    {
        if (HasWeapon && weapon.CanAttack)
        {
            weapon.Attack();
        }
    }
    public void Attack(Vector3 direction)
    {
        if (HasWeapon && weapon.CanAttack)
        {
            weapon.Attack(direction);
        }
    }

    public void Stun()
    {
        if (IsStunned) { return; }
        if (!gameObject.activeSelf) { return; }

        StartCoroutine("StunCoroutine");
    }
    IEnumerator StunCoroutine()
    {
        SoundFx sfx = SoundFx.Instance;
        sfx.PlayOneShot3D(transform.position, sfx.enemyStun);

        PauseAnimation();
        IsStunned = true;

        SendMessage("OnStun", SendMessageOptions.DontRequireReceiver);

        yield return new WaitForSeconds(StunDuration);

        ResumeAnimation();
        IsStunned = false;
    }

    public void Paralyze(float duration, bool pauseAnim = true)
    {
        if (!gameObject.activeSelf) { return; }
        if(pauseAnim)
        {
            PauseAnimation();
        }
        StartCoroutine("ParalyzeCoroutine", duration);
    }
    IEnumerator ParalyzeCoroutine(float duration)
    {
        IsParalyzed = true;

        yield return new WaitForSeconds(duration);

        ResumeAnimation();
        IsParalyzed = false;
    }

    public void Push(Vector3 direction)
    {
        if (gameObject.activeSelf)
        {
            StopCoroutine("PushCoroutine");
            StartCoroutine("PushCoroutine", direction);
        }
    }
    IEnumerator PushCoroutine(Vector3 force)
    {
        Rigidbody rb = GetComponent<Rigidbody>();

        RigidbodyConstraints storedConstraints = rb.constraints;
        rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
        rb.AddForce(force, ForceMode.VelocityChange);

        SendMessage("DisableAI", SendMessageOptions.DontRequireReceiver);

        float duration = _healthController.tempInvincibilityDuration;
        yield return new WaitForSeconds(duration);

        rb.constraints = storedConstraints;

        SendMessage("EnableAI", SendMessageOptions.DontRequireReceiver);
    }


    public bool ShouldFollowBait()
    {
        bool followBait = false;
        if (respondsToBait && Bait.ActiveBait != null)
        {
            float distSq = (transform.position - Bait.ActiveBait.transform.position).sqrMagnitude;
            if (distSq < Bait.MaxLureDistanceSq)
            {
                followBait = true;
            }
        }
        return followBait;
    }


    float _prevY;
    IEnumerator CheckForLanding()
    {
        const float INTERVAL = 0.02f;

        do {
            _prevY = Position.y;
            yield return new WaitForSeconds(INTERVAL);
        } while (Position.y != _prevY);

        OnLanded();
    }

    void OnLanded()
    {
        //print("Enemy::OnLanded");
        IsJumping = false;
    }


    void OnTriggerEnter(Collider other)
    {
        if(CommonObjects.IsPlayer(other.gameObject))
        {
            OnHitPlayer();
        }
    }

    void OnHitPlayer()
    {
        if (IsStunned || IsParalyzed)
        {
            return;
        }

        Player.HealthController.TakeDamage((uint)meleeDamage, gameObject);
    }

    void OnEnemyDeath()
    {
        if (SpawnPoint != null)
        {
            SpawnPoint.OnSpawnedEnemyDeath();
        }
    }
}