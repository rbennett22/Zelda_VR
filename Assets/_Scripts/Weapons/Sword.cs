using UnityEngine;
using System.Collections.Generic;


public class Sword : MonoBehaviour
{
    const float ShieldBlockDotThreshold = 0.6f;   // [0-1].  Closer to 1 means enemy has to be facing an incoming attack more directly in order to block it.


    public GameObject projectilePrefab;
    public float reach = 1;
    public float speed = 1;
    public int damage = 1;
    public AudioClip swordSound, swordShootSound;
    public bool projectileEnabled;
    public string[] invulnerables;  // Enemies that cannot be hurt by sword 


    bool _isExtending;
    Vector3 _origin;
    GameObject _spawnedProjectile = null;
    Transform _projectilesContainer;
    List<string> _invulnerables;


    public bool IsAttacking { get; set; }


    void Awake()
    {
        _projectilesContainer = GameObject.Find("Projectiles").transform;
        _invulnerables = new List<string>(invulnerables);
    }

    void Start()
    {
        _origin = transform.localPosition;
        GetComponent<Collider>().enabled = false;
    }


    public void Attack()
    {
        if (IsAttacking)
        {
            return;
        }

        Vector3 targetPos = transform.localPosition + Vector3.forward * reach;

        iTween.MoveTo(gameObject, iTween.Hash(
            "islocal", true,
            "position", targetPos,
            "speed", speed,
            "easetype", iTween.EaseType.linear,
            "oncomplete", "OnExtendedCompletely",
            "oncompletetarget", gameObject)
        );

        SoundFx.Instance.PlayOneShot(swordSound);

        GetComponent<Collider>().enabled = true;
        IsAttacking = true;
        _isExtending = true;
    }

    void OnExtendedCompletely()
    {
        iTween.MoveTo(gameObject, iTween.Hash(
            "islocal", true,
            "position", _origin,
            "speed", speed,
            "easetype", iTween.EaseType.linear,
            "oncomplete", "OnRetractedCompletely",
            "oncompletetarget", gameObject)
        );

        if (projectileEnabled && _spawnedProjectile == null)
        {
            FireProjectile();
        }

        _isExtending = false;
    }

    void OnRetractedCompletely()
    {
        IsAttacking = false;
        GetComponent<Collider>().enabled = false;
    }


    void FireProjectile()
    {
        _spawnedProjectile = Instantiate(projectilePrefab) as GameObject;
        SimpleProjectile p = _spawnedProjectile.GetComponent<SimpleProjectile>();
        
        p.transform.parent = _projectilesContainer;
        p.transform.position = transform.position;
        p.transform.rotation = transform.rotation;
        p.direction = transform.up;
        p.damage = damage;

        if (name == "MagicSword_Weapon") { p.name = "MagicSword_Projectile"; }
        else if (name == "WhiteSword_Weapon") { p.name = "WhiteSword_Projectile"; }
        else if (name == "WoodenSword_Weapon") { p.name = "WoodenSword_Projectile"; }

        SoundFx.Instance.PlayOneShot(swordShootSound);

        NotifyOnDestroy n = _spawnedProjectile.AddComponent<NotifyOnDestroy>();
        n.receiver = gameObject;
        n.methodName = "OnProjectileDestroyed";
    }

    void OnProjectileDestroyed()
    {
        //print("OnProjectileDestroyed");
        _spawnedProjectile = null;
    }


    void OnCollisionEnter(Collision collision)
    {
        GameObject other = collision.gameObject;
        //print("Sword --> OnCollisionEnter: " + other.name);

        HealthController hc = other.GetComponent<HealthController>();
        if (hc != null)
        {
            Enemy enemy = other.GetComponent<Enemy>();
            bool blocked = _invulnerables.Contains(other.name) || enemy.CanBlockAttack(transform.up);
            if (blocked)
            {
                if (enemy.playSoundWhenBlockingAttack)
                {
                    SoundFx sfx = SoundFx.Instance;
                    sfx.PlayOneShot(sfx.shield);
                }
            }
            else
            {
                hc.TakeDamage((uint)damage, gameObject);
            }
        }

        ForceRetract();
    }

    void ForceRetract()
    {
        if (!_isExtending) { return; }

        iTween.MoveTo(gameObject, iTween.Hash(
            "islocal", true,
            "position", _origin,
            "speed", speed,
            "easetype", iTween.EaseType.linear,
            "oncomplete", "OnRetractedCompletely",
            "oncompletetarget", gameObject)
        );
    }

}
