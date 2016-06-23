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


    void Start()
    {
        AssignToWall(wallDir);

        _enemyMove.Mode = EnemyMove.MovementMode.Destination;
        _enemyMove.AlwaysFaceTowardsMoveDirection = true;
        _enemyMove.targetPositionReached_Callback += OnTargetPositionReached;

        _state = State.OutsideRoom;
    }


    void Update()
    {
        if (!_doUpdate) { return; }
        if (IsPreoccupied) { return; }

        if (_hasControlOfLink)
        {
            Player.Position = transform.position;
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
        AssignToRandomWall();

        int rand = Random.Range(1, 4);
        Vector3 startPos = wall.transform.position + (rand * _wallTangent);
        startPos.y = CeilingHeight;

        transform.position = startPos - (2 * _wallNormal);
        TargetPosition = startPos + (ProtrudeDistance * _wallNormal);

        _state = State.ComingThroughWall;
    }

    void OnTargetPositionReached_ComingThroughWall()
    {
        Vector3 p = transform.position;
        p.y = GroundHeight;
        TargetPosition = p;

        _state = State.CrawlingDownWall;
    }

    void OnTargetPositionReached_CrawlingDownWall()
    {
        TargetPosition = transform.position - (HorzTravelDistance * _wallTangent);

        _state = State.OnFloor;
    }

    void OnTargetPositionReached_Floor()
    {
        Vector3 p = transform.position;
        p.y = CeilingHeight;
        TargetPosition = p;

        _state = State.CrawlingUpWall;
    }

    void OnTargetPositionReached_CrawlingUpWall()
    {
        TargetPosition = transform.position - (2 * _wallNormal);

        if (_hasControlOfLink)
        {
            ReleaseControlOfLink();
        }

        _state = State.ExitingThroughWall;
    }

    void OnTargetPositionReached_ExitingThroughWall()
    {
        _state = State.OutsideRoom;

        OnTargetPositionReached_OutsideRoom();
    }


    void AssignToRandomWall()
    {
        DungeonRoomInfo.WallDirection d = WorldInfo.Instance.IsInDungeon ? DungeonRoomInfo.GetRandomWallDirection() : wallDir;
        AssignToWall(d);
    }
    void AssignToWall(DungeonRoomInfo.WallDirection newWallDir)
    {
        wallDir = newWallDir;

        if (WorldInfo.Instance.IsInDungeon)
        {
            DungeonRoom dr = _enemy.DungeonRoomRef;
            wall = dr.GetWallForDirection(wallDir);
        }

        _wallNormal = DungeonRoomInfo.NormalForWallDirection(wallDir);
        _wallTangent = DungeonRoomInfo.TangentForWallDirection(wallDir);
        if (Extensions.FlipCoin()) { _wallTangent *= -1; }
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
        Player.IsParalyzed = true;
        _hasControlOfLink = true;
    }
    void ReleaseControlOfLink()
    {
        _hasControlOfLink = false;
        Player.IsParalyzed = false;

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