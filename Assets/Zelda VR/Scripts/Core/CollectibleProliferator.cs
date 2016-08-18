using UnityEngine;

public class CollectibleProliferator : MonoBehaviour
{
    public int updateInterval = 1;


    int _removalDist, _removalDistSq;
    Transform _playerTransform;


    OverworldInfo _owInfo;
    OverworldInfo OWInfo
    {
        get
        {
            if (_owInfo == null)
            {
                GameObject g = GameObject.FindGameObjectWithTag("OverworldInfo");
                if (g != null)
                {
                    _owInfo = g.GetComponent<OverworldInfo>();
                }
            }
            return _owInfo;
        }
    }


    void Awake()
    {
        _playerTransform = CommonObjects.PlayerController_G.transform;

        _removalDist = ZeldaVRSettings.Instance.collectibleRemovalDistance;
        _removalDistSq = _removalDist * _removalDist;
    }

    void Start()
    {
        InvokeRepeating("Tick", 0, updateInterval);
    }


    void Tick()
    {
        if (OWInfo == null)
        {
            return;
        }

        Vector3 playerPos = _playerTransform.position;

        foreach (Transform child in OWInfo.collectibleSPs)
        {
            CollectibleSpawnPoint s = child.GetComponent<CollectibleSpawnPoint>();
            if (s == null) { continue; }

            float distSqr = (s.transform.position - playerPos).sqrMagnitude;
            if (distSqr > _removalDistSq)
            {
                s.DestroySpawnedCollectible();
            }
            else
            {
                if (s.SpawnedCollectible == null && !s.HasBeenCollected)
                {
                    s.SpawnCollectible();
                }
            }
        }
    }
}