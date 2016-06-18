using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [SerializeField]
    float _updateInterval = 1.0f;
    [SerializeField]
    float _spawnDistThreshold = 8;
    float _spawnDistThresholdSq;


    void Start()
    {
        InvokeRepeating("Tick", 0, _updateInterval);
    }


    void Tick()
    {

    }
}