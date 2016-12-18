using UnityEngine;

public class GrottoSpawnManager : MonoBehaviour, ISpawnManager
{
    [SerializeField]
    float _updateInterval = 0.5f;
    [SerializeField]
    float _spawnDistThreshold = 8;
    float _spawnDistThresholdSq;


    GrottoSpawnPoint _activeGrottoSP;


    void Start()
    {
        _spawnDistThresholdSq = _spawnDistThreshold * _spawnDistThreshold;

        InvokeRepeating("Tick", 0, _updateInterval);
    }


    void Tick()
    {
        (this as ISpawnManager).DoUpdate();
    }
    void ISpawnManager.DoUpdate(bool ignoreProxThreshMin = false)
    {
        if (PlayerIsInsideAGrotto())
        {
            return;
        }

        GrottoSpawnPoint closestGSP = null;
        float closestDistSq = GetGrottoClosestToPlayer(out closestGSP);

        if (_activeGrottoSP != null)
        {
            if (closestDistSq > _spawnDistThresholdSq || closestGSP != _activeGrottoSP)
            {
                DestroyCurrentlyActiveGrotto();
            }
        }

        if (closestDistSq <= _spawnDistThresholdSq && closestGSP != _activeGrottoSP)
        {
            SpawnNewActiveGrotto(closestGSP);
        }
    }

    float GetGrottoClosestToPlayer(out GrottoSpawnPoint gSP)
    {
        Vector3 playerPos = CommonObjects.Player_C.Position;

        float closestDistSq = float.PositiveInfinity;
        Transform closestGSP = null;
        foreach (Transform child in transform)
        {
            Vector3 toPlayer = playerPos - child.GetComponent<GrottoSpawnPoint>().marker.transform.position;
            float distanceToPlayerSqr = Vector3.SqrMagnitude(toPlayer);
            if (distanceToPlayerSqr < closestDistSq)
            {
                closestDistSq = distanceToPlayerSqr;
                closestGSP = child;
            }
        }

        gSP = closestGSP.GetComponent<GrottoSpawnPoint>();

        return closestDistSq;
    }

    void DestroyCurrentlyActiveGrotto()
    {
        if (_activeGrottoSP == null) { return; }
            
        _activeGrottoSP.DestroyGrotto();
        _activeGrottoSP = null;
    }

    void SpawnNewActiveGrotto(GrottoSpawnPoint gSP)
    {
        gSP.SpawnGrotto();
        _activeGrottoSP = gSP;
    }


    public bool PlayerIsInsideAGrotto()
    {
        return (_activeGrottoSP != null) && _activeGrottoSP.SpawnedGrotto.PlayerIsInside;
    }
}