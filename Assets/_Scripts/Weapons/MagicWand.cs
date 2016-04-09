using UnityEngine;


public class MagicWand : MonoBehaviour 
{
    public GameObject shotPrefab;
    public AudioClip shotFireSound;
    public GameObject flamePrefab;
    public bool spawnFlame;
    public AudioClip flameDropSound;


    GameObject _spawnedShot = null;
    Transform _projectilesContainer;


    public bool CanUse { get { return (_spawnedShot == null); } }


    void Awake()
    {
        _projectilesContainer = GameObject.Find("Projectiles").transform;
    }

    public void Fire()
    {
        _spawnedShot = Instantiate(shotPrefab) as GameObject;

        float shotLength = 0;
        CapsuleCollider cc = _spawnedShot.GetComponent<CapsuleCollider>();
        if (cc != null) { shotLength = cc.radius * 2; }
        else
        {
            SphereCollider sc = _spawnedShot.GetComponent<SphereCollider>();
            if (sc != null) { shotLength = sc.radius * 2; }
        }

        Vector3 offset = shotLength * 0.7f * transform.forward;

        SimpleProjectile p = _spawnedShot.GetComponent<SimpleProjectile>();
        p.transform.parent = _projectilesContainer;
        p.transform.position = transform.position + offset;
        //p.transform.AddToY(0.15f);

        p.transform.up = transform.forward;
        Vector3 euler = p.transform.rotation.eulerAngles;
        euler.x = 90;
        p.transform.rotation = Quaternion.Euler(euler);

        p.direction = transform.forward;

        SoundFx.Instance.PlayOneShot(shotFireSound);

        NotifyOnDestroy n = _spawnedShot.AddComponent<NotifyOnDestroy>();
        n.receiver = gameObject;
        n.methodName = "OnShotDestroyed";
    }

    void OnShotDestroyed()
    {
        //print("OnShotDestroyed");

        if (spawnFlame)
        {
            Vector3 pos = _spawnedShot.transform.position + _spawnedShot.GetComponent<SimpleProjectile>().direction * 0.3f;
            SpawnFlame(pos);
        }

        _spawnedShot = null;
    }

    void SpawnFlame(Vector3 position)
    {
        GameObject flame = Instantiate(flamePrefab, position, Quaternion.identity) as GameObject;
        flame.transform.parent = _spawnedShot.transform.parent;

        SoundFx.Instance.PlayOneShot(flameDropSound);

        DungeonRoom dr = CommonObjects.Player_C.OccupiedDungeonRoom();
        if (dr != null && !dr.IsNpcRoom)
        {
            dr.ActivateTorchLights();
        }
    }

}
