using UnityEngine;


public class EnemySpawnPoint : MonoBehaviour 
{
    const float ProximityThresholdMin_OW = 8;           // how far away player must be to trigger the spawn               
    const float ProximityThresholdMax_OW = 12;          // how close player must be to trigger the spawn 
    const float ProximityThresholdMin_Dungeon = 1;
    const float ProximityThresholdMax_Dungeon = 11;


    public GameObject enemyPrefab;
    public bool willRespawn = true;
    public float cooldown = 30;                     // minimum time between spawns
    public float spawnDistanceMin = -1;
    public float spawnDistance = -1;
    public Collectible specialDrop = null;
    public bool autoSpawn = false;


    OVRPlayerController _ovrPlayerController;
    float _lastEnemyTimeOfDeath = float.NegativeInfinity;
    GameObject _spawnedEnemy = null;
    Transform _enemiesContainer;
    float _proximityThresholdMin, _proximityThresholdMax;
    float _proximityThresholdMinSqd, _proximityThresholdMaxSqd;


    void Awake()
    {
        if (spawnDistance == -1)
        {
            _proximityThresholdMax = WorldInfo.Instance.IsInDungeon ? ProximityThresholdMax_Dungeon : ProximityThresholdMax_OW;
        }
        else
        {
            _proximityThresholdMax = spawnDistance;
        }

        if (spawnDistanceMin == -1)
        {
            _proximityThresholdMin = WorldInfo.Instance.IsInDungeon ? ProximityThresholdMin_Dungeon : ProximityThresholdMin_OW;
        }
        else
        {
            _proximityThresholdMin = spawnDistanceMin;
        }

        _proximityThresholdMinSqd = _proximityThresholdMin * _proximityThresholdMin;
        _proximityThresholdMaxSqd = _proximityThresholdMax * _proximityThresholdMax;


        _ovrPlayerController = GameObject.Find("OVRPlayerController").GetComponent<OVRPlayerController>();
        _enemiesContainer = GameObject.Find("Enemies").transform;

        // The SpawnPoint's SpriteRenderer exists only for convenience in the Editor, so we Destroy it here
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        Destroy(sr);

        if (specialDrop != null) { specialDrop.gameObject.SetActive(false); }
    }


    public void DoUpdate()
    {
        if (autoSpawn)
        {
            if (_spawnedEnemy != null) { return; }

            float timeSinceLastSpawn = Time.time - _lastEnemyTimeOfDeath;
            if (timeSinceLastSpawn < cooldown) { return; }

            Vector3 toPlayer = _ovrPlayerController.transform.position - transform.position;
            float distanceToPlayerSqr = Vector3.SqrMagnitude(toPlayer);
            if (distanceToPlayerSqr > _proximityThresholdMaxSqd) { return; }
            if (distanceToPlayerSqr < _proximityThresholdMinSqd) { return; }

            SpawnEnemy();
        }
    }

    public GameObject SpawnEnemy()
    {
        _spawnedEnemy = Instantiate(enemyPrefab, transform.position, Quaternion.identity) as GameObject;
        _spawnedEnemy.name = enemyPrefab.name;
        _spawnedEnemy.transform.parent = WorldInfo.Instance.IsOverworld ? _enemiesContainer : transform.parent;
        _spawnedEnemy.transform.forward = transform.up;

        print("_spawnedEnemy: " + _spawnedEnemy.name);

        Enemy enemy = _spawnedEnemy.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.SpawnPoint = this;
            AssignSpecialDropItemToSpawnedEnemy(enemy);
        }

        NotifyOnDestroy n = _spawnedEnemy.AddComponent<NotifyOnDestroy>();
        n.receiver = gameObject;
        n.methodName = "OnSpawnedEnemyDestroyed";

        SendMessage("OnEnemySpawned", enemy, SendMessageOptions.DontRequireReceiver);

        return _spawnedEnemy;
    }

    void AssignSpecialDropItemToSpawnedEnemy(Enemy enemy)
    {
        if (specialDrop == null) { return; }

        if (WorldInfo.Instance.IsInDungeon)
        {
            DungeonRoom dr = DungeonRoom.GetRoomForPosition(transform.position);
            if (dr.SpecialDropItemHasBeenCollected) { return; }
        }

        enemy.GetComponent<EnemyItemDrop>().specialDrop = specialDrop;
        specialDrop.transform.parent = enemy.transform;
        specialDrop.transform.localPosition = Vector3.zero;
        specialDrop.gameObject.SetActive(true);
    }


    public void OnSpawnedEnemyDeath()
    {
        _lastEnemyTimeOfDeath = willRespawn ? Time.time : float.PositiveInfinity;
    }

    void OnSpawnedEnemyDestroyed()
    {
        _spawnedEnemy = null;
    }

}
