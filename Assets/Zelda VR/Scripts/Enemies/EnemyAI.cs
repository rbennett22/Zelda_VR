using UnityEngine;
using Immersio.Utility;

[RequireComponent(typeof(Enemy))]
[RequireComponent(typeof(EnemyMove))]

public class EnemyAI : MonoBehaviour
{
    protected const float TILE_EXTENT = 0.5f;
    protected const float EPSILON = 0.001f;


    protected bool _doUpdate = true;

    protected Enemy _enemy;
    protected EnemyMove _enemyMove;
    protected HealthController _healthController;


    protected float GroundPosY { get { return WorldInfo.Instance.GroundPosY; } }

    protected Vector3 ToPlayer { get { return (CommonObjects.PlayerController_G.transform.position - transform.position).normalized; } }

    protected Animator AnimatorInstance { get { return _enemy.enemyAnim.AnimatorInstance; } }

    protected bool IsPreoccupied { get { return _enemy.IsPreoccupied; } }


    public Vector3 MoveDirection
    {
        get { return _enemyMove.MoveDirection; }
        protected set
        {
            Vector3 moveDir = value.normalized;

            Vector3 targetPos;
            targetPos.x = (int)(_enemy.TileX + moveDir.x + EPSILON) + TILE_EXTENT;
            targetPos.y = transform.position.y;
            targetPos.z = (int)(_enemy.TileZ + moveDir.z + EPSILON) + TILE_EXTENT;

            _enemyMove.TargetPosition = targetPos;
        }
    }
    /*public TileDirection MoveDirection
    {
        get
        {
            Vector3 m = _enemyMove.MoveDirection;
            return new TileDirection(m.x, m.z);
        }
        protected set
        {
            Vector3 moveDir = new Vector3(value.X, 0, value.Y);

            Vector3 targetPos;
            targetPos.x = (int)(_enemy.TileX + moveDir.x + EPSILON) + TILE_EXTENT;
            targetPos.y = transform.position.y;
            targetPos.z = (int)(_enemy.TileZ + moveDir.z + EPSILON) + TILE_EXTENT;

            _enemyMove.TargetPosition = targetPos;
        }
    }*/

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


    protected Vector3 EnforceBoundary(Vector3 desiredMoveDirection)
    {
        if (transform.position.x < _enemy.Boundary.xMin)
        {
            desiredMoveDirection = new Vector3(1, 0, 0);
        }
        else if (transform.position.x > _enemy.Boundary.xMax)
        {
            desiredMoveDirection = new Vector3(-1, 0, 0);
        }
        else if (transform.position.z < _enemy.Boundary.yMin)
        {
            desiredMoveDirection = new Vector3(0, 0, 1);
        }
        else if (transform.position.z > _enemy.Boundary.yMax)
        {
            desiredMoveDirection = new Vector3(0, 0, -1);
        }

        return desiredMoveDirection;
    }


    protected void SetEnemyPositionXZToTile(Index2 tile)
    {
        SetEnemyPositionXZToTile(tile.x, tile.y);
    }
    protected void SetEnemyPositionXZToTile(int tileX, int tileY)
    {
        transform.SetX(tileX + TILE_EXTENT);
        transform.SetZ(tileY + TILE_EXTENT);
    }
}