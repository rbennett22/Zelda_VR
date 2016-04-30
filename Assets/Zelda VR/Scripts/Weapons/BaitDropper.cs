using UnityEngine;


public class BaitDropper : MonoBehaviour
{
    public GameObject baitPrefab;
    public float baitDropDistance = 2.0f;
    public float cooldown = 1.0f;

    float BaitRadius { get { return baitPrefab.GetComponent<SphereCollider>().radius; } }


    GameObject _spawnedBait = null;
    Transform _projectilesContainer;
    float _lastBaitDropTime = float.NegativeInfinity;


    public bool CanUse { 
        get {
            if (_spawnedBait != null) { return false; }
            return Time.time - _lastBaitDropTime > cooldown;
        }
    }


    void Awake()
    {
        _projectilesContainer = GameObject.Find("Projectiles").transform;
    }

    public void DropBait()
    {
        if (!CanUse) { return; }

        _spawnedBait = Instantiate(baitPrefab) as GameObject;
        DetermineBaitDropPosition(_spawnedBait);
        _spawnedBait.transform.parent = _projectilesContainer;

        NotifyOnDestroy n = _spawnedBait.AddComponent<NotifyOnDestroy>();
        n.receiver = gameObject;
        n.methodName = "OnBaitDestroyed";

        _lastBaitDropTime = Time.time;
    }

    void DetermineBaitDropPosition(GameObject bait)
    {
        Vector3 baitPos;

        Vector3 pos = transform.position;
        Vector3 forward = transform.forward;
        Ray ray = new Ray(pos, forward);
        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo, baitDropDistance))
        {
            baitPos = pos + forward * (hitInfo.distance - BaitRadius);
        }
        else
        {
            baitPos = pos + forward * baitDropDistance;
        }

        bait.transform.position = baitPos;
    }

    void OnBaitDestroyed()
    {
        //print("OnBaitDestroyed");
        _spawnedBait = null;
    }

}
