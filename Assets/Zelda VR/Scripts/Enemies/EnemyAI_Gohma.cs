using UnityEngine;
using System.Collections;

public class EnemyAI_Gohma : EnemyAI 
{
    const float TileOffset = 0.5f;
    const float Epsilon = 0.001f;


    public float pathWidth = 5.0f, pathHeight = 2.0f;
    public float chanceToAttack = 0.15f;
    public float chanceToMoveDown = 0.05f;
    public float chanceToOpenEye = 0.10f;
    public float openEyeDuration = 2.0f;

    public Animator animator;

    
    Vector3 _targetPos = Vector3.zero;
    Vector3 _moveDirection;
    Rect _moveBounds;

    Vector3 _right = new Vector3(1, 0, 0);
    Vector3 _left = new Vector3(-1, 0, 0);
    Vector3 _down = new Vector3(0, 0, -1);
    Vector3 _up = new Vector3(0, 0, 1);
    

    public Vector3 MoveDirection 
    {
        get { return _moveDirection; }
        set
        {
            _moveDirection = value;

            _targetPos.x = (int)(_enemy.TileX + _moveDirection.x + Epsilon) + TileOffset;
            _targetPos.z = (int)(_enemy.TileZ + _moveDirection.z + Epsilon) + TileOffset;
            _targetPos.y = transform.position.y;
        }
    }

    public bool IsEyeOpen { get { return animator.GetCurrentAnimatorStateInfo(0).IsTag("EyeOpen"); } }
    public bool IsEyeClosed { get { return animator.GetCurrentAnimatorStateInfo(0).IsTag("EyeClosed"); } }


	void Start () 
    {
        Vector3 p = transform.position;
        _moveBounds = new Rect(
            p.x - pathWidth * 0.5f, 
            p.z - pathHeight,
            pathWidth, pathHeight
            );

        MoveDirection = (Extensions.FlipCoin(0.5f)) ? _left : _right;
	}


    void Update()
    {
        if (!_doUpdate) { return; }

        bool isPreoccupied = (_enemy.IsStunned || _enemy.IsParalyzed);
        if (isPreoccupied) { return; }

        if (_moveDirection != Vector3.zero)
        {
            _enemy.MoveInDirection(_moveDirection, false);
        }

        if (HasReachedTargetPosition())     
        {
            SetNextMoveDirection();

            print("IsEyeClosed: " + IsEyeClosed);
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
    }

    bool HasReachedTargetPosition()
    {
        Vector3 toTarget = _targetPos - transform.position;
        toTarget.y = 0;
        bool hasReachedTarget = (toTarget == Vector3.zero) || (Vector3.Dot(toTarget, _moveDirection) <= 0);

        if(hasReachedTarget)
        {
            transform.position = _targetPos;
        }

        return hasReachedTarget;
    }

    void SetNextMoveDirection()
    {
        Vector3 p = transform.position;

        Vector3 newMoveDirection = Vector3.zero;
        if (_moveDirection == _down)
        {
            if (p.z <= _moveBounds.yMin)
            {
                newMoveDirection = _up;
            }
        }
        else if (_moveDirection == _up)
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
                if (_moveDirection == _right)
                {
                    if (p.x >= _moveBounds.xMax)
                    {
                        newMoveDirection = _left;
                    }
                }
                else if (_moveDirection == _left)
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
            newMoveDirection = _moveDirection;
        }

        MoveDirection = newMoveDirection;
    }

    void Attack()
    {
        Vector3 toPlayer = _enemy.PlayerController.transform.position - transform.position;
        toPlayer.Normalize();
        _enemy.weapon.Fire(toPlayer);
    }

    IEnumerator OpenEye()
    {
        print(" ~ OpenEye ~");
        animator.SetTrigger("OpenEye");
        yield return new WaitForSeconds(openEyeDuration);
        CloseEye();
    }

    void CloseEye()
    {
        animator.SetTrigger("CloseEye");
    }

}