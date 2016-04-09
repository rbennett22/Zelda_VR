using UnityEngine;


public class BombDropper : MonoBehaviour
{
    public GameObject bombPrefab;
    public AudioClip bombDropSound;
    public float bombDropDistance = 1.0f;

    float BombRadius { get { return bombPrefab.GetComponent<SphereCollider>().radius; } }


    GameObject _spawnedBomb = null;
    Transform _projectilesContainer;


    public bool CanUse { get { return _spawnedBomb == null; } }


    void Awake()
    {
        _projectilesContainer = GameObject.Find("Projectiles").transform;
    }

    public void DropBomb()
    {
        if (!CanUse) { return; }

        _spawnedBomb = Instantiate(bombPrefab) as GameObject;
        DetermineBombDropPosition(_spawnedBomb);
        _spawnedBomb.transform.parent = _projectilesContainer;
        
        SoundFx.Instance.PlayOneShot(bombDropSound);

        NotifyOnDestroy n = _spawnedBomb.AddComponent<NotifyOnDestroy>();
        n.receiver = gameObject;
        n.methodName = "OnBombDestroyed";
    }

    void DetermineBombDropPosition(GameObject bomb)
    {
        Vector3 bombPos;

        Vector3 pos = transform.position;
        Vector3 forward = transform.forward;

        Ray ray = new Ray(pos, forward * bombDropDistance);
        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo, bombDropDistance))
        {
            bombPos = pos + forward * (hitInfo.distance - BombRadius);
        }
        else
        {
            bombPos = pos + forward * bombDropDistance;
        }

        bomb.transform.position = bombPos;
    }

    void OnBombDestroyed()
    {
        //print("OnBombDestroyed");
        _spawnedBomb = null;
    }

}
