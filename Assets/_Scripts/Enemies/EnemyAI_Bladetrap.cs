using UnityEngine;
using System.Collections;

public class EnemyAI_Bladetrap : EnemyAI 
{
    const float TileHalfWidth = 0.5f;


    public float triggerSpeed = 2;
    public float returnSpeed = 1;


    Vector3 _moveDirection;
    Vector3 _origin;
    bool _movingToPlayer;
    bool _returningToOrigin;


    public bool PoisedToTrigger { get { return !(_movingToPlayer || _returningToOrigin); } }


    void Start()
    {
        _origin = transform.position;
    }


    void Update()
    {
        if (!_doUpdate) { return; }
        if (_enemy.IsParalyzed || _enemy.IsStunned) { return; }

        if (PoisedToTrigger)
        {
            DungeonRoom playerDungeonRoom = DungeonRoom.GetRoomForPosition(_enemy.PlayerController.transform.position);
            if (_enemy.DungeonRoomRef == playerDungeonRoom)
            {
                Vector3 pos = transform.position;
                Vector3 toPlayer = _enemy.PlayerController.transform.position - pos;
                if (Mathf.Abs(toPlayer.y) < 0.5f)
                {
                    if (Mathf.Abs(toPlayer.x) < TileHalfWidth)
                    {
                        toPlayer.x = 0;
                        Trigger(toPlayer);
                    }
                    else if (Mathf.Abs(toPlayer.z) < TileHalfWidth)
                    {
                        toPlayer.z = 0;
                        Trigger(toPlayer);
                    }
                }
            }
        }
        else
        {
            if (_moveDirection != Vector3.zero)
            {
                _enemy.MoveInDirection(_moveDirection, false);
            }

            if (_movingToPlayer)
            {
                Ray ray = new Ray(transform.position, _moveDirection);
                RaycastHit hitInfo;
                if (Physics.Raycast(ray, out hitInfo, 1.05f))
                {
                    if (hitInfo.collider.gameObject != _enemy.PlayerController)
                    {
                        ReturnToOrigin();
                    }
                }
            }
            else
            {
                if (HasReachedOrigin())
                {
                    _moveDirection = Vector3.zero;
                    _returningToOrigin = false;
                }
            }
        }
    }

    bool HasReachedOrigin()
    {
        Vector3 toOrigin = _origin - transform.position;
        toOrigin.y = 0;
        bool hasReachedOrigin = (toOrigin == Vector3.zero) || (Vector3.Dot(toOrigin, _moveDirection) <= 0);

        if (hasReachedOrigin)
        {
            transform.position = _origin;
        }

        return hasReachedOrigin;
    }

    void Trigger(Vector3 direction)
    {
        if (!PoisedToTrigger) { return; }

        _enemy.speed = triggerSpeed;
        _moveDirection = direction;
        _moveDirection.y = 0;
        _movingToPlayer = true;
    }

    void ReturnToOrigin()
    {
        if (_returningToOrigin) { return; }

        _enemy.speed = returnSpeed;
        _moveDirection = -_moveDirection;
        _movingToPlayer = false;
        _returningToOrigin = true;
    }

}