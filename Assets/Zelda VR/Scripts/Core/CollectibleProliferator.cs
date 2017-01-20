using UnityEngine;

public class CollectibleProliferator : MonoBehaviour, ISpawnManager
{
    public int updateInterval = 1;


    int _removalDist, _removalDistSq;


    OverworldInfo _owInfo;
    OverworldInfo OWInfo
    {
        get
        {
            if (_owInfo == null)
            {
                GameObject g = GameObject.FindGameObjectWithTag(ZeldaTags.OVERWORLD_INFO);
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
        _removalDist = ZeldaVRSettings.Instance.collectibleRemovalDistance;
        _removalDistSq = _removalDist * _removalDist;
    }

    void Start()
    {
        InvokeRepeating("Tick", 0, updateInterval);
    }


    void Tick()
    {
        (this as ISpawnManager).DoUpdate();
    }
    void ISpawnManager.DoUpdate(bool ignoreProxThreshMin = false)
    {
        if (OWInfo == null)
        {
            return;
        }

        Vector3 playerPos = CommonObjects.Player_C.Position;

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

    void ISpawnManager.SetSpawningEnabled(bool value)
    {
        if (OWInfo == null)
        {
            return;
        }

        foreach (Transform sp in OWInfo.collectibleSPs)
        {
            sp.GetComponent<CollectibleSpawnPoint>().SpawningEnabled = value;
        }
    }
}