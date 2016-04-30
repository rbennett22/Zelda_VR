using UnityEngine;


public class Weapon : MonoBehaviour 
{
    public float attackDuration = 1;
    public GameObject projectilePrefab;
    public AudioClip projectileFireSound;
    public bool isWand;
    public int damage = -1;     // Unless set damage is determined by the projectile itself


    protected float _attackStartTime = float.NegativeInfinity;
    GameObject _spawnedProjectile = null;
    Transform _projectilesContainer;


    virtual public bool IsAttacking { get { return (Time.time - _attackStartTime < attackDuration); } }


    void Awake()
    {
        _projectilesContainer = GameObject.Find("Projectiles").transform;
    }


    public void Fire()
    {
        Fire(Vector3.zero);
    }
    virtual public void Fire(Vector3 direction)
    {
        _attackStartTime = Time.time;

        _spawnedProjectile = Instantiate(projectilePrefab) as GameObject;

        float projectileLength = 0;
        CapsuleCollider cc = _spawnedProjectile.GetComponent<CapsuleCollider>();
        if (cc != null) { projectileLength = cc.height; }
        else
        {
            SphereCollider sc = _spawnedProjectile.GetComponent<SphereCollider>();
            if (sc != null) { projectileLength = sc.radius * 2; }
        }

        Vector3 dir = (direction == Vector3.zero) ? transform.forward : direction;
        Vector3 offset = projectileLength * 0.5f * dir;

        SimpleProjectile p = _spawnedProjectile.GetComponent<SimpleProjectile>();
        p.transform.parent = _projectilesContainer;
        p.transform.position = transform.position + offset;
        Vector3 dirEuler = Quaternion.FromToRotation(Vector3.forward, dir).eulerAngles;
        p.transform.rotation = Quaternion.Euler(90, dirEuler.y, 0);

        //p.transform.up = dir;
        p.direction = dir;

        if (damage >= 0)
        {
            p.damage = damage;
        }

        if (isWand)
        {
            p.transform.up = transform.forward;
            Vector3 euler = p.transform.rotation.eulerAngles;
            euler.x = 90;
            p.transform.rotation = Quaternion.Euler(euler);
        }

        SoundFx.Instance.PlayOneShot3D(transform.position, projectileFireSound);

        NotifyOnDestroy n = _spawnedProjectile.AddComponent<NotifyOnDestroy>();
        n.receiver = gameObject;
        n.methodName = "OnProjectileDestroyed";
    }

    void OnProjectileDestroyed()
    {
        //print("OnProjectileDestroyed");
        _spawnedProjectile = null;
    }

}
