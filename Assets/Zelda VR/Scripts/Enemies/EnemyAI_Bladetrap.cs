using UnityEngine;
using Immersio.Utility;

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
            playerDungeonRoom = DungeonRoom.GetRoomForPosition(_enemy.PlayerController.transform.position);
        }

        if (playerDungeonRoom == null || _enemy.DungeonRoomRef == playerDungeonRoom)
        {
            Vector3 toPlayer = ToPlayer;
            if (Mathf.Abs(toPlayer.y) < 0.5f)
            {
                if (Mathf.Abs(toPlayer.x) < TileMap.BLOCK_OFFSET_XZ)
                {
                    toPlayer.x = 0;
                    Trigger(new TileDirection(toPlayer));
                }
                else if (Mathf.Abs(toPlayer.z) < TileMap.BLOCK_OFFSET_XZ)
                {
                    toPlayer.z = 0;
                    Trigger(new TileDirection(toPlayer));
                }
            }
        }
    }

    void Trigger(TileDirection direction)
    {
        if (!PoisedToTrigger) { return; }

        _enemyMove.Speed = triggerSpeed;
        MoveDirection = direction;
        _movingToPlayer = true;
    }

    void OnTargetPositionReached(EnemyMove sender, Vector3 moveDirection)
    {
        if (_movingToPlayer)
        {
            Ray ray = new Ray(transform.position, moveDirection);
            RaycastHit hitInfo;
            if (Physics.Raycast(ray, out hitInfo, 1.05f))
            {
                if (hitInfo.collider.gameObject != _enemy.PlayerController)
                {
                    ReturnToOrigin(new TileDirection(-moveDirection));
                }
            }
        }
        else
        {
            MoveDirection = TileDirection.Zero;
            _returningToOrigin = false;
        }
    }

    void ReturnToOrigin(TileDirection moveDirection)
    {
        if (_returningToOrigin) { return; }
        _returningToOrigin = true;

        _enemyMove.Speed = returnSpeed;
        MoveDirection = moveDirection;
        _movingToPlayer = false;
    }
}