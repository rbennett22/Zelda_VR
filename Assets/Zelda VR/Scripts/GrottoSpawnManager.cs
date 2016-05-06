using UnityEngine;
using System.Diagnostics;
using System.Collections;

public class GrottoSpawnManager : MonoBehaviour
{
    [SerializeField]
    float _updateInterval_ms = 500;
    [SerializeField]
    float _spawnDistance = 8;


    float _spawnDistanceSq;
    GrottoSpawnPoint _activeGrottoSP;
    

	void Start () 
    {
        _spawnDistanceSq = _spawnDistance * _spawnDistance;

        StartCoroutine("Update_Coroutine");
    }


    IEnumerator Update_Coroutine()
    {
        while (true)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            {
                Tick();
            }
            stopWatch.Stop();

            int elapsedTime = stopWatch.Elapsed.Milliseconds;
            float waitTime = 0.001f * Mathf.Max(0, _updateInterval_ms - elapsedTime);
            yield return new WaitForSeconds(waitTime);
        }
    }

    void Tick()
    {
        if (_activeGrottoSP == null || !_activeGrottoSP.SpawnedGrotto.PlayerIsInside)
        {
            Vector3 playerPos = CommonObjects.PlayerController_G.transform.position;

            float closestDistSq = float.PositiveInfinity;
            Transform closestGrottoSP = null;
            foreach (Transform child in transform)
            {
                Vector3 toPlayer = playerPos - child.GetComponent<GrottoSpawnPoint>().marker.transform.position;
                float distanceToPlayerSqr = Vector3.SqrMagnitude(toPlayer);
                if (distanceToPlayerSqr < closestDistSq)
                {
                    closestDistSq = distanceToPlayerSqr;
                    closestGrottoSP = child;
                }
            }

            GrottoSpawnPoint gsp = closestGrottoSP.GetComponent<GrottoSpawnPoint>();

            // Destroy currently active grotto?
            if (closestDistSq > _spawnDistanceSq || gsp != _activeGrottoSP)
            {
                if (_activeGrottoSP != null)
                {
                    _activeGrottoSP.DestroyGrotto();
                    _activeGrottoSP = null;
                }
            }

            // Spawn new
            if (closestDistSq < _spawnDistanceSq && gsp != _activeGrottoSP)
            {
                gsp.SpawnGrotto();
                _activeGrottoSP = gsp;
            }
        }
    }
}