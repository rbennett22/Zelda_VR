using UnityEngine;
using System.Collections;

public class EnemyAI_Gohma : EnemyAI 
{
    public float pathWidth = 5.0f, pathHeight = 2.0f;
    public float chanceToAttack = 0.15f;
    public float chanceToMoveDown = 0.05f;
    public float chanceToOpenEye = 0.10f;
    public float openEyeDuration = 2.0f;

    
    Rect _moveBounds;

    Vector3 _right = new Vector3(1, 0, 0);
    Vector3 _left = new Vector3(-1, 0, 0);
    Vector3 _down = new Vector3(0, 0, -1);
    Vector3 _up = new Vector3(0, 0, 1);
    

    public bool IsEyeOpen { get { return AnimatorInstance.GetCurrentAnimatorStateInfo(0).IsTag("EyeOpen"); } }
    public bool IsEyeClosed { get { return AnimatorInstance.GetCurrentAnimatorStateInfo(0).IsTag("EyeClosed"); } }


	void Start () 
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

        MoveDirection = (Extensions.FlipCoin(0.5f)) ? _left : _right;
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

        Vector3 newMoveDirection = Vector3.zero;
        if (MoveDirection == _down)
        {
            if (p.z <= _moveBounds.yMin)
            {
                newMoveDirection = _up;
            }
        }
        else if (MoveDirection == _up)
        {
            if (p.z >= _moveBounds.yMax)
            {
                newMoveDirection = (Extensions.FlipCoin(0.5f)) ? _left : _right;
            }
        }
        else
        {
            if (Extensions.FlipCoin(chanceToMoveDown))
            {
                newMoveDirection = _down;
            }
            else
            {
                if (MoveDirection == _right)
                {
                    if (p.x >= _moveBounds.xMax)
                    {
                        newMoveDirection = _left;
                    }
                }
                else if (MoveDirection == _left)
                {
                    if (p.x <= _moveBounds.xMin)
                    {
                        newMoveDirection = _right;
                    }
                }
            }
        }

        if (newMoveDirection == Vector3.zero)
        {
            newMoveDirection = MoveDirection;
        }

        MoveDirection = newMoveDirection;
    }

    void Attack()
    {
        _enemy.weapon.Fire(ToPlayer);
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