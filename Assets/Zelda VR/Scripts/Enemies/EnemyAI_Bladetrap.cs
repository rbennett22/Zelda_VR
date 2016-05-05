using UnityEngine;

public class EnemyAI_Bladetrap : EnemyAI 
{
    public float triggerSpeed = 2;
    public float returnSpeed = 1;


    Vector3 _origin;

    bool _movingToPlayer;
    bool _returningToOrigin;


    public bool PoisedToTrigger { get { return !(_movingToPlayer || _returningToOrigin); } }


    void Start()
    {
        _enemyMove.Mode = EnemyMove.MovementMode.Destination;
        _enemyMove.AlwaysFaceTowardsMoveDirection = false;
        _enemyMove.targetPositionReached_Callback = OnTargetPositionReached;

        _origin = transform.position;
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
            Vector3 dir = ToPlayer;
            if (Mathf.Abs(dir.y) < 0.5f)
            {
                if (Mathf.Abs(dir.x) < TILE_EXTENT)
                {
                    dir.x = 0;
                    Trigger(dir);
                }
                else if (Mathf.Abs(dir.z) < TILE_EXTENT)
                {
                    dir.z = 0;
                    Trigger(dir);
                }
            }
        }
    }

    void Trigger(Vector3 direction)
    {
        if (!PoisedToTrigger) { return; }

        _enemyMove.Speed = triggerSpeed;
        MoveDirection = new Vector3(direction.x, 0, direction.z);
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
                    ReturnToOrigin(-moveDirection);
                }
            }
        }
        else
        {
            MoveDirection = Vector3.zero;
            _returningToOrigin = false;
        }
    }

    void ReturnToOrigin(Vector3 moveDirection)
    {
        if (_returningToOrigin) { return; }
        _returningToOrigin = true;

        _enemyMove.Speed = returnSpeed;
        MoveDirection = moveDirection;
        _movingToPlayer = false;
    }
}