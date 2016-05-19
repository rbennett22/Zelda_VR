using Immersio.Utility;
using UnityEngine;

public class EnemyAI_Aquamentus : EnemyAI
{
    public TileDirection.Direction forwardDirection = TileDirection.Direction.Left;


    TileDirection _forwardTileDirection;


    void Start()
    {
        _enemyMove.Mode = EnemyMove.MovementMode.Destination;
        _enemyMove.AlwaysFaceTowardsMoveDirection = false;
        _enemyMove.targetPositionReached_Callback = OnTargetPositionReached;

        _forwardTileDirection = TileDirection.TileDirectionForDirection(forwardDirection);
        transform.forward = _forwardTileDirection.ToVector3();
        MoveDirection = _forwardTileDirection;
    }


    void OnTargetPositionReached(EnemyMove sender, Vector3 moveDirection)
    {
        if (moveDirection == _forwardTileDirection.ToVector3())
        {
            Attack();
            MoveDirection = _forwardTileDirection.Reversed;
        }
        else
        {
            MoveDirection = _forwardTileDirection;
        }
    }

    void Attack()
    {
        float degreesDelta = Random.Range(1, 30);
        float radiansDelta = degreesDelta * Mathf.PI / 180;

        Vector3 dir = ToPlayer;
        Vector3 dirL = Vector3.RotateTowards(dir, -dir, -radiansDelta, 999);
        Vector3 dirR = Vector3.RotateTowards(dir, -dir, radiansDelta, 999);

        _enemy.Attack(dir);
        _enemy.Attack(dirL);
        _enemy.Attack(dirR);
    }
}