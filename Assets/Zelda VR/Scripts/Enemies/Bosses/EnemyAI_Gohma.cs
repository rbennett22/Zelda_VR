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


    public bool IsEyeOpen { get { return AnimatorInstance.GetCurrentAnimatorStateInfo(0).IsTag("EyeOpen"); } }
    public bool IsEyeClosed { get { return AnimatorInstance.GetCurrentAnimatorStateInfo(0).IsTag("EyeClosed"); } }


    void Start()
    {
        _enemyMove.Mode = EnemyMove.MovementMode.Destination;
        _enemyMove.AlwaysFaceTowardsMoveDirection = false;
        _enemyMove.targetPositionReached_Callback += OnTargetPositionReached;

        Vector3 p = transform.position;
        Boundary = new Rect(
            p.x - pathWidth * 0.5f,
            p.z - pathHeight,
            pathWidth, pathHeight
            );

        MoveDirection_Tile = (Extensions.FlipCoin(0.5f)) ? IndexDirection2.left : IndexDirection2.right;
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

        IndexDirection2 newMoveDir = IndexDirection2.zero;

        if (MoveDirection_Tile.IsDown())
        {
            if (p.z <= Boundary.yMin)
            {
                newMoveDir = IndexDirection2.up;
            }
        }
        else if (MoveDirection_Tile.IsUp())
        {
            if (p.z >= Boundary.yMax)
            {
                newMoveDir = (Extensions.FlipCoin()) ? IndexDirection2.left : IndexDirection2.right;
            }
        }
        else
        {
            if (Extensions.FlipCoin(chanceToMoveDown))
            {
                newMoveDir = IndexDirection2.down;
            }
            else
            {
                if (MoveDirection_Tile.IsRight())
                {
                    if (p.x >= Boundary.xMax)
                    {
                        newMoveDir = IndexDirection2.left;
                    }
                }
                else if (MoveDirection_Tile.IsLeft())
                {
                    if (p.x <= Boundary.xMin)
                    {
                        newMoveDir = IndexDirection2.right;
                    }
                }
            }
        }

        if (newMoveDir.IsZero())
        {
            newMoveDir = MoveDirection_Tile;
        }

        MoveDirection_Tile = newMoveDir;
    }

    void Attack()
    {
        _enemy.Attack(DirectionToPlayer);
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