using System.Collections;
using UnityEngine;


public class Enemy : MonoBehaviour
{
    const int BoundaryWidth = 14;
    const int BoundaryHeight = 9;
    const float RemovalDistanceThreshold = 32;      // How far away Enemy must be from player before it is destroyed (Overworld only)
    const float StunDuration = 3.0f;


    public static int EnemiesKilled { get; set; }
    public static int EnemiesKilledWithoutTakingDamage { get; set; }


    public EnemyAnimation enemyAnim;

    public Weapon_Base weapon;
    public bool HasWeapon { get { return weapon != null; } }

    public Shield_Base shield;
    public bool HasShield { get { return shield != null; } }

    public int meleeDamage = 1;
    public float speed = 1;
    public float jumpPower = 1;
    public float jumpUpFactor = 1.75f;
    public bool respondsToBait = true;
    public bool isBoss;
    public bool pushBackOnhit = true;
    public bool playSoundWhenBlockingAttack = true;


    public EnemySpawnPoint SpawnPoint { get; set; }
    public DungeonRoom DungeonRoomRef { get; set; }    // (Will be null in overworld)
    public GameObject PlayerController { get { return CommonObjects.PlayerController_G; } }
  
    public bool IsAttacking { get { return HasWeapon && weapon.IsCooldownActive; } }
    public bool IsSpawning { get { return (enemyAnim != null && enemyAnim.IsSpawning); } }

    public bool IsJumping { get; private set; }
    public bool IsStunned { get; private set; }
    public bool IsParalyzed { get; private set; }

    public bool IsPreoccupied { get { return IsAttacking || IsJumping || IsSpawning || IsParalyzed || IsStunned; } }

    public int TileX { get { return (int)transform.position.x; } }
    public int TileZ { get { return (int)transform.position.z; } }


    public Rect Boundary { get; private set; }


    HealthController _healthController;


    void Awake()
    {
        _healthController = GetComponent<HealthController>();
    }

    void Start()
    {
        Vector3 pos = transform.position;
        Boundary = new Rect(
            pos.x - BoundaryWidth * 0.5f,
            pos.z - BoundaryHeight * 0.5f,
            BoundaryWidth,
            BoundaryHeight
            );
    }


    public void PauseAnimation()
    {
        enemyAnim.Pause();
    }
    public void ResumeAnimation()
    {
        enemyAnim.Resume();
    }


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
        if (HasWeapon)
        {
            weapon.Attack();
        }
    }
    public void Attack(Vector3 direction)
    {
        if (HasWeapon)
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
        RigidbodyConstraints storedConstraints = GetComponent<Rigidbody>().constraints;
        GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
        GetComponent<Rigidbody>().AddForce(force, ForceMode.VelocityChange);

        SendMessage("DisableAI", SendMessageOptions.DontRequireReceiver);

        float duration = _healthController.tempInvincibilityDuration;
        yield return new WaitForSeconds(duration);


        GetComponent<Rigidbody>().constraints = storedConstraints;

        SendMessage("EnableAI", SendMessageOptions.DontRequireReceiver);
    }


    public bool CanBlockAttack(Vector3 directionOfAttack)
    {
        if (_healthController.isIndestructible)
        {
            return true;
        }

        return HasShield && shield.CanBlockAttack(directionOfAttack);
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


    void OnTriggerEnter(Collider otherCollider)
    {
        GameObject other = otherCollider.gameObject;
        //print("Enemy::OnTriggerEnter: " + other.name);

        if (other == PlayerController)
        {
            OnHitPlayer(other.transform.parent.gameObject);
        }
    }

    void OnHitPlayer(GameObject player)
    {
        //print("Enemy::OnHitPlayer");
        player.GetComponent<HealthController>().TakeDamage((uint)meleeDamage, gameObject);
    }

    void OnEnemyDeath()
    {
        if (SpawnPoint != null)
        {
            SpawnPoint.OnSpawnedEnemyDeath();
        }
    }
}