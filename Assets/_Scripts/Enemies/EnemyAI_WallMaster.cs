using UnityEngine;
using System.Collections;

public class EnemyAI_WallMaster : EnemyAI 
{
    const float TileOffset = 0.5f;
    const float Epsilon = 0.001f;
    const float GroundHeight = 0.5f;
    const float GroundHeight_Big = 2.0f;
    const float ProtrudeDistance = 0.5f;
    const float ProtrudeDistance_Big = 2.0f;
    const float CeilingHeight = 4.5f;
    const float CeilingHeight_Big = 25;
    const float HorzTravelDistance = 4;
    const float HorzTravelDistance_Big = 24;


    public GameObject wall;
    public DungeonRoomInfo.WallDirection wallDir;
    public bool isBig = false;


    Vector3 _targetPos = Vector3.zero;
    Vector3 _moveDirection;
    Vector3 _wallNormal, _wallTangent;

    
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

    bool _hasPlayerInClutches;
    

    public Vector3 MoveDirection 
    {
        get { return _moveDirection; }
        set
        {
            _moveDirection = value;

            _targetPos.x = (int)(_enemy.TileX + _moveDirection.x + Epsilon) + TileOffset;
            _targetPos.z = (int)(_enemy.TileZ + _moveDirection.z + Epsilon) + TileOffset;
            _targetPos.y = (int)((int)_enemy.transform.position.y + _moveDirection.y + Epsilon) + TileOffset;
        }
    }


    void Start()
    {
        _state = State.OutsideRoom;
    }


    void Update()
    {
        if (!_doUpdate) { return; }

        bool isPreoccupied = (_enemy.IsSpawning || _enemy.IsParalyzed || _enemy.IsStunned);
        if (isPreoccupied) { return; }

        if (_moveDirection != Vector3.zero)
        {
            _enemy.MoveInDirection(_moveDirection);
        }

        switch (_state)
        {
            case State.OutsideRoom: OnOutsideRoom(); break;
            case State.ComingThroughWall: OnComingThroughWall(); break;
            case State.CrawlingDownWall: OnCrawlingDownWall(); break;
            case State.OnFloor: OnFloor(); break;
            case State.CrawlingUpWall: OnCrawlingUpWall(); break;
            case State.ExitingThroughWall: OnExitingThroughWall(); break;
            default: break;
        }

        if (_hasPlayerInClutches)
        {
            _enemy.PlayerController.transform.position = transform.position;
        }
    }


    void OnOutsideRoom()
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
        startPos.y = isBig ? CeilingHeight_Big : CeilingHeight;

        transform.position = startPos - (2 * _wallNormal);
        _moveDirection = _wallNormal;
        float protrudeDistance = isBig ? ProtrudeDistance_Big : ProtrudeDistance;
        _targetPos = startPos + (protrudeDistance * _wallNormal);

        _state = State.ComingThroughWall;
    }

    void OnComingThroughWall()
    {
        if (HasReachedTargetPosition())
        {
            _moveDirection = new Vector3(0, -1, 0);
            _targetPos = transform.position;
            _targetPos.y = isBig ? GroundHeight_Big : GroundHeight;

            _state = State.CrawlingDownWall;
        }
    }

    void OnCrawlingDownWall()
    {
        if (HasReachedTargetPosition())
        {
            _moveDirection = -_wallTangent;
            float horzTravelDistance = isBig ? HorzTravelDistance_Big : HorzTravelDistance;
            _targetPos = transform.position - (horzTravelDistance * _wallTangent);

            _state = State.OnFloor;
        }
    }

    void OnFloor()
    {
        if (HasReachedTargetPosition())
        {
            _moveDirection = new Vector3(0, 1, 0);
            _targetPos = transform.position;
            _targetPos.y = isBig ? CeilingHeight_Big : CeilingHeight; ;

            _state = State.CrawlingUpWall;
        }
    }

    void OnCrawlingUpWall()
    {
        if (HasReachedTargetPosition())
        {
            _moveDirection = -1 * _wallNormal;
            _targetPos = transform.position + (2 * _moveDirection);

            if (_hasPlayerInClutches)
            {
                if (WorldInfo.Instance.IsInDungeon)
                {
                    WarpLinkToDungeonEntrance();
                }
                else
                {
                    CommonObjects.Player_C.IsParalyzed = false;
                }
                
                _hasPlayerInClutches = false;
            }

            _state = State.ExitingThroughWall;
        }
    }

    void OnExitingThroughWall()
    {
        if (HasReachedTargetPosition())
        {
            _state = State.OutsideRoom;
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

    bool _storedGravityEnabledState;
    void WarpLinkToDungeonEntrance()
    {
        OVRPlayerController pc = CommonObjects.PlayerController_C;
        _storedGravityEnabledState = pc.gravityEnabled;
        pc.gravityEnabled = false;
        Locations.Instance.WarpToDungeonEntranceRoom(gameObject, true);
    }
    void FinishedWarpingPlayer()
    {
        CommonObjects.Player_C.IsParalyzed = false;
        CommonObjects.PlayerController_C.gravityEnabled = _storedGravityEnabledState;
    }


    void OnTriggerEnter(Collider otherCollider)
    {
        GameObject other = otherCollider.gameObject;

        if (CommonObjects.IsPlayer(other))
        {
            CommonObjects.Player_C.IsParalyzed = true;
            _hasPlayerInClutches = true;
        }
    }

}