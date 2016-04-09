using UnityEngine;
using System.Collections;


public class DungeonEntranceSpawnPoint : MonoBehaviour
{

    public GameObject dungeonEntrancePrefab;
    public GameObject marker;
    public float spawnDistance = 12;

    public int dungeonNum;


    DungeonEntrance _spawnedDungeonEntrance = null;
    Transform _dungeonEntranceContainer;
    OVRPlayerController _ovrPlayerController;

    float _spawnDistanceSqd;
    float _destroyDistanceSqd;


    void Awake()
    {
        _dungeonEntranceContainer = GameObject.Find("DungeonEntrances").transform;
        _ovrPlayerController = GameObject.Find("OVRPlayerController").GetComponent<OVRPlayerController>();

        _spawnDistanceSqd = spawnDistance * spawnDistance;
        _destroyDistanceSqd = _spawnDistanceSqd + 20;

        marker.SetActive(false);
    }


    void Update()
    {
        Vector3 toPlayer = _ovrPlayerController.transform.position - marker.transform.position;
        float distanceToPlayerSqr = Vector3.SqrMagnitude(toPlayer);

        if (_spawnedDungeonEntrance == null)
        {
            if (distanceToPlayerSqr < _spawnDistanceSqd) { SpawnDungeonEntrance(); }
        }
        else
        {
            if (distanceToPlayerSqr > _destroyDistanceSqd) { DestroyDungeonEntrance(); }
        }
    }

    public DungeonEntrance SpawnDungeonEntrance()
    {
        GameObject g = Instantiate(dungeonEntrancePrefab, transform.position, transform.rotation) as GameObject;
        _spawnedDungeonEntrance = g.GetComponent<DungeonEntrance>();
        _spawnedDungeonEntrance.name = "Dungeon Entrance " + dungeonNum.ToString();
        _spawnedDungeonEntrance.transform.parent = _dungeonEntranceContainer;
        _spawnedDungeonEntrance.DungeonNum = dungeonNum;

        return _spawnedDungeonEntrance;
    }

    void DestroyDungeonEntrance()
    {
        Destroy(_spawnedDungeonEntrance.gameObject);
        _spawnedDungeonEntrance = null;
    }

}