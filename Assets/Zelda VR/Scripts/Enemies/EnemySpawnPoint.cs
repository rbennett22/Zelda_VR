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


    float _lastEnemyTimeOfDeath = float.NegativeInfinity;
    GameObject _spawnedEnemy = null;
    Transform _enemiesContainer;
    float _proximityThresholdMin, _proximityThresholdMax;
    float _proximityThresholdMinSq, _proximityThresholdMaxSq;


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

        _proximityThresholdMinSq = _proximityThresholdMin * _proximityThresholdMin;
        _proximityThresholdMaxSq = _proximityThresholdMax * _proximityThresholdMax;


        _enemiesContainer = GameObject.Find("Enemies").transform;

        // The SpawnPoint's SpriteRenderer exists only for convenience in the Editor, so we Destroy it here
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        Destroy(sr);

        if (specialDrop != null)
        {
            specialDrop.gameObject.SetActive(false);
        }
    }

    public void Update()
    {
        if (_spawnedEnemy != null)
        {
            Enemy e = _spawnedEnemy.GetComponent<Enemy>();
            AssignBoundaryToSpawnedEnemy(e);
        }
    }

    public void DoUpdate()
    {
        if (!autoSpawn) { return; }
        if (_spawnedEnemy != null) { return; }

        float timeSinceLastSpawn = Time.time - _lastEnemyTimeOfDeath;
        if (timeSinceLastSpawn < cooldown) { return; }

        Vector3 toPlayer = CommonObjects.Player_C.Position - transform.position;
        float distToPlayerSq = Vector3.SqrMagnitude(toPlayer);
        if (distToPlayerSq > _proximityThresholdMaxSq) { return; }
        if (distToPlayerSq < _proximityThresholdMinSq) { return; }

        SpawnEnemy();
    }

    public GameObject SpawnEnemy()
    {
        GameObject g = Instantiate(enemyPrefab, transform.position, Quaternion.identity) as GameObject;
        g.name = enemyPrefab.name;

        Transform t = g.transform;
        t.SetParent(WorldInfo.Instance.IsOverworld ? _enemiesContainer : transform.parent);
        t.forward = transform.up;

        Enemy enemy = g.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.SpawnPoint = this;

            AssignBoundaryToSpawnedEnemy(enemy);
            AssignSpecialDropItemToSpawnedEnemy(enemy);
        }

        NotifyOnDestroy n = g.AddComponent<NotifyOnDestroy>();
        n.receiver = gameObject;
        n.methodName = "OnSpawnedEnemyDestroyed";

        _spawnedEnemy = g;

        SendMessage("OnEnemySpawned", enemy, SendMessageOptions.DontRequireReceiver);

        return g;
    }

    void AssignBoundaryToSpawnedEnemy(Enemy enemy)
    {
        if (enemy == null) { return; }

        EnemyAI ai = enemy.GetComponent<EnemyAI>();
        if (ai == null) { return; }

        if (WorldInfo.Instance.IsInDungeon)
        {
            /*if(enemy.name == "Wizzrobe_Blue")     // TODO
            {
                print("!!! Wizzrobe_Blue");
            }*/
            DungeonRoom dr = DungeonRoom.GetRoomForPosition(transform.position);
            ai.Boundary = dr.Bounds;
        }
        else
        {
            TileMap tileMap = CommonObjects.OverworldTileMap;
            enemy.Sector = tileMap.GetSectorContainingPosition(transform.position);
            ai.Boundary = tileMap.GetBoundsForSector(enemy.Sector);
        }
    }
    void AssignSpecialDropItemToSpawnedEnemy(Enemy enemy)
    {
        if (specialDrop == null)
        {
            return;
        }

        if (WorldInfo.Instance.IsInDungeon)
        {
            DungeonRoom dr = DungeonRoom.GetRoomForPosition(transform.position);
            if (dr.SpecialDropItemHasBeenCollected)
            {
                return;
            }
        }

        enemy.GetComponent<EnemyItemDrop>().specialDrop = specialDrop;

        Transform t = specialDrop.transform;
        t.SetParent(enemy.transform);
        t.localPosition = Vector3.zero;

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


    public void ForceCooldown()
    {
        _lastEnemyTimeOfDeath = Time.time;
    }
}