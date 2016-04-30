using UnityEngine;
using System.Collections.Generic;
using Immersio.Utility;


public class LostWoods : MonoBehaviour 
{
    public float fogStartDist_Min, fogEndDist_Min;
    public float innerRadius, outerRadius;
    public float verticalTransportDistance;
    public GameObject duplicate;
    public LostWoodsPortal entrance, east, west, south, north, solutionExit;
    public float enemyWarpProximity = 16;


    Transform _playerTransform;
    float _fogStartDist_Normal, _fogEndDistNormal;
    float _sunlightIntensityNormal;
    int _tilesWide, _tilesLong;
    Light _sunlight;
    List<Transform> _warpedObjects;
    LostWoodsPortal[] _solution;
    int _solutionIndex;
    Transform _enemiesContainer;


    void Start()
    {
        _playerTransform = CommonObjects.PlayerController_G.transform;
        _enemiesContainer = GameObject.FindGameObjectWithTag("Enemies").transform;

        _fogStartDist_Normal = RenderSettings.fogStartDistance;
        _fogEndDistNormal = RenderSettings.fogEndDistance;
        _sunlight = GameObject.FindGameObjectWithTag("Sunlight").GetComponent<Light>();
        _sunlightIntensityNormal = _sunlight.intensity;

        _solution = new LostWoodsPortal[] { north, west, south, solutionExit };   // Player must move through woods in these directions (in order) to pass through.

        duplicate.SetActive(false);

        _tilesWide = ZeldaVRSettings.Instance.overworldSectorWidthInTiles;
        _tilesLong = ZeldaVRSettings.Instance.overworldSectorHeightInTiles;
    }


	void Update ()
    {
        // TODO: optimize (remove LostWoods when player is far away)

        Vector3 toPlayer = _playerTransform.position - entrance.transform.position;
        toPlayer.y = 0;
        float distanceToPlayer = toPlayer.magnitude;
        if (distanceToPlayer < outerRadius + 1)
        {
            float r = Mathf.Clamp(distanceToPlayer, innerRadius, outerRadius);
            RenderSettings.fogStartDistance = MathHelper.ConvertFromRangeToRange(innerRadius, outerRadius, fogStartDist_Min, _fogStartDist_Normal, r);
            RenderSettings.fogEndDistance = MathHelper.ConvertFromRangeToRange(innerRadius, outerRadius, fogEndDist_Min, _fogEndDistNormal, r);
            _sunlight.intensity = MathHelper.ConvertFromRangeToRange(innerRadius, outerRadius, 0.1f, _sunlightIntensityNormal, r);
        }
	}


    public void OnPlayerEnteredPortal(LostWoodsPortal portal)
    {
        if (CheckSolution(portal))
        {
            WarpBackToActualWoods();
            return;
        }

        if (portal == entrance)
        {
            WarpToDuplicateWoods();
        }
        else if (portal == east)
        {
            WarpBackToActualWoods();
        }
        else if (portal == west)
        {
            WarpObjects(_warpedObjects, new Vector3(_tilesWide, 0, 0));
        }
        else if (portal == north)
        {
            WarpObjects(_warpedObjects, new Vector3(0, 0, -_tilesLong));
        }
        else if (portal == south)
        {
            WarpObjects(_warpedObjects, new Vector3(0, 0, _tilesLong));
        }
    }

    bool CheckSolution(LostWoodsPortal portal)
    {
        if (_solution[_solutionIndex] == portal)
        {
            _solutionIndex++;
            if (_solutionIndex >= _solution.Length)
            {
                _solutionIndex = 0;
                return true;
            }
        }
        else
        {
            if (portal != solutionExit) // solutionExit is a special case.  _solutionIndex should not be reset when player passes through it
            {
                _solutionIndex = 0;
            }
        }

        return false;
    }

    void WarpToDuplicateWoods()
    {
        duplicate.SetActive(true);

        _warpedObjects = GetNearbyObjects(enemyWarpProximity);
        _warpedObjects.Add(_playerTransform);
        WarpObjects(_warpedObjects, new Vector3(0, verticalTransportDistance, 0));
    }

    void WarpBackToActualWoods()
    {
        duplicate.SetActive(false);

        WarpObjects(_warpedObjects, new Vector3(0, -verticalTransportDistance, 0));
    }

    List<Transform> GetNearbyObjects(float proximity)
    {
        List<Transform> nearbyObjects = new List<Transform>();
        float warpProximitySqd = proximity * proximity;

        foreach (Transform child in _enemiesContainer)
        {
            Vector3 toPlayer = _playerTransform.position - child.position;
            float distSqd = Vector3.SqrMagnitude(toPlayer);
            if (distSqd > warpProximitySqd) { continue; }

            nearbyObjects.Add(child);
        }

        foreach (Transform child in EnemyDroppedCollectibles.Instance.transform)
        {
            Vector3 toPlayer = _playerTransform.position - child.position;
            float distSqd = Vector3.SqrMagnitude(toPlayer);
            if (distSqd > warpProximitySqd) { continue; }

            nearbyObjects.Add(child);
        }

        return nearbyObjects;
    }

    void WarpObjects(List<Transform> objects, Vector3 offset)
    {
        foreach (var ob in objects)
        {
            if (ob == null) { continue; }
            ob.position += offset;

            EnemyAI_Random enemyAI_Random = ob.GetComponent<EnemyAI_Random>();
            if (enemyAI_Random != null)
            {
                enemyAI_Random.TargetPosition += offset;
            }
        }
    }

}