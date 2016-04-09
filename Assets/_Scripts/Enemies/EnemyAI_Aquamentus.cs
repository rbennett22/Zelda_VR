using UnityEngine;
using System.Collections;

public class EnemyAI_Aquamentus : EnemyAI 
{
    const float TileOffset = 0.5f;
    const float Epsilon = 0.001f;


    public Vector3 forwardDirection = new Vector3(-1, 0, 0);
    public Vector3 backwardDirection = new Vector3(1, 0, 0);
    

    Vector3 _targetPos = Vector3.zero;
    Vector3 _moveDirection;


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


	void Start () 
    {
        transform.forward = forwardDirection;
        MoveDirection = forwardDirection;
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
            if (_moveDirection == forwardDirection) 
            {
                Attack();
                MoveDirection = backwardDirection;
            }
            else 
            { 
                MoveDirection = forwardDirection;
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

    void Attack()
    {
        float degreesDelta = Random.RandomRange(1, 30);
        float radiansDelta = degreesDelta * Mathf.PI / 180;

        Vector3 toPlayer = _enemy.PlayerController.transform.position - transform.position;
        toPlayer.Normalize();

        Vector3 dir = toPlayer;
        Vector3 dirL = Vector3.RotateTowards(dir, -dir, -radiansDelta, 999);
        Vector3 dirR = Vector3.RotateTowards(dir, -dir,  radiansDelta, 999);

        _enemy.weapon.Fire(dir);
        _enemy.weapon.Fire(dirL);
        _enemy.weapon.Fire(dirR);
    }

}