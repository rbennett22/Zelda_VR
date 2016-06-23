using System;
using UnityEngine;

[RequireComponent(typeof(Enemy))]

public class EnemyMove : MonoBehaviour
{
    public float speed = 1;


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

        _targetPos = transform.position;
        _moveDirection = Vector3.zero;
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
            UpdateDestinationMode();
        }
    }

    void UpdateDestinationMode()
    {
        if (_targetPositionHasBeenReached)
        {
            return;
        }

        if (CheckIfTargetPositionHasBeenReached())
        {
            _targetPositionHasBeenReached = true;

            OnTargetPositionReached();

            if (_targetPositionHasBeenReached)  // We check again because a new targetPos may have been assigned in the callback above
            {
                _moveDirection = Vector3.zero;
            }
        }
    }
    bool CheckIfTargetPositionHasBeenReached()
    {
        Vector3 toTarget = _targetPos - transform.position;
        //toTarget.y = 0;
        bool hasReachedTarget = (toTarget == Vector3.zero) || (Vector3.Dot(toTarget, _moveDirection) <= 0);
        if (hasReachedTarget)
        {
            transform.position = _targetPos;
        }

        return hasReachedTarget;
    }

    void OnTargetPositionReached()
    {
        if (targetPositionReached_Callback != null)
        {
            targetPositionReached_Callback(this, _moveDirection);
        }
    }


    void MoveInDirection(Vector3 direction, bool doFaceTowardsDirection = true)
    {
        Vector3 displacement = direction * speed * Time.deltaTime;
        transform.position += displacement;
        if (doFaceTowardsDirection)
        {
            transform.forward = direction;
        }
    }
}