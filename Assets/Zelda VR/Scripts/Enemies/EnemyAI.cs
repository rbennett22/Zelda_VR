using Immersio.Utility;
using UnityEngine;

[RequireComponent(typeof(Enemy))]

public class EnemyAI : MonoBehaviour
{
    protected const float MAX_DISTANCE_PLAYER_CAN_BE_SEEN = 16.0f;
    protected const float DEFAULT_OBSTRUCTION_FEELER_LENGTH = 1.45f;
    protected const float EPSILON = 0.001f;


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


    public IndexDirection2 MoveDirection
    {
        get
        {
            return new IndexDirection2(_enemyMove.MoveDirection);
        }
        protected set
        {
            Vector3 moveDir = value.ToVector3();

            Vector3 targetPos;
            targetPos.x = (int)(_enemy.TileX + moveDir.x + EPSILON) + TileMap.BLOCK_OFFSET_XZ;
            targetPos.y = transform.position.y;
            targetPos.z = (int)(_enemy.TileZ + moveDir.z + EPSILON) + TileMap.BLOCK_OFFSET_XZ;

            _enemyMove.TargetPosition = targetPos;
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


    protected bool IsPlayerInSight(float maxDistance, out Vector3 direction)
    {
        Vector3[] directionsToCheck = {
            Vector3.left, Vector3.right, Vector3.forward, Vector3.back
        };
        foreach (Vector3 dir in directionsToCheck)
        {
            direction = dir;
            if (IsPlayerInLineOfSight(direction, maxDistance))
            {
                return true;
            }
        }

        direction = Vector3.zero;
        return false;
    }
    protected bool IsPlayerInLineOfSight(Vector3 direction, float maxDistance)
    {
        LayerMask mask = Extensions.GetLayerMaskIncludingLayers("Link");
        return Physics.Raycast(transform.position, direction, maxDistance, mask);
    }

    protected bool CanMoveInDirection_Overworld(IndexDirection2 dir)
    {
        Index2 adjacentTile = _enemy.Tile + dir;
        return CanOccupyTile_Overworld(adjacentTile);
    }
    protected bool CanOccupyTile_Overworld(Index2 tile)
    {
        int tileCode = CommonObjects.OverworldTileMap.Tile(tile);
        return TileMapData.IsTileCodeValid(tileCode) && TileInfo.IsTilePassable(tileCode);
    }

    protected bool DetectObstructions(Vector3 direction, float distance)
    {
        const float SHORTEST_BLOCK_HEIGHT = 0.2f;

        Vector3 from = transform.position;
        from.y = WorldOffsetY + SHORTEST_BLOCK_HEIGHT - EPSILON;   // This ensures that short blocks don't go undetected
        Vector3 to = from + distance * direction;

        return !CanMoveFromTo(from, to);
    }

    virtual protected bool CanMoveFromTo(Vector3 from, Vector3 to)
    {
        Vector3 dir = to - from;
        LayerMask mask = Extensions.GetLayerMaskIncludingLayers("Blocks", "Walls", "InvisibleBlocks");
        return Physics.Raycast(from, dir, dir.magnitude, mask);
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
        Rect b = _enemy.Boundary;

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


    protected void SetEnemyPositionXZToTile(Index2 tile)
    {
        SetEnemyPositionXZToTile(tile.x, tile.y);
    }
    protected void SetEnemyPositionXZToTile(int tileX, int tileY)
    {
        transform.SetX(tileX + TileMap.BLOCK_OFFSET_XZ);
        transform.SetZ(tileY + TileMap.BLOCK_OFFSET_XZ);
    }
}