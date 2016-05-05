using UnityEngine;
using System.Collections;
using System.Diagnostics;

public class CollectibleProliferator : MonoBehaviour
{
    public int updateInterval_ms = 1000;


    int _removalDist, _removalDistSq;

    Transform _playerTransform;


    void Awake()
    {
        _playerTransform = CommonObjects.PlayerController_G.transform;

        _removalDist = ZeldaVRSettings.Instance.collectibleRemovalDistance;
        _removalDistSq = _removalDist * _removalDist;
    }

    void Start()
    {
        StartCoroutine("Update_CR");
    }


    IEnumerator Update_CR()
    {
        // TODO: replace with a simple InvokeRepeating?

        while (true)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            {
                Tick();

                Resources.UnloadUnusedAssets();
            }
            stopWatch.Stop();

            int elapsedTime = stopWatch.Elapsed.Milliseconds;
            float waitTime = 0.001f * Mathf.Max(0, updateInterval_ms - elapsedTime);
            yield return new WaitForSeconds(waitTime);
        }
    }

    void Tick()
    {
        GameObject g = GameObject.FindGameObjectWithTag("OverworldInfo");       // TODO: cache this?
        if (g == null)
        {
            return;
        }

        OverworldInfo owInfo = g.GetComponent<OverworldInfo>();
        Vector3 playerPos = _playerTransform.position;

        foreach (Transform child in owInfo.collectibleSPs)
        {
            CollectibleSpawnPoint csp = child.GetComponent<CollectibleSpawnPoint>();
            if (csp == null) { continue; }

            float distSqr = (csp.transform.position - playerPos).sqrMagnitude;
            if (distSqr > _removalDistSq)
            {
                csp.DestroySpawnedCollectible();
            }
            else
            {
                if (csp.SpawnedCollectible == null && !csp.HasBeenCollected)
                {
                    csp.SpawnCollectible();
                }
            }
        }
    }
}
