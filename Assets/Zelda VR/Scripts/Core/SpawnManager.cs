using System;
using UnityEngine;


public interface ISpawnManager
{
    void SetSpawningEnabled(bool value);
    void DoUpdate(bool ignoreProxThreshMin = false);
}

// TODO

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
        //InvokeRepeating("Tick", 0, _updateInterval);  // TODO
    }


    void Tick()
    {
        (this as ISpawnManager).DoUpdate();
    }
    void ISpawnManager.DoUpdate(bool ignoreProxThreshMin = false)
    {
        // TODO
    }


    void ISpawnManager.SetSpawningEnabled(bool value)
    {
        // TODO
    }
}