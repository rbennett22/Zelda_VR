using Immersio.Utility;
using UnityEngine;

public class EnemyAI_Bladetrap : EnemyAI
{
    const float BLADETRAP_RADIUS = 0.5f;        // TODO:


    public float triggerSpeed = 2;
    public float returnSpeed = 1;


    bool _movingToPlayer;
    bool _returningToOrigin;


    public bool PoisedToTrigger { get { return !(_movingToPlayer || _returningToOrigin); } }


    void Start()
    {
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
            DoTriggerIfPlayerDetected();
        }
    }

    void DoTriggerIfPlayerDetected()
    {
        DungeonRoom playerDungeonRoom = null;
        if (WorldInfo.Instance.IsInDungeon)
        {
            playerDungeonRoom = DungeonRoom.GetRoomForPosition(Player.Position);
        }

        if (playerDungeonRoom == null || _enemy.DungeonRoomRef == playerDungeonRoom)
        {
            Vector3 toPlayer = DirectionToPlayer;
            if (Mathf.Abs(toPlayer.y) < BLADETRAP_RADIUS)
            {
                if (Mathf.Abs(toPlayer.x) < BLADETRAP_RADIUS)
                {
                    toPlayer.x = 0;
                    Trigger(new IndexDirection2(toPlayer));
                }
                else if (Mathf.Abs(toPlayer.z) < BLADETRAP_RADIUS)
                {
                    toPlayer.z = 0;
                    Trigger(new IndexDirection2(toPlayer));
                }
            }
        }
    }

    void Trigger(IndexDirection2 direction)
    {
        if (!PoisedToTrigger) { return; }

        _enemyMove.speed = triggerSpeed;
        MoveDirection_Tile = direction;
        _movingToPlayer = true;
    }

    void OnTargetPositionReached(EnemyMove sender, Vector3 moveDirection)
    {
        if (_movingToPlayer)
        {
            if (DetectObstructions(moveDirection, DEFAULT_OBSTRUCTION_FEELER_LENGTH))
            {
                ReturnToOrigin(new IndexDirection2(-moveDirection));
            }
        }
        else if (_returningToOrigin)
        {
            MoveDirection_Tile = IndexDirection2.zero;
            _returningToOrigin = false;
        }
    }

    void ReturnToOrigin(IndexDirection2 moveDirection)
    {
        if (_returningToOrigin) { return; }
        _returningToOrigin = true;

        _enemyMove.speed = returnSpeed;
        MoveDirection_Tile = moveDirection;
        _movingToPlayer = false;
    }
}