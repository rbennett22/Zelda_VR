using Immersio.Utility;
using UnityEngine;

public class EnemyAI_Aquamentus : EnemyAI
{
    public IndexDirection2.DirectionEnum forwardDirection = IndexDirection2.DirectionEnum.Left;


    IndexDirection2 _forwardTileDirection;


    void Start()
    {
        _enemyMove.Mode = EnemyMove.MovementMode.Destination;
        _enemyMove.AlwaysFaceTowardsMoveDirection = false;
        _enemyMove.targetPositionReached_Callback = OnTargetPositionReached;

        _forwardTileDirection = IndexDirection2.FromDirectionEnum(forwardDirection);
        transform.forward = _forwardTileDirection.ToVector3();
        MoveDirection_Tile = _forwardTileDirection;
    }


    void OnTargetPositionReached(EnemyMove sender, Vector3 moveDirection)
    {
        if (moveDirection == _forwardTileDirection.ToVector3())
        {
            Attack();
            MoveDirection_Tile = _forwardTileDirection.Reversed;
        }
        else
        {
            MoveDirection_Tile = _forwardTileDirection;
        }
    }

    void Attack()
    {
        float degreesDelta = Random.Range(1, 30);
        float radiansDelta = degreesDelta * Mathf.PI / 180;

        Weapon_Gun gun = _enemy.weapon as Weapon_Gun;

        Vector3 dir = (Player.Position - gun.Muzzle.position).normalized;
        Vector3 dirL = Vector3.RotateTowards(dir, -dir, -radiansDelta, 999);
        Vector3 dirR = Vector3.RotateTowards(dir, -dir, radiansDelta, 999);

        _enemy.Attack(dir);
        _enemy.Attack(dirL);
        _enemy.Attack(dirR);
    }
}