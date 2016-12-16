using Immersio.Utility;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

using Object = System.Object;

public class LostSector : MonoBehaviour
{
    const int OVERRIDE_SECTOR_DISTANCE = 3;


    [SerializeField]
    IndexDirection2.DirectionEnum[] _solution;    // Player must move through this sector in these directions (in order) to pass through. 

    [SerializeField]
    LostSectorPortal _leftPortal, _rightPortal, _downPortal, _upPortal;
    Dictionary<IndexDirection2, LostSectorPortal> _directionToPortal;
    LostSectorPortal DirectionToPortal(IndexDirection2 dir) { return _directionToPortal[dir]; }
    IndexDirection2 PortalToDirection(LostSectorPortal portal) { return _directionToPortal.FirstOrDefault(p => p.Value == portal).Key; }

    [SerializeField]
    LostSectorPortal _entrance, _escapeExit;
    [SerializeField]
    IndexDirection2.DirectionEnum _escapeExitDirection;
    public IndexDirection2 EscapeExitDirection { get { return IndexDirection2.FromDirectionEnum(_escapeExitDirection); } }
    public IndexDirection2 SolutionExitDirection { get { return PortalToDirection(_passwordLock.GetPasswordEntryAt(_passwordLock.PasswordLength - 1) as LostSectorPortal);  } }


    int _sectorWidth, _sectorHeight;
    Transform _playerTransform;
    Transform _enemiesContainer;

    List<Transform> _warpedObjects;

    PasswordLock _passwordLock;     // Helps with logic that determines if player is moving through lost sector in the correct "secret" order
    bool _hasEnteredLostSector;


    public Vector3 PlayerPos { get { return _playerTransform.position; } }
    public Vector3 Position { get { return _entrance.transform.position; } }
    public Index2 Sector {
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
        _directionToPortal = new Dictionary<IndexDirection2, LostSectorPortal>
        {
            { IndexDirection2.left, _leftPortal },
            { IndexDirection2.right, _rightPortal },
            { IndexDirection2.down, _downPortal },
            { IndexDirection2.up, _upPortal }
        };

        InitPasswordLock();
    }

    void Start()
    {
        TileMap s = CommonObjects.OverworldTileMap;
        _sectorWidth = s.SectorWidthInTiles;
        _sectorHeight = s.SectorHeightInTiles;

        _playerTransform = CommonObjects.PlayerController_G.transform;
        _enemiesContainer = GameObject.FindGameObjectWithTag("Enemies").transform;

        CommonObjects.Player_C.OccupiedSectorChanged += PlayerEnteredNewSector;
    }

    void InitPasswordLock()
    {
        List<LostSectorPortal> solutionPortals = new List<LostSectorPortal>();
        foreach (IndexDirection2.DirectionEnum d in _solution)
        {
            IndexDirection2 dir = IndexDirection2.FromDirectionEnum(d);
            LostSectorPortal portal = DirectionToPortal(dir);
            solutionPortals.Add(portal);
        }
        _passwordLock = new PasswordLock(solutionPortals.ToArray());

        _passwordLock.CorrectEntryCallback += OnCorrectEntryAddedToSequence;
        _passwordLock.IncorrectEntryCallback += OnIncorrectEntryAddedToSequence;
        _passwordLock.CorrectPasswordEnteredCallback += OnSequenceCompleted;
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
        if (_hasEnteredLostSector)
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

        OverrideSectorsWithThisSector(dirs, true);
    }
    void PlayerExitedNeighborSector(Index2 newSector)
    {
        if (newSector == Sector)
        {
            return;
        }

        OverrideSectorsWithThisSector(IndexDirection2.AllValidNonZeroDirections, false);
    }

    void OverrideSectorsWithThisSector(IndexDirection2[] dirs, bool doOverride)
    {
        foreach (IndexDirection2 d in dirs)
        {
            OverrideSectorsWithThisSector(d, doOverride);
        }
    }
    void OverrideSectorsWithThisSector(IndexDirection2 dir, bool doOverride)
    {
        if (dir == EscapeExitDirection) { return; }

        Index2 s = Sector;
        for (int i = 0; i < OVERRIDE_SECTOR_DISTANCE; i++)
        {
            s = s + dir;

            OverrideSectorWithThisSectorsVoxels(s, doOverride);

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
    void OverrideSectorWithThisSectorsVoxels(Index2 sector, bool doOverride)
    {
        OverworldTerrainEngine eng = OverworldTerrainEngine.Instance;

        OverworldChunk ch = eng.GetChunkForSector(sector) as OverworldChunk;
        if (ch != null)
        {
            OverrideChunkWithThisSectorsVoxels(ch, doOverride);

            OverworldChunk ch_down = ch.GetChunkDirectlyBelowThisChunk() as OverworldChunk;        // TODO: Better methods for getting neighbors
            if (ch_down != null)
            {
                OverrideChunkWithThisSectorsVoxels(ch_down, doOverride);
            }
        }
    }
    void OverrideChunkWithThisSectorsVoxels(OverworldChunk ch, bool doOverride)
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


    public void PlayerEnteredPortal(LostSectorPortal portal)
    {
        if (portal == _entrance)
        {
            if (!_hasEnteredLostSector)
            {
                OnEnteredLostSector();
            }
            return;
        }

        if (!_hasEnteredLostSector || HasSolutionSequenceBeenCompleted)
        {
            return;
        }

        AddEntryToSequence(portal);
        if (HasSolutionSequenceBeenCompleted)
        {
            return;
        }

        if (portal == _escapeExit)
        {
            OnExitedLostSector();
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

    void OnEnteredLostSector()
    {
        _hasEnteredLostSector = true;
        _warpedObjects = new List<Transform>() { _playerTransform };

        ResetSequence();
        OverrideSectorsWithThisSector(IndexDirection2.AllValidNonZeroDirections, true);
    }
    void OnExitedLostSector()
    {
        OverrideSectorsWithThisSector(IndexDirection2.AllValidNonZeroDirections, false);

        _hasEnteredLostSector = false;
        _warpedObjects = null;
    }


    #region Sequence Solving

    void AddEntryToSequence(LostSectorPortal entry) { _passwordLock.InputNextEntry(entry); }
    bool HasSolutionSequenceBeenCompleted { get { return _passwordLock.HasCorrectPasswordBeenEntered(); } }
    
    void OnCorrectEntryAddedToSequence(PasswordLock sender, Object entry)
    {
        if (sender != _passwordLock)
        {
            return;
        }

        if (_passwordLock.RemainingEntriesNeeded == 1)
        {
            OnSequenceIsOneAwayFromCompleted();
        }
    }
    void OnSequenceIsOneAwayFromCompleted()
    {
        OverrideSectorsWithThisSector(SolutionExitDirection, false);
    }
    void OnSequenceCompleted(PasswordLock sender)
    {
        if (sender != _passwordLock)
        {
            return;
        }

        PlaySecretSound();

        OnExitedLostSector();
    }

    void OnIncorrectEntryAddedToSequence(PasswordLock sender, Object entry)
    {
        OverrideSectorsWithThisSector(IndexDirection2.AllValidNonZeroDirections, true);
    }

    void ResetSequence()
    {
        _passwordLock.ResetEntered();
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

    void PlaySecretSound()
    {
        SoundFx.Instance.PlayOneShot(SoundFx.Instance.secret);
    }
}