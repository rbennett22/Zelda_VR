using Immersio.Utility;
using UnityEngine;

[RequireComponent(typeof(Enemy))]

public class EnemyAI : MonoBehaviour
{
    public const string PLAYER_LAYER_NAME = "Link";     // TODO


    protected const float MAX_DISTANCE_PLAYER_CAN_BE_SEEN = 16.0f;
    protected const float DEFAULT_OBSTRUCTION_FEELER_LENGTH = 1.45f;
    protected const float EPSILON = 0.001f;


    virtual public float Radius { get { return 0.5f; } }       // TODO

    public Rect Boundary { get; set; }    
    protected Rect GetBoundaryDeflatedByRadius()
    {
        Rect b = Boundary;
        b.xMin += Radius;
        b.xMax -= Radius;
        b.yMin += Radius;
        b.yMax -= Radius;
        return b;
    }

    protected bool DoesBoundaryAllowPosition(Vector3 p)
    {
        return DoesBoundaryAllowPosition(new Vector2(p.x, p.z));
    }
    protected bool DoesBoundaryAllowPosition(Vector2 p)
    {
        return GetBoundaryDeflatedByRadius().Contains(p);
    }

    protected Vector3 GetRandomAllowedPositionInsideBoundary()
    {
        Rect b = GetBoundaryDeflatedByRadius();
        float x = Random.Range(b.xMin, b.xMax);
        float z = Random.Range(b.yMin, b.yMax);
        return new Vector3(x, 0, z);
    }


    protected bool _doUpdate = true;

    protected Enemy _enemy;
    protected EnemyMove _enemyMove;
    protected HealthController _healthController;


    protected float WorldOffsetY { get { return WorldInfo.Instance.WorldOffset.y; } }

    protected Player Player { get { return _enemy.Player; } }
    protected Vector3 DirectionToPlayer { get { return (Player.Position - transform.position).normalized; } }

    protected Animator AnimatorInstance { get { return _enemy.enemyAnim.AnimatorComponent; } }

    protected bool IsPreoccupied { get { return _enemy.IsPreoccupied; } }


    protected void FacePlayer()
    {
        transform.forward = DirectionToPlayer;
    }


    public IndexDirection2 MoveDirection_Tile
    {
        get { return new IndexDirection2(_enemyMove.MoveDirection); }
        protected set
        {
            Index2 targetTile = _enemy.Tile + value;

            Vector3 targetPos = targetTile.ToVector3() + WorldInfo.Instance.WorldOffset + TileMap.TileExtents;
            targetPos.y = transform.position.y;

            _enemyMove.TargetPosition = targetPos;
        }
    }
    public Vector3 MoveDirection
    {
        get { return _enemyMove.MoveDirection; }
        protected set
        {
            _enemyMove.MoveDirection = value;
            _enemyMove.TargetPosition = transform.position + value;
        }
    }

    public Vector3 TargetPosition
    {
        get { return _enemyMove.TargetPosition; }
        set { _enemyMove.TargetPosition = value; }
    }


    virtual protected void Awake()
    {
        _enemy = GetComponent<Enemy>();
        _enemyMove = GetComponent<EnemyMove>();
        _healthController = GetComponent<HealthController>();
    }


    virtual public void DisableAI()
    {
        _doUpdate = false;
        if (_enemyMove != null)
        {
            _enemyMove.enabled = false;
        }
    }
    virtual public void EnableAI()
    {
        _doUpdate = true;
        if (_enemyMove != null)
        {
            _enemyMove.enabled = true;
        }
    }


    virtual protected bool IsPlayerInSight(float maxDistance = MAX_DISTANCE_PLAYER_CAN_BE_SEEN)
    {
        IndexDirection2 direction;
        return IsPlayerInSight(maxDistance, out direction);
    }
    virtual protected bool IsPlayerInSight(float maxDistance, out IndexDirection2 direction)
    {
        foreach (IndexDirection2 dir in IndexDirection2.AllValidNonZeroDirections)
        {
            if (IsPlayerInLineOfSight(maxDistance, dir.ToVector3()))
            {
                direction = dir;
                return true;
            }
        }

        direction = IndexDirection2.zero;
        return false;
    }
    virtual protected bool IsPlayerInLineOfSight(float maxDistance, Vector3 direction)
    {
        if (WorldInfo.Instance.IsInDungeon)
        {
            // We ensure Enemy is in the same DungeonRoom as Player
            DungeonRoom playerDR = DungeonRoom.GetRoomForPosition(Player.Position);
            if (_enemy.DungeonRoomRef != playerDR && playerDR != null)
            {
                return false;
            }
        }

        LayerMask mask = Extensions.GetLayerMaskIncludingLayers(PLAYER_LAYER_NAME, "Walls", "Blocks");
        RaycastHit hitInfo;
        bool didHit = Physics.Raycast(transform.position, direction, out hitInfo, maxDistance, mask);
        
        return didHit ? CommonObjects.IsPlayer(hitInfo.collider.gameObject) : false;
    }

    protected bool CanMoveInDirection_Overworld(Vector3 dir)
    {
        Vector3 newPos = _enemy.Position + dir * 2 * Radius;
        Index2 newTile = Actor.PositionToTile(newPos);
        return CanOccupyTile_Overworld(newTile);
    }
    protected bool CanMoveInDirection_Overworld(IndexDirection2 dir)
    {
        Index2 adjacentTile = _enemy.Tile + dir;
        return CanOccupyTile_Overworld(adjacentTile);
    }
    protected bool CanOccupyTile_Overworld(Index2 tile)
    {
        int tileCode = CommonObjects.OverworldTileMap.TryGetTile(tile);
        bool canOccupy = TileMapData.IsTileCodeValid(tileCode) && TileInfo.IsTilePassable(tileCode);

        Vector3 from = transform.position;
        Vector3 to = tile.ToVector3();
        to.y = from.y;
        DrawDebugLine(from, to, canOccupy);

        return canOccupy;
    }

    protected bool DetectObstructions(Index2 tile)
    {
        Vector2 p = Actor.TileToPosition_Center(tile);
        Vector3 pp = new Vector3(p.x, WorldOffsetY + Radius, p.y);
        return CanMoveFromTo(pp, pp + 0.001f * Vector3.one);
    }
    protected bool DetectObstructions(IndexDirection2 dir, float distance)
    {
        return DetectObstructions(dir.ToVector3(), distance);
    }
    protected bool DetectObstructions(Vector3 direction, float distance)
    {
        const float SHORTEST_BLOCK_HEIGHT = 0.2f;

        Vector3 from = transform.position;
        if (from.y >= 0)    // This check is necessary for Keese in SubDungeons to detect obstructions using their actual y position  (TODO)
        {
            from.y = WorldOffsetY + SHORTEST_BLOCK_HEIGHT - EPSILON;   // This ensures that short blocks don't go undetected
        }
        Vector3 to = from + distance * direction;

        bool canMove = CanMoveFromTo(from, to);

        DrawDebugLine(from, to, canMove);

        return !canMove;
    }

    virtual protected bool CanMoveFromTo(Vector3 from, Vector3 to)
    {
        Vector3 dir = to - from;
        Ray ray = new Ray(from, dir);
        LayerMask mask = Extensions.GetLayerMaskIncludingLayers("Blocks", "Walls", "InvisibleBlocks", "Enemies");
        return ! Physics.SphereCast(ray, Radius * 0.99f, dir.magnitude, mask, QueryTriggerInteraction.UseGlobal);
    }

    protected IndexDirection2 EnforceBoundary(IndexDirection2 desiredMoveDirection)
    {
        Vector3 vec = desiredMoveDirection.ToVector3();
        vec = EnforceBoundary(vec);
        return new IndexDirection2(vec);
    }
    protected Vector3 EnforceBoundary(Vector3 desiredMoveDirection)
    {
        Vector3 pos = transform.position;
        Rect b = Boundary;

        if (pos.x < b.xMin)
        {
            desiredMoveDirection = Vector3.right;
        }
        else if (pos.x > b.xMax)
        {
            desiredMoveDirection = Vector3.left;
        }
        else if (pos.z < b.yMin)
        {
            desiredMoveDirection = Vector3.forward;
        }
        else if (pos.z > b.yMax)
        {
            desiredMoveDirection = Vector3.back;
        }

        return desiredMoveDirection;
    }


    void DrawDebugLine(Vector3 from, Vector3 to, bool b)
    {
        Color c = b ? Color.green : Color.red;
        Debug.DrawLine(from, to, c, 0.5f);
    }
}