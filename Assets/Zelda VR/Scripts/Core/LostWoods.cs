using Immersio.Utility;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

public class LostWoods : MonoBehaviour
{
    const int OVERRIDE_SECTOR_DISTANCE = 3;
    const float SUNLIGHT_INTENSITY_MIN = 0.1f;


    public float fogDensity_Max;
    public float innerRadius, outerRadius;

    [SerializeField]
    LostWoodsPortal _leftPortal, _rightPortal, _downPortal, _upPortal;
    Dictionary<IndexDirection2, LostWoodsPortal> _directionToPortal;
    LostWoodsPortal DirectionToPortal(IndexDirection2 dir) { return _directionToPortal[dir]; }
    IndexDirection2 PortalToDirection(LostWoodsPortal portal) { return _directionToPortal.FirstOrDefault(p => p.Value == portal).Key; }

    [SerializeField]
    LostWoodsPortal _entrance, _escapeExit;
    [SerializeField]
    IndexDirection2.DirectionEnum _escapeExitDirection;
    public IndexDirection2 EscapeExitDirection { get { return IndexDirection2.FromDirectionEnum(_escapeExitDirection); } }
    public IndexDirection2 SolutionExitDirection { get { return PortalToDirection(_solutionSequence[_solutionSequence.Length - 1]);  } }


    int _sectorWidth, _sectorHeight;
    Transform _playerTransform;
    Transform _enemiesContainer;

    GlobalFog _fog;
    float _fogDensity_Normal;
    float _sunlightIntensityNormal;
    Light _sunlight;

    List<Transform> _warpedObjects;
    LostWoodsPortal[] _solutionSequence;
    int _solutionSeqIndex;
    bool _hasEnteredLostWoods;


    Vector3 PlayerPos { get { return _playerTransform.position; } }
    Vector3 Position { get { return _entrance.transform.position; } }
    Index2 Sector {
        get {
            Index2 sector;
            CommonObjects.OverworldTileMap.TileIndex_WorldToSector((int)Position.x, (int)Position.z, out sector);
            return sector;
        }
    }

    Dictionary<IndexDirection2, Index2> _neighborSectors;
    Dictionary<IndexDirection2, Index2> NeighborSectors {
        get {
            if (_neighborSectors == null)
            {
                _neighborSectors = new Dictionary<IndexDirection2, Index2>();
                foreach (IndexDirection2 dir in IndexDirection2.AllValidNonZeroDirections)
                {
                    _neighborSectors.Add(dir, Sector + dir);
                }
            }
            return _neighborSectors;
        }
    }
    bool IsNeighborSector(Index2 sector) { return NeighborSectors.ContainsValue(sector); }


    void Awake()
    {
        _directionToPortal = new Dictionary<IndexDirection2, LostWoodsPortal>
        {
            { IndexDirection2.left, _leftPortal },
            { IndexDirection2.right, _rightPortal },
            { IndexDirection2.down, _downPortal },
            { IndexDirection2.up, _upPortal }
        };
    }

    void Start()
    {
        TileMap s = CommonObjects.OverworldTileMap;
        _sectorWidth = s.SectorWidthInTiles;
        _sectorHeight = s.SectorHeightInTiles;

        _playerTransform = CommonObjects.PlayerController_G.transform;
        _enemiesContainer = GameObject.FindGameObjectWithTag("Enemies").transform;

        InitAtmosphere();

        // Player must move through woods in these directions (in order) to pass through.
        _solutionSequence = new LostWoodsPortal[] { _upPortal, _leftPortal, _downPortal, _leftPortal };     // (+z, -x, -z, -x)

        CommonObjects.Player_C.OccupiedSectorChanged += PlayerEnteredNewSector;
    }

    void InitAtmosphere()
    {
        _fog = FindObjectOfType<GlobalFog>();
        _fogDensity_Normal = _fog.heightDensity;

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
            float d = Mathf.Clamp(distToPlayer, innerRadius, outerRadius);
            float t = Mathf.InverseLerp(innerRadius, outerRadius, d);
            UpdateFog(t);
            UpdateSunlight(t);
        }
    }
    void UpdateFog(float t)
    {
        _fog.heightDensity = Mathf.Lerp(_fogDensity_Normal, fogDensity_Max, 1 - t);
    }
    void UpdateSunlight(float t)
    {
        _sunlight.intensity = Mathf.Lerp(SUNLIGHT_INTENSITY_MIN, _sunlightIntensityNormal, t);
    }


    IndexDirection2 DirectionForNeighborSector(Index2 sector)
    {
        return NeighborSectors.FirstOrDefault(s => s.Value == sector).Key;
    }

    void PlayerEnteredNewSector(Index2 prevSector, Index2 newSector)
    {
        if (!WorldInfo.Instance.IsOverworld)
        {
            return;
        }
        if (_hasEnteredLostWoods)
        {
            return;
        }

        if (IsNeighborSector(newSector))
        {
            PlayerEnteredNeighborSector(newSector);
        }
        else if (IsNeighborSector(prevSector))
        {
            PlayerExitedNeighborSector(newSector);
        }
    }
    void PlayerEnteredNeighborSector(Index2 sector)
    {
        IndexDirection2 sectorDir = DirectionForNeighborSector(sector);
        IndexDirection2[] dirs = IndexDirection2.AllValidNonZeroDirections.Where(d => d != sectorDir).ToArray();

        OverrideSectorsWithLostWoods(dirs, true, OVERRIDE_SECTOR_DISTANCE);
    }
    void PlayerExitedNeighborSector(Index2 newSector)
    {
        if (newSector == Sector)
        {
            return;
        }

        OverrideSectorsWithLostWoods(IndexDirection2.AllValidNonZeroDirections, false);
    }

    void OverrideSectorsWithLostWoods(IndexDirection2[] dirs, bool doOverride, int distance = 1)
    {
        foreach (IndexDirection2 d in dirs)
        {
            OverrideSectorsWithLostWoods(d, doOverride, distance);
        }
    }
    void OverrideSectorsWithLostWoods(IndexDirection2 dir, bool doOverride, int distance = 1)
    {
        if (dir == EscapeExitDirection) { return; }

        Index2 s = Sector;
        for (int i = 0; i < distance; i++)
        {
            s = s + dir;

            OverrideSectorWithLostWoodsVoxels(s, doOverride);

            if (doOverride)
            {
                DisableEnemySpawningInSector(s);
            }
            else
            {
                EnableEnemySpawningInSector(s);
            }
        }
    }  
    void OverrideSectorWithLostWoodsVoxels(Index2 sector, bool doOverride)
    {
        OverworldTerrainEngine eng = OverworldTerrainEngine.Instance;

        OverworldChunk ch = eng.GetChunkForSector(sector) as OverworldChunk;
        OverrideChunkWithLostWoodsVoxels(ch, doOverride);

        OverworldChunk ch_down = ch.neighborChunks[1] as OverworldChunk;        // TODO: Better methods for getting neighbors
        OverrideChunkWithLostWoodsVoxels(ch_down, doOverride);
    }
    void OverrideChunkWithLostWoodsVoxels(OverworldChunk ch, bool doOverride)
    {
        if (ch == null)
        {
            return;
        }
        if (ch.useOverridingSector == doOverride
            && ch.overridingSector == this.Sector)
        {
            return;
        }
        ch.useOverridingSector = doOverride;
        ch.overridingSector = this.Sector;

        OverworldTerrainEngine.Instance.ForceRegenerateTerrain(ch);
    }

    void DisableEnemySpawningInSector(Index2 sector, bool destroyEnemies = true)
    {
        if (destroyEnemies)
        {
            foreach (Enemy e in Actor.FindObjectsInSector<Enemy>(Sector))
            {
                Destroy(e.gameObject);
            }
        }
        foreach (EnemySpawnPoint sp in Actor.FindObjectsInSector<EnemySpawnPoint>(sector))
        {
            sp.autoSpawn = false;
        }
    }
    void EnableEnemySpawningInSector(Index2 sector)
    {
        foreach (EnemySpawnPoint sp in Actor.FindObjectsInSector<EnemySpawnPoint>(sector))
        {
            sp.autoSpawn = true;
        }
    }


    public void PlayerEnteredPortal(LostWoodsPortal portal)
    {
        if (portal == _entrance)
        {
            if (!_hasEnteredLostWoods)
            {
                OnEnteredLostWoods();
            }
            return;
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
        else if (portal == _rightPortal)
        {
            WarpObjects(_warpedObjects, new Vector3(-_sectorWidth, 0, 0));
        }
        else if (portal == _leftPortal)
        {
            WarpObjects(_warpedObjects, new Vector3(_sectorWidth, 0, 0));
        }
        else if (portal == _upPortal)
        {
            WarpObjects(_warpedObjects, new Vector3(0, 0, -_sectorHeight));
        }
        else if (portal == _downPortal)
        {
            WarpObjects(_warpedObjects, new Vector3(0, 0, _sectorHeight));
        }
    }

    void OnEnteredLostWoods()
    {
        _hasEnteredLostWoods = true;
        _warpedObjects = new List<Transform>() { _playerTransform };

        ResetSequence();
        OverrideSectorsWithLostWoods(IndexDirection2.AllValidNonZeroDirections, true, OVERRIDE_SECTOR_DISTANCE);
    }
    void OnExitedLostWoods()
    {
        OverrideSectorsWithLostWoods(IndexDirection2.AllValidNonZeroDirections, false);

        _hasEnteredLostWoods = false;
        _warpedObjects = null;
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
        if (entry == null)
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

        //print("!!!!   _solutionSeqIndex: " + _solutionSeqIndex);
    }

    void OnCorrectEntryAddedToSequence()
    {
        _solutionSeqIndex++;

        if (_solutionSeqIndex == _solutionSequence.Length - 1)
        {
            OnSequenceIsOneAwayFromCompleted();
        }
        if (_solutionSeqIndex == _solutionSequence.Length)
        {
            OnSequenceCompleted();
        }
    }
    void OnSequenceIsOneAwayFromCompleted()
    {
        OverrideSectorsWithLostWoods(SolutionExitDirection, false);
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
        }
    }
}