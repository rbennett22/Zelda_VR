using UnityEngine;


public interface IAutoSpawnerDelegate
{
    bool ShouldObjectSpawn(AutoSpawner autoSpawner, ref Vector3 spawnPosition, ref Vector3 spawnVelocity);	// (The return value indicates whether the Spawn will happen)
}


public class AutoSpawner : MonoBehaviour, IAutoSpawner
{
    public IAutoSpawnerDelegate autoSpawnerDelegate;
    public GameObject spawnObjectPrefab;

    public float cooldown = 0.1f;
    public int maxLiveObjects = 5;
    public float spawnedObjectSpeed = 0;
    public Vector3 spawnedObjectMoveDirection = Vector3.zero;
    public Transform spawnedObjectParent;
    public bool doAutoSpawn = false;


    float _cooldownTimer = 0.1f;
    int _currNumObjects = 0;


    public Vector3 spawnOrigin_Local = Vector3.zero;
    public Vector3 spawnOrigin_World { get { return transform.TransformPoint(spawnOrigin_Local); } }


    void Start()
    {
        if (!spawnedObjectParent)
            spawnedObjectParent = transform.parent;
    }


    public GameObject RequestToSpawnObject()
    {
        return RequestToSpawnObject(spawnOrigin_World, spawnedObjectMoveDirection * spawnedObjectSpeed);
    }
    public GameObject RequestToSpawnObject(Vector3 velocity)
    {
        return RequestToSpawnObject(spawnOrigin_World, velocity);
    }
    public GameObject RequestToSpawnObject(Vector3 position, Vector3 velocity)
    {
        if (_cooldownTimer > 0 || _currNumObjects >= maxLiveObjects)
            return null;

        if (autoSpawnerDelegate != null)
        {
            if (!autoSpawnerDelegate.ShouldObjectSpawn(this, ref position, ref velocity))
                return null;
        }

        return SpawnObject(position, velocity);
    }
    GameObject SpawnObject(Vector3 position, Vector3 velocity)
    {
        GameObject spawnedObj = Instantiate(spawnObjectPrefab, position, Quaternion.identity) as GameObject;

        if (spawnedObjectParent)
            spawnedObj.transform.parent = spawnedObjectParent;

        Rigidbody rb = spawnedObj.GetComponent<Rigidbody>();
        if (rb)
            rb.AddForce(velocity, ForceMode.VelocityChange);

        _cooldownTimer = cooldown;
        _currNumObjects++;

        AutoSpawnedObject autoSpawnComponent = spawnedObj.AddComponent<AutoSpawnedObject>();
        autoSpawnComponent.autoSpawner = this;
        //print("autoSpawnComponent: " + autoSpawnComponent);
        //print("autoSpawnComponent.autoSpawner: " + autoSpawnComponent.autoSpawner);

        return spawnedObj;
    }


    void Update()
    {
        _cooldownTimer -= Time.deltaTime;

        if (doAutoSpawn)
            RequestToSpawnObject();
    }


    #region IAutoSpawner

    void IAutoSpawner.OnSpawnedObjectDestroy(AutoSpawnedObject spawnedObj)
    {
        _currNumObjects--;
    }

    #endregion IAutoSpawner
}