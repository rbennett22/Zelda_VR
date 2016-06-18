using UnityEngine;

public class DungeonEntranceSpawnPoint : MonoBehaviour
{
    [SerializeField]
    float _updateInterval = 0.5f;

    public GameObject dungeonEntrancePrefab;
    public GameObject marker;

    public float spawnDistance = 12;
    public int dungeonNum;


    DungeonEntrance _spawnedDungeonEntrance = null;
    Transform _dungeonEntranceContainer;

    float _spawnDistanceSqd;
    float _destroyDistanceSqd;


    void Awake()
    {
        _dungeonEntranceContainer = GameObject.Find("DungeonEntrances").transform;

        _spawnDistanceSqd = spawnDistance * spawnDistance;
        _destroyDistanceSqd = _spawnDistanceSqd + 20;

        marker.SetActive(false);
    }

    void Start()
    {
        InvokeRepeating("Tick", 0, _updateInterval);
    }


    void Tick()
    {
        Vector3 toPlayer = CommonObjects.Player_C.Position - marker.transform.position;
        float distToPlayerSqd = Vector3.SqrMagnitude(toPlayer);

        GrottoSpawnManager gsp = FindObjectOfType<GrottoSpawnManager>();
        bool playerIsInsideAGrotto = (gsp == null) ? false : gsp.PlayerIsInsideAGrotto();

        if (_spawnedDungeonEntrance == null)
        {
            if (distToPlayerSqd < _spawnDistanceSqd && !playerIsInsideAGrotto)
            {
                SpawnDungeonEntrance();
            }
        }
        else
        {
            if (distToPlayerSqd > _destroyDistanceSqd || playerIsInsideAGrotto)
            {
                DestroyDungeonEntrance();
            }
        }
    }

    void SpawnDungeonEntrance()
    {
        GameObject g = Instantiate(dungeonEntrancePrefab, transform.position, transform.rotation) as GameObject;

        DungeonEntrance e = g.GetComponent<DungeonEntrance>();
        
        e.name = "Dungeon Entrance " + dungeonNum.ToString();
        e.transform.SetParent(_dungeonEntranceContainer);
        e.DungeonNum = dungeonNum;

        _spawnedDungeonEntrance = e;
    }

    void DestroyDungeonEntrance()
    {
        Destroy(_spawnedDungeonEntrance.gameObject);
        _spawnedDungeonEntrance = null;
    }
}