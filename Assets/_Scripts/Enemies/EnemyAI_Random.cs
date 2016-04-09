using UnityEngine;
using System.Collections;

public class EnemyAI_Random : EnemyAI 
{
    public int chanceToIdle = 0;
    public int chanceToAttack = 0;
    public int chanceToJump = 0;
    public int chanceToChangeDirection = 20;
    public float minIdleDuration = 0, maxIdleDuration = 1;
    public bool chasePlayerIfInSight;
    public float chaseSpeedMultiplier = 1.0f;
    public bool faceTowardsMoveDirection = true;
    public float feelerLength = 1.45f;              // Used when rayCasting for walls in Dungeon to determine next moveDirection
    public bool aimAttacksAtPlayer;
    public bool flying;                 // If true, the enemy can pass over HazardBlocks (water, lava, etc.)
    public bool avoidsReversingDirections;


    Vector3 _targetPos = Vector3.zero;
    Vector3 _moveDirection;
    

    public enum DiscreteAction
    {
        Idle,
        ContinueInSameDirection,
        ChangeDirection,
        Attack,
        Jump
    }

    bool _isIdling;
    float _idleStartTime = float.NegativeInfinity;
    float _idleDuration;

    bool _justFinishedIdling;
    bool _prevIsJumping;

    float _baseSpeed;


    public Vector3 MoveDirection 
    {
        get { return _moveDirection; }
        set
        {
            _moveDirection = value;

            _targetPos.x = (int)(_enemy.TileX + _moveDirection.x + Epsilon) + TileOffset;
            _targetPos.z = (int)(_enemy.TileZ + _moveDirection.z + Epsilon) + TileOffset;
            _targetPos.y = transform.position.y;
        }
    }

    public Vector3 TargetPosition
    {
        get { return _targetPos; }
        set
        {
            _targetPos = value;
            _moveDirection = _targetPos - transform.position;
            _moveDirection.Normalize();
        }
    }


    override public void EnableAI()
    {
        base.EnableAI();

        TargetPosition = transform.position;
    }


	void Start () 
    {
        _baseSpeed = _enemy.speed;
        MoveDirection = DetermineActualMoveDirection(GetRandomMoveDirection());
	}


    void Update()
    {
        if (!_doUpdate) { return; }

        bool isPreoccupied = (_enemy.IsAttacking || _enemy.IsJumping || _enemy.IsSpawning || _enemy.IsParalyzed || _enemy.IsStunned);
        if (isPreoccupied) { _prevIsJumping = _enemy.IsJumping; return; }

        if (_prevIsJumping)
        {
            // Enemy just landed this frame
            _targetPos = transform.position;
            _moveDirection = Vector3.zero;
            _prevIsJumping = false;
        }

        if (_moveDirection != Vector3.zero)
        {
            _enemy.MoveInDirection(_moveDirection, faceTowardsMoveDirection);
        }

        if (_isIdling)
        {
            if (Time.time - _idleStartTime >= _idleDuration)
            {
                _isIdling = false;
                _justFinishedIdling = true;
            }
        }
        else if (HasReachedTargetPosition())     
        {
            //print("HasReachedTargetPosition");
            DiscreteAction desiredAction = GetDesiredAction();

            if (desiredAction == DiscreteAction.Attack)
            {
                Vector3 direction = Vector3.zero;
                if (aimAttacksAtPlayer)
                {
                    direction = _enemy.PlayerController.transform.position - transform.position;
                    direction.Normalize();
                }
                _enemy.Attack(direction);
            }
            else if (desiredAction == DiscreteAction.Idle)
            {
                EnterIdleState();
            }
            else
            {
                Vector3 desiredMoveDirection = GetDesiredMoveDirection(desiredAction);
                if (desiredAction == DiscreteAction.Jump)
                {
                    MoveDirection = desiredMoveDirection;
                    _enemy.Jump(MoveDirection);
                }
                else
                {
                    MoveDirection = DetermineActualMoveDirection(desiredMoveDirection);
                }
            }

            _justFinishedIdling = false;
        }
    }

    void EnterIdleState()
    {
        MoveDirection = Vector3.zero;
        _targetPos = transform.position;
        _idleStartTime = Time.time;
        _idleDuration = Random.Range(minIdleDuration, maxIdleDuration);
        _isIdling = true;
    }

    bool HasReachedTargetPosition()
    {
        Vector3 toTarget = _targetPos - transform.position;
        toTarget.y = 0;
        bool hasReachedTarget = (toTarget == Vector3.zero) || (Vector3.Dot(toTarget, _moveDirection) <= 0);

        if(hasReachedTarget)
        {
            transform.position = _targetPos;
        }

        return hasReachedTarget;
    }

    DiscreteAction GetDesiredAction()
    {
        DiscreteAction action;

        int rand = Random.Range(0, 100);
        if (_enemy.weapon != null && rand < chanceToAttack)
        {
            action = DiscreteAction.Attack;
        }
        else
        {
            rand = Random.Range(0, 100);
            if (rand < chanceToIdle && !_justFinishedIdling)
            {
                action = DiscreteAction.Idle;
            }
            else
            {
                rand = Random.Range(0, 100);
                if (rand < chanceToJump)
                {
                    action = DiscreteAction.Jump;
                }
                else
                {
                    rand = Random.Range(0, 100);
                    if (rand < chanceToChangeDirection)
                    {
                        action = DiscreteAction.ChangeDirection;
                    }
                    else
                    {
                        action = DiscreteAction.ContinueInSameDirection;
                    }
                }
            } 
        }

        return action;
    }

    Vector3 GetDesiredMoveDirection(DiscreteAction action)
    {
        Vector3 desiredMoveDirection = Vector3.zero;
        Vector3 directionToPlayer = Vector3.zero;

        _enemy.speed = _baseSpeed;

        if (_enemy.ShouldFollowBait())
        {
            Vector3 directionToBait = Bait.ActiveBait.transform.position - transform.position;
            Vector2 toBait = new Vector2(directionToBait.x, directionToBait.z);
            toBait = toBait.GetNearestNormalizedAxisDirection();
            desiredMoveDirection = new Vector3(toBait.x, 0, toBait.y);
        }
        else if (chasePlayerIfInSight && IsPlayerInSight(out directionToPlayer))
        {
            print("playerInSight: " + _baseSpeed);
            desiredMoveDirection = directionToPlayer;
            _enemy.speed = _baseSpeed * chaseSpeedMultiplier;
        }
        else
        {
            if (_moveDirection == Vector3.zero)
            {
                desiredMoveDirection = GetRandomMoveDirection();
            }
            else
            {
                if (action == DiscreteAction.ChangeDirection)
                {
                    if (avoidsReversingDirections)
                    {
                        desiredMoveDirection = GetRandomMoveDirectionExcluding(_moveDirection, true);
                    }
                    else
                    {
                        desiredMoveDirection = GetRandomMoveDirectionExcluding(_moveDirection);
                    }
                }
                else if (action == DiscreteAction.Jump)
                {
                    desiredMoveDirection = GetRandomMoveDirection();
                }
                else
                {
                    desiredMoveDirection = _moveDirection;
                }
            }
        }

        if (!WorldInfo.Instance.IsInDungeon)
        {
            desiredMoveDirection = EnforceBoundary(desiredMoveDirection);
        }

        return desiredMoveDirection;
    }

    bool IsPlayerInSight(out Vector3 direction)
    {
        RaycastHit hitInfo;
        Vector3 pos = transform.position;

        Ray rayLeft = new Ray(pos, new Vector3(-1, 0, 0));
        if (Physics.Raycast(rayLeft, out hitInfo))
        {
            if (CommonObjects.IsPlayer(hitInfo.collider.gameObject))
            {
                direction = rayLeft.direction;
                return true;
            }
        }

        Ray rayRight = new Ray(pos, new Vector3(1, 0, 0));
        if (Physics.Raycast(rayRight, out hitInfo))
        {
            if (CommonObjects.IsPlayer(hitInfo.collider.gameObject))
            {
                direction = rayRight.direction;
                return true;
            }
        }

        Ray rayUp = new Ray(pos, new Vector3(0, 0, 1));
        if (Physics.Raycast(rayUp, out hitInfo))
        {
            if (CommonObjects.IsPlayer(hitInfo.collider.gameObject))
            {
                direction = rayUp.direction;
                return true;
            }
        }

        Ray rayDown = new Ray(pos, new Vector3(0, 0, -1));
        if (Physics.Raycast(rayDown, out hitInfo))
        {
            if (CommonObjects.IsPlayer(hitInfo.collider.gameObject))
            {
                direction = rayDown.direction;
                return true;
            }
        }

        direction = Vector3.zero;
        return false;
    }


    Vector3 DetermineActualMoveDirection(Vector3 desiredMoveDirection)
    {
        Vector3 dir = desiredMoveDirection;
        Vector3 pos = transform.position;
        pos.y = 0.2f;

        if (WorldInfo.Instance.IsOverworld)
        {
            int nextTileX = (int)(_enemy.TileX + dir.x + Epsilon);
            int nextTileZ = (int)(_enemy.TileZ + dir.z + Epsilon);
            int nextTileCode = TileProliferator.Instance.tileMap.Tile(nextTileX, nextTileZ);

            float turnAngle = 90;
            while (!TileInfo.IsTilePassable(nextTileCode) && turnAngle < 360)
            {
                dir = Quaternion.Euler(0, turnAngle, 0) * desiredMoveDirection;
                nextTileX = (int)(_enemy.TileX + dir.x + Epsilon);
                nextTileZ = (int)(_enemy.TileZ + dir.z + Epsilon);
                nextTileCode = TileProliferator.Instance.tileMap.Tile(nextTileX, nextTileZ);
                turnAngle += 90;
            }
        }
        else
        {
            RaycastHit hitInfo;

            int layerMask;
            if (flying) { layerMask = Extensions.GetLayerMaskIncludingLayers("Blocks", "Walls"); }
            else        { layerMask = Extensions.GetLayerMaskIncludingLayers("Blocks", "Walls", "InvisibleBlocks"); }

            bool hit = true;
            float turnAngle = 0;
            do 
            {
                dir = Quaternion.Euler(0, turnAngle, 0) * desiredMoveDirection;
                Ray ray = new Ray(pos, dir);
                hit = Physics.Raycast(ray, out hitInfo, feelerLength, layerMask);
                turnAngle += 90;
            } while (hit && turnAngle < 360);
        }

        dir.Normalize();
        return dir;
    }


    Vector3 GetRandomMoveDirection()
    {
        int angle = Random.Range(0, 4) * 90;
        Vector3 dir = Quaternion.Euler(0, angle, 0) * new Vector3(1, 0, 0);
        return dir;
    }

    Vector3 GetRandomMoveDirectionExcluding(Vector3 excludeDirection, bool alsoExcludeReverse = false)
    {
        if (excludeDirection == Vector3.zero)
        {
            return GetRandomMoveDirection();
        }

        int angle;
        Vector3 dir;
        if (alsoExcludeReverse)
        {
            angle = Extensions.FlipCoin() ? 90 : 270;
            dir = Quaternion.Euler(0, angle, 0) * excludeDirection;
        }
        else
        {
            angle = Random.Range(1, 4) * 90;
            dir = Quaternion.Euler(0, angle, 0) * excludeDirection;
        }

        return dir;
    }

}