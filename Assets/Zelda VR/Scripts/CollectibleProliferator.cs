using UnityEngine;

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
        InvokeRepeating("Tick", 0, updateInterval_ms * 0.001f);
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