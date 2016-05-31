using System.Collections;
using UnityEngine;

public class Enemy : Actor
{
    const float RemovalDistanceThreshold = 32;      // How far away Enemy must be from player before it is destroyed (Overworld only)
    const float StunDuration = 3.0f;


    public static int EnemiesKilled { get; set; }       // TODO: Put this elsewhere
    public static int EnemiesKilledWithoutTakingDamage { get; set; }


    public EnemyAnimation enemyAnim;
    public void PauseAnimation()
    {
        enemyAnim.Pause();
    }
    public void ResumeAnimation()
    {
        enemyAnim.Resume();
    }


    public int meleeDamage = 1;
    public float jumpPower = 1;
    public float jumpUpFactor = 1.75f;
    public bool respondsToBait = true;
    public bool isBoss;
    public bool pushBackOnhit = true;
    


    public EnemySpawnPoint SpawnPoint { get; set; }
    public DungeonRoom DungeonRoomRef { get; set; }    // (Will be null in overworld)

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
        direction += new Vector3(0, jumpUpFactor, 0);
        Vector3 force = direction * jumpPower;
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

        SendMessage("OnStun", SendMessageOptions.DontRequireReceiver);
    }
    IEnumerator StunCoroutine()
    {
        SoundFx sfx = SoundFx.Instance;
        sfx.PlayOneShot3D(transform.position, sfx.enemyStun);

        PauseAnimation();
        IsStunned = true;

        yield return new WaitForSeconds(StunDuration);

        ResumeAnimation();
        IsStunned = false;
    }

    public void Paralyze(float duration)
    {
        if (!gameObject.activeSelf) { return; }
        StartCoroutine("ParalyzeCoroutine", duration);
    }
    IEnumerator ParalyzeCoroutine(float duration)
    {
        PauseAnimation();
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
        do
        {
            _prevY = transform.position.y;
            yield return new WaitForSeconds(0.02f);
        } while (transform.position.y != _prevY);

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
            OnHitPlayer(Player);
        }
    }

    void OnHitPlayer(Player player)
    {
        player.HealthController.TakeDamage((uint)meleeDamage, gameObject);
    }

    void OnEnemyDeath()
    {
        if (SpawnPoint != null)
        {
            SpawnPoint.OnSpawnedEnemyDeath();
        }
    }
}