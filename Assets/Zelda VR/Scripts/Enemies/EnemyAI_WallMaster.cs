using UnityEngine;

public class EnemyAI_WallMaster : EnemyAI
{
    const float GROUND_HEIGHT = 0.5f;
    const float GROUND_HEIGHT_BIG = 2.0f;
    const float PROTRUDE_DISTANCE = 0.5f;
    const float PROTRUDE_DISTANCE_BIG = 2.0f;
    const float CEILING_HEIGHT = 4.5f;
    const float CEILING_HEIGHT_BIG = 25;
    const float HORZ_TRAVEL_DISTANCE = 4;
    const float HORZ_TRAVEL_DISTANCE_BIG = 24;


    public GameObject wall;
    public DungeonRoomInfo.WallDirection wallDir;
    public bool isBig = false;


    Vector3 _wallNormal, _wallTangent;


    float GroundHeight { get { return WorldOffsetY + (isBig ? GROUND_HEIGHT_BIG : GROUND_HEIGHT); } }
    float ProtrudeDistance { get { return isBig ? PROTRUDE_DISTANCE_BIG : PROTRUDE_DISTANCE; } }
    float CeilingHeight { get { return WorldOffsetY + (isBig ? CEILING_HEIGHT_BIG : CEILING_HEIGHT); } }
    float HorzTravelDistance { get { return isBig ? HORZ_TRAVEL_DISTANCE_BIG : HORZ_TRAVEL_DISTANCE; } }


    enum State
    {
        OutsideRoom,
        ComingThroughWall,
        CrawlingDownWall,
        OnFloor,
        CrawlingUpWall,
        ExitingThroughWall
    }
    State _state;

    bool _hasControlOfLink;


    public new Vector3 MoveDirection
    {
        get { return _enemyMove.MoveDirection; }
        set
        {
            Vector3 moveDir = value;

            Vector3 targetPos;
            targetPos.x = (int)(_enemy.TileX + moveDir.x + EPSILON) + TileMap.BLOCK_OFFSET_XZ;
            targetPos.z = (int)(_enemy.TileZ + moveDir.z + EPSILON) + TileMap.BLOCK_OFFSET_XZ;
            targetPos.y = (int)((int)_enemy.transform.position.y + moveDir.y + EPSILON) + TileMap.BLOCK_OFFSET_XZ;

            _enemyMove.TargetPosition = targetPos;
        }
    }


    void Start()
    {
        _enemyMove.Mode = EnemyMove.MovementMode.Destination;
        _enemyMove.AlwaysFaceTowardsMoveDirection = true;
        _enemyMove.targetPositionReached_Callback = OnTargetPositionReached;

        _state = State.OutsideRoom;
    }


    void Update()
    {
        if (!_doUpdate) { return; }
        if (IsPreoccupied) { return; }

        if (_hasControlOfLink)
        {
            _enemy.PlayerController.transform.position = transform.position;
        }
    }


    void OnTargetPositionReached(EnemyMove sender, Vector3 moveDirection)
    {
        switch (_state)
        {
            case State.OutsideRoom: OnTargetPositionReached_OutsideRoom(); break;
            case State.ComingThroughWall: OnTargetPositionReached_ComingThroughWall(); break;
            case State.CrawlingDownWall: OnTargetPositionReached_CrawlingDownWall(); break;
            case State.OnFloor: OnTargetPositionReached_Floor(); break;
            case State.CrawlingUpWall: OnTargetPositionReached_CrawlingUpWall(); break;
            case State.ExitingThroughWall: OnTargetPositionReached_ExitingThroughWall(); break;
            default: break;
        }
    }

    void OnTargetPositionReached_OutsideRoom()
    {
        int rand;

        if (!WorldInfo.Instance.IsSpecial)
        {
            DungeonRoom dr = _enemy.DungeonRoomRef;
            rand = Random.Range(0, 4);
            wallDir = (DungeonRoomInfo.WallDirection)rand;
            wall = dr.GetWallForDirection(wallDir);
        }

        _wallNormal = DungeonRoomInfo.NormalForWallDirection(wallDir);
        _wallTangent = DungeonRoomInfo.TangentForWallDirection(wallDir);
        if (Random.Range(0, 2) == 1) { _wallTangent *= -1; }

        rand = Random.Range(1, 4);
        Vector3 startPos = wall.transform.position + (rand * _wallTangent);
        startPos.y = CeilingHeight;

        transform.position = startPos - (2 * _wallNormal);
        MoveDirection = _wallNormal;
        TargetPosition = startPos + (ProtrudeDistance * _wallNormal);

        _state = State.ComingThroughWall;
    }

    void OnTargetPositionReached_ComingThroughWall()
    {
        MoveDirection = new Vector3(0, -1, 0);
        TargetPosition = new Vector3(transform.position.x, GroundHeight, transform.position.z);

        _state = State.CrawlingDownWall;
    }

    void OnTargetPositionReached_CrawlingDownWall()
    {
        MoveDirection = -_wallTangent;
        TargetPosition = transform.position - (HorzTravelDistance * _wallTangent);

        _state = State.OnFloor;
    }

    void OnTargetPositionReached_Floor()
    {
        MoveDirection = new Vector3(0, 1, 0);
        TargetPosition = new Vector3(transform.position.x, CeilingHeight, transform.position.z);

        _state = State.CrawlingUpWall;
    }

    void OnTargetPositionReached_CrawlingUpWall()
    {
        MoveDirection = -1 * _wallNormal;
        TargetPosition = transform.position + (2 * MoveDirection);

        if (_hasControlOfLink)
        {
            ReleaseControlOfLink();
        }

        _state = State.ExitingThroughWall;
    }

    void OnTargetPositionReached_ExitingThroughWall()
    {
        _state = State.OutsideRoom;
    }


    void OnTriggerEnter(Collider otherCollider)
    {
        GameObject other = otherCollider.gameObject;

        if (CommonObjects.IsPlayer(other))
        {
            TakeControlOfLink();
        }
    }

    void TakeControlOfLink()
    {
        CommonObjects.Player_C.IsParalyzed = true;
        _hasControlOfLink = true;
    }
    void ReleaseControlOfLink()
    {
        _hasControlOfLink = false;
        CommonObjects.Player_C.IsParalyzed = false;

        if (WorldInfo.Instance.IsInDungeon)
        {
            // WallMaster teleports Link back to dungeon entrance - by far the most annoying enemy in the entire game.
            WarpLinkToDungeonEntrance();
        }
    }
    void WarpLinkToDungeonEntrance()
    {
        Locations.Instance.WarpToDungeonEntranceRoom();
    }
}