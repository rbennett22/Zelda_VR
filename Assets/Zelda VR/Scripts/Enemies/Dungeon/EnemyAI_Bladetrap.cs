using Immersio.Utility;
using UnityEngine;

public class EnemyAI_Bladetrap : EnemyAI
{
    const float VISION_RANGE = 12;        // TODO      


    public float triggerSpeed = 4;
    public float returnSpeed = 2;


    bool _movingToPlayer;
    bool _returningToOrigin;
    Vector3 _origin;


    public bool PoisedToTrigger { get { return !(_movingToPlayer || _returningToOrigin); } }


    void Start()
    {
        _origin = transform.position;

        _enemyMove.Mode = EnemyMove.MovementMode.Destination;
        _enemyMove.AlwaysFaceTowardsMoveDirection = false;
        _enemyMove.targetPositionReached_Callback = OnTargetPositionReached;
    }


    void Update()
    {
        if (!_doUpdate) { return; }
        if (IsPreoccupied) { return; }

        if (PoisedToTrigger)
        {
            IndexDirection2 toPlayer;
            if (IsPlayerInSight(VISION_RANGE, out toPlayer))
            {
                Trigger(Player.Position);
            }
        }
        else if (_movingToPlayer)
        {
            if (DetectObstructions(MoveDirection, DEFAULT_OBSTRUCTION_FEELER_LENGTH))
            {
                ReturnToOrigin();
            }
        }
    }

    void Trigger(Vector3 targetPos)
    {
        if (!PoisedToTrigger)
        {
            return;
        }
        _movingToPlayer = true;

        _enemyMove.speed = triggerSpeed;

        TargetPosition = targetPos;
    }

    void ReturnToOrigin()
    {
        if (_returningToOrigin)
        {
            return;
        }
        _returningToOrigin = true;
        _movingToPlayer = false;

        _enemyMove.speed = returnSpeed;

        TargetPosition = _origin;
    }

    void OnTargetPositionReached(EnemyMove sender, Vector3 moveDirection)
    {
        if(_movingToPlayer)
        {
            ReturnToOrigin();
        }
        else if (_returningToOrigin)
        {
            _returningToOrigin = false;
        }
    }
}