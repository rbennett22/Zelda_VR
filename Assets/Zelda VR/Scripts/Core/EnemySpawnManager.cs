using UnityEngine;

public class EnemySpawnManager : MonoBehaviour, ISpawnManager
{
    [SerializeField]
    float _updateInterval = 0.5f;
    [SerializeField]
    float _enemyRemovalDistance = 32;      // How far away Enemy must be from player before it is destroyed (Overworld only)


    float _enemyRemovalDistanceSq;


    void Start()
    {
        _enemyRemovalDistanceSq = _enemyRemovalDistance * _enemyRemovalDistance;

        InvokeRepeating("Tick", 0, _updateInterval);
    }


    void Tick()
    {
        (this as ISpawnManager).DoUpdate();
    }
    void ISpawnManager.DoUpdate(bool ignoreProxThreshMin = false)
    {
        foreach (EnemySpawnPoint sp in GetComponentsInChildren<EnemySpawnPoint>())
        {
            sp.DoUpdate(ignoreProxThreshMin);
        }

        Vector3 playerPos = CommonObjects.Player_C.Position;

        foreach (Transform child in CommonObjects.EnemiesContainer)
        {
            Vector3 toPlayer = playerPos - child.position;
            float distToPlayerSq = toPlayer.sqrMagnitude;
            if (distToPlayerSq > _enemyRemovalDistanceSq)
            {
                Destroy(child.gameObject);
            }
        }
    }
    void ISpawnManager.SetSpawningEnabled(bool value)
    {
        foreach (EnemySpawnPoint sp in GetComponentsInChildren<EnemySpawnPoint>())
        {
            sp.SpawningEnabled = value;
        }
    }
}