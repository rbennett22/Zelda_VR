using Immersio.Utility;
using UnityEngine;

public class EnemyAI_Bladetrap : EnemyAI
{
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
            if (Mathf.Abs(toPlayer.y) < 0.5f)
            {
                if (Mathf.Abs(toPlayer.x) < TileMap.BLOCK_OFFSET_XZ)
                {
                    toPlayer.x = 0;
                    Trigger(new IndexDirection2(toPlayer));
                }
                else if (Mathf.Abs(toPlayer.z) < TileMap.BLOCK_OFFSET_XZ)
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
        MoveDirection = direction;
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
            MoveDirection = IndexDirection2.zero;
            _returningToOrigin = false;
        }
    }

    void ReturnToOrigin(IndexDirection2 moveDirection)
    {
        if (_returningToOrigin) { return; }
        _returningToOrigin = true;

        _enemyMove.speed = returnSpeed;
        MoveDirection = moveDirection;
        _movingToPlayer = false;
    }
}