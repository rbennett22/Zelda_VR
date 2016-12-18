using UnityEngine;


public interface ISpawnManager
{
    void DoUpdate(bool ignoreProxThreshMin = false);
}


public class SpawnManager : MonoBehaviour, ISpawnManager
{
    [SerializeField]
    float _updateInterval = 1.0f;
    [SerializeField]
    float _spawnDistThreshold = 8;
    float _spawnDistThresholdSq;


    void Awake()
    {
        _spawnDistThresholdSq = _spawnDistThreshold * _spawnDistThreshold;
    }

    void Start()
    {
        InvokeRepeating("Tick", 0, _updateInterval);
    }


    void Tick()
    {
        (this as ISpawnManager).DoUpdate();
    }
    void ISpawnManager.DoUpdate(bool ignoreProxThreshMin = false)
    {
        //
    }
}