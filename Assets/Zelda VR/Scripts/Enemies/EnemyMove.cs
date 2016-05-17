using System;
using UnityEngine;

[RequireComponent(typeof(Enemy))]

public class EnemyMove : MonoBehaviour
{
    public enum MovementMode
    {
        Destination,
        DirectionOnly,
    }
    MovementMode _mode;
    public MovementMode Mode
    {
        get { return _mode; }
        set
        {
            if (value == _mode)
            {
                return;
            }
            _mode = value;

            switch (_mode)
            {
                case MovementMode.Destination: TargetPosition = _targetPos; break;
                case MovementMode.DirectionOnly: break;
                default: break;
            }
        }
    }

    public Action<EnemyMove, Vector3> targetPositionReached_Callback;


    Enemy _enemy;

    Vector3 _moveDirection;
    Vector3 _targetPos = Vector3.zero;
    bool _targetPositionHasBeenReached;


    public bool AlwaysFaceTowardsMoveDirection { get; set; }
    public float Speed { get { return _enemy.speed; } set { _enemy.speed = value; } }       // TODO: Don't store speed in Enemy


    public Vector3 MoveDirection
    {
        get { return _moveDirection; }
        set
        {
            if (_mode == MovementMode.DirectionOnly)
            {
                _moveDirection = value.normalized;
            }
        }
    }

    public Vector3 TargetPosition
    {
        get { return _targetPos; }
        set
        {
            _targetPos = value;

            if (_mode == MovementMode.Destination)
            {
                _moveDirection = (_targetPos - transform.position).normalized;
                _targetPositionHasBeenReached = false;
            }
        }
    }


    void Awake()
    {
        _enemy = GetComponent<Enemy>();
    }


    void Update()
    {
        if (_enemy.IsPreoccupied) { return; }

        if (_moveDirection != Vector3.zero)
        {
            MoveInDirection(_moveDirection, AlwaysFaceTowardsMoveDirection);
        }

        if (_mode == MovementMode.Destination)
        {
            if (!_targetPositionHasBeenReached)
            {
                if (HasReachedTargetPosition())
                {
                    _targetPositionHasBeenReached = true;

                    if (targetPositionReached_Callback != null)
                    {
                        targetPositionReached_Callback(this, _moveDirection);
                    }

                    if (_targetPositionHasBeenReached)
                    {
                        _moveDirection = Vector3.zero;
                    }
                }
            }
        }
    }

    void MoveInDirection(Vector3 direction, bool doFaceTowardsDirection = true)
    {
        Vector3 displacement = direction * Speed * Time.deltaTime;
        transform.position += displacement;
        if (doFaceTowardsDirection)
        {
            transform.forward = direction;
        }
    }

    bool HasReachedTargetPosition()
    {
        Vector3 toTarget = _targetPos - transform.position;
        bool hasReachedTarget = (toTarget == Vector3.zero) || (Vector3.Dot(toTarget, _moveDirection) <= 0);

        if (hasReachedTarget)
        {
            transform.position = _targetPos;
        }

        return hasReachedTarget;
    }
}