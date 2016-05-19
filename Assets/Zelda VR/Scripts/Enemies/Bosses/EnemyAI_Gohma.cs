using Immersio.Utility;
using System.Collections;
using UnityEngine;

public class EnemyAI_Gohma : EnemyAI
{
    public float pathWidth = 5.0f, pathHeight = 2.0f;
    public float chanceToAttack = 0.15f;
    public float chanceToMoveDown = 0.05f;
    public float chanceToOpenEye = 0.10f;
    public float openEyeDuration = 2.0f;


    Rect _moveBounds;


    public bool IsEyeOpen { get { return AnimatorInstance.GetCurrentAnimatorStateInfo(0).IsTag("EyeOpen"); } }
    public bool IsEyeClosed { get { return AnimatorInstance.GetCurrentAnimatorStateInfo(0).IsTag("EyeClosed"); } }


    void Start()
    {
        _enemyMove.Mode = EnemyMove.MovementMode.Destination;
        _enemyMove.AlwaysFaceTowardsMoveDirection = false;
        _enemyMove.targetPositionReached_Callback = OnTargetPositionReached;

        Vector3 p = transform.position;
        _moveBounds = new Rect(
            p.x - pathWidth * 0.5f,
            p.z - pathHeight,
            pathWidth, pathHeight
            );

        MoveDirection = (Extensions.FlipCoin(0.5f)) ? TileDirection.Left : TileDirection.Right;
    }


    void OnTargetPositionReached(EnemyMove sender, Vector3 moveDirection)
    {
        SetNextMoveDirection();

        //print("IsEyeClosed: " + IsEyeClosed);

        if (IsEyeClosed)
        {
            if (Extensions.FlipCoin(chanceToOpenEye))
            {
                StartCoroutine("OpenEye");
            }
        }

        if (Extensions.FlipCoin(chanceToAttack))
        {
            Attack();
        }
    }

    void SetNextMoveDirection()
    {
        Vector3 p = transform.position;

        TileDirection newMoveDir = TileDirection.Zero;

        if (MoveDirection.IsDown())
        {
            if (p.z <= _moveBounds.yMin)
            {
                newMoveDir = TileDirection.Up;
            }
        }
        else if (MoveDirection.IsUp())
        {
            if (p.z >= _moveBounds.yMax)
            {
                newMoveDir = (Extensions.FlipCoin(0.5f)) ? TileDirection.Left : TileDirection.Right;
            }
        }
        else
        {
            if (Extensions.FlipCoin(chanceToMoveDown))
            {
                newMoveDir = TileDirection.Down;
            }
            else
            {
                if (MoveDirection.IsRight())
                {
                    if (p.x >= _moveBounds.xMax)
                    {
                        newMoveDir = TileDirection.Left;
                    }
                }
                else if (MoveDirection.IsLeft())
                {
                    if (p.x <= _moveBounds.xMin)
                    {
                        newMoveDir = TileDirection.Right;
                    }
                }
            }
        }

        if (newMoveDir.IsZero())
        {
            newMoveDir = MoveDirection;
        }

        MoveDirection = newMoveDir;
    }

    void Attack()
    {
        _enemy.Attack(ToPlayer);
    }


    IEnumerator OpenEye()
    {
        //print(" ~ OpenEye ~");

        AnimatorInstance.SetTrigger("OpenEye");

        yield return new WaitForSeconds(openEyeDuration);

        CloseEye();
    }

    void CloseEye()
    {
        AnimatorInstance.SetTrigger("CloseEye");
    }
}