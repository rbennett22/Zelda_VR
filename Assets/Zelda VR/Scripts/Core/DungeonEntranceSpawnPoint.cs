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

    float _spawnDistanceSq;
    float _destroyDistanceSq;


    void Awake()
    {
        _dungeonEntranceContainer = GameObject.Find("DungeonEntrances").transform;

        _spawnDistanceSq = spawnDistance * spawnDistance;
        _destroyDistanceSq = _spawnDistanceSq + 20;

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
            if (distToPlayerSqd < _spawnDistanceSq && !playerIsInsideAGrotto)
            {
                SpawnDungeonEntrance();
            }
        }
        else
        {
            if (distToPlayerSqd > _destroyDistanceSq || playerIsInsideAGrotto)
            {
                DestroyDungeonEntrance();
            }
        }
    }

    void SpawnDungeonEntrance()
    {
        if (_spawnedDungeonEntrance != null)
        {
            return;
        }

        GameObject g = Instantiate(dungeonEntrancePrefab, transform.position, transform.rotation) as GameObject;

        DungeonEntrance e = g.GetComponent<DungeonEntrance>();   
        e.name = "Dungeon Entrance " + dungeonNum.ToString();
        e.transform.SetParent(_dungeonEntranceContainer);
        e.DungeonNum = dungeonNum;

        _spawnedDungeonEntrance = e;
    }

    void DestroyDungeonEntrance()
    {
        if(_spawnedDungeonEntrance == null)
        {
            return;
        }

        Destroy(_spawnedDungeonEntrance.gameObject);
        _spawnedDungeonEntrance = null;
    }
}