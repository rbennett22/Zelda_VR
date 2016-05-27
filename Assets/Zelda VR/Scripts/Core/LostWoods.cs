using Immersio.Utility;
using System.Collections.Generic;
using UnityEngine;
using Uniblocks;

public class LostWoods : MonoBehaviour
{
    static public bool IsWarpedToDuplicate { get; private set; }


    public float fogStartDist_Min, fogEndDist_Min;
    public float innerRadius, outerRadius;

    [SerializeField]
    LostWoodsPortal _eastPortal, _westPortal, _southPortal, _northPortal;
    Dictionary<Index2.Direction, LostWoodsPortal> _portalForDirection;

    [SerializeField]
    LostWoodsPortal _entrance, _escapeExit;
    [SerializeField]
    Index2.Direction _escapeExitDirection, _solutionExitDirection;


    public float verticalTransportDistance;
    public GameObject duplicate;
    public float enemyWarpProximity = 16;


    int _tilesWide, _tilesLong;
    Transform _playerTransform;
    Transform _enemiesContainer;

    float _fogStartDistNormal, _fogEndDistNormal;
    float _sunlightIntensityNormal;
    Light _sunlight;

    List<Transform> _warpedObjects;
    LostWoodsPortal[] _solutionSequence;
    int _solutionSeqIndex;
    bool _hasEnteredLostWoods;


    public Vector3 PlayerPos { get { return _playerTransform.position; } }
    public Vector3 Position { get { return _entrance.transform.position; } }
    public Index2 Sector {
        get {
            Index2 sector;
            CommonObjects.OverworldTileMap.TileIndex_WorldToSector((int)Position.x, (int)Position.z, out sector);
            return sector;
        }
    }


    Chunk LostWoodsChunk { get { return Engine.PositionToChunk(Position).GetComponent<Chunk>(); } }


    void Awake()
    {
        _portalForDirection = new Dictionary<Index2.Direction, LostWoodsPortal>
        {
            { Index2.Direction.Left, _westPortal },
            { Index2.Direction.Right, _eastPortal },
            { Index2.Direction.Up, _northPortal },
            { Index2.Direction.Down, _southPortal }
        };
    }

    void Start()
    {
        TileMap s = CommonObjects.OverworldTileMap;
        _tilesWide = s.TilesWide;
        _tilesLong = s.TilesHigh;

        _playerTransform = CommonObjects.PlayerController_G.transform;
        _enemiesContainer = GameObject.FindGameObjectWithTag("Enemies").transform;

        InitAtmosphere();

        // Player must move through woods in these directions (in order) to pass through.
        _solutionSequence = new LostWoodsPortal[] { _northPortal, _westPortal, _southPortal, _westPortal };     // (right, up, left, up)

        //duplicate.SetActive(false);

        CommonObjects.Player_C.OccupiedSectorChanged += PlayerOccupiedSectorChanged;
    }

    void InitAtmosphere()
    {
        _fogStartDistNormal = RenderSettings.fogStartDistance;
        _fogEndDistNormal = RenderSettings.fogEndDistance;

        _sunlight = GameObject.FindGameObjectWithTag("Sunlight").GetComponent<Light>();
        _sunlightIntensityNormal = _sunlight.intensity;
    }


    void Update()
    {
        Vector3 toPlayer = PlayerPos - Position;
        toPlayer.y = 0;
        float distToPlayer = toPlayer.magnitude;
        if (distToPlayer < outerRadius + 1)
        {
            float r = Mathf.Clamp(distToPlayer, innerRadius, outerRadius);
            RenderSettings.fogStartDistance = MathHelper.ConvertFromRangeToRange(innerRadius, outerRadius, fogStartDist_Min, _fogStartDistNormal, r);
            RenderSettings.fogEndDistance = MathHelper.ConvertFromRangeToRange(innerRadius, outerRadius, fogEndDist_Min, _fogEndDistNormal, r);
            _sunlight.intensity = MathHelper.ConvertFromRangeToRange(innerRadius, outerRadius, 0.1f, _sunlightIntensityNormal, r);
        }
    }


    void PlayerOccupiedSectorChanged(Index2 prevSector, Index2 newSector)
    {
        if (newSector == Sector)
        {
            return;
        }

        //
    }

    void OverrideSectorsWithLostWoodsVoxels(Direction dir, int distance = 1)
    {
        List<Chunk> chunks = new List<Chunk>();
        Index i = LostWoodsChunk.chunkIndex;
        for (int s = 0; s < distance; s++)
        {
            i = i.GetAdjacentIndex(dir);
            Chunk ch = ChunkManager.GetChunkComponent(i);
            chunks.Add(ch);
        }

        OverworldTerrainEngine.Instance.ForceRegenerateTerrain(chunks);
    }


    public void PlayerEnteredPortal(LostWoodsPortal portal)
    {
        if(!_hasEnteredLostWoods)
        {
            if (portal == _entrance)
            {
                OnEnteredLostWoods();
            }
        }

        if (!_hasEnteredLostWoods || SolutionSequenceHasBeCompleted)
        {
            return;
        }

        AddEntryToSequence(portal);
        if (SolutionSequenceHasBeCompleted)
        {
            return;
        }

        if (portal == _escapeExit)
        {
            OnExitedLostWoods();
        }
        else if (portal == _eastPortal)
        {
            WarpObjects(_warpedObjects, new Vector3(-_tilesWide, 0, 0));
        }
        else if (portal == _westPortal)
        {
            WarpObjects(_warpedObjects, new Vector3(_tilesWide, 0, 0));
        }
        else if (portal == _northPortal)
        {
            WarpObjects(_warpedObjects, new Vector3(0, 0, -_tilesLong));
        }
        else if (portal == _southPortal)
        {
            WarpObjects(_warpedObjects, new Vector3(0, 0, _tilesLong));
        }
    }


    #region Sequence Solving

    bool SolutionSequenceHasBeCompleted { get { return _solutionSeqIndex >= _solutionSequence.Length; } }
    LostWoodsPortal NextRequiredEntryInSequence {
        get {
            if(_solutionSeqIndex >= _solutionSequence.Length)
            {
                return null;
            }
            return _solutionSequence[_solutionSeqIndex];
        }
    }

    void AddEntryToSequence(LostWoodsPortal entry)
    {
        if(entry == null)
        {
            return;
        }

        if (entry == NextRequiredEntryInSequence)
        {
            OnCorrectEntryAddedToSequence();
        }
        else
        { 
            OnIncorrectEntryAddedToSequence();
        }
    }

    void OnCorrectEntryAddedToSequence()
    {
        _solutionSeqIndex++;
        if (_solutionSeqIndex >= _solutionSequence.Length)
        {
            OnSequenceCompleted();
        }
    }
    void OnSequenceCompleted()
    {
        OnExitedLostWoods();
    }

    void OnIncorrectEntryAddedToSequence()
    {
        ResetSequence();
    }
    void ResetSequence()
    {
        _solutionSeqIndex = 0;
    }

    #endregion Sequence Solving


    void OnEnteredLostWoods()
    {
        _hasEnteredLostWoods = true;
        _warpedObjects = new List<Transform>() { _playerTransform };
        ResetSequence();

        //WarpToDuplicateWoods();
    }
    void OnExitedLostWoods()
    {
        _hasEnteredLostWoods = false;
        _warpedObjects = null;

        //WarpBackToActualWoods();
    }


    /*void WarpToDuplicateWoods()
    {
        if (IsWarpedToDuplicate)
        {
            return;
        }
        IsWarpedToDuplicate = true;

        _warpedObjects = new List<Transform>() { _playerTransform };
        //_warpedObjects = GetNearbyObjects(enemyWarpProximity);
        //WarpObjects(_warpedObjects, new Vector3(0, verticalTransportDistance, 0));
        //duplicate.SetActive(true);
    }

    void WarpBackToActualWoods()
    {
        if(!IsWarpedToDuplicate)
        {
            return;
        }
        IsWarpedToDuplicate = false;

        //WarpObjects(_warpedObjects, new Vector3(0, -verticalTransportDistance, 0));
        //duplicate.SetActive(false);
        _warpedObjects = null;
    }*/


    List<Transform> GetNearbyObjects(float proximity)
    {
        List<Transform> nearbyObjects = new List<Transform>();
        float warpProximitySqd = proximity * proximity;

        foreach (Transform child in _enemiesContainer)
        {
            Vector3 toPlayer = PlayerPos - child.position;
            float distSqd = Vector3.SqrMagnitude(toPlayer);
            if (distSqd > warpProximitySqd) { continue; }

            nearbyObjects.Add(child);
        }

        foreach (Transform child in EnemyDroppedCollectibles.Instance.transform)
        {
            Vector3 toPlayer = PlayerPos - child.position;
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