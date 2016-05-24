using Immersio.Utility;
using UnityEngine;

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
    bool _wasJumping;

    float _baseSpeed;


    override public void EnableAI()
    {
        base.EnableAI();

        TargetPosition = transform.position;
    }


    void Start()
    {
        _enemyMove.Mode = EnemyMove.MovementMode.Destination;
        _enemyMove.AlwaysFaceTowardsMoveDirection = faceTowardsMoveDirection;
        _enemyMove.targetPositionReached_Callback = OnTargetPositionReached;

        _baseSpeed = _enemyMove.Speed;

        MoveDirection = new TileDirection(DetermineActualMoveDirection(GetRandomTileDirection()));
    }


    void Update()
    {
        if (!_doUpdate) { return; }
        if (IsPreoccupied) { return; }

        if (_wasJumping && !_enemy.IsJumping)
        {
            OnLanded();
        }

        if (_isIdling)
        {
            if (Time.time - _idleStartTime >= _idleDuration)
            {
                _isIdling = false;
                _justFinishedIdling = true;
            }
        }
        else if (MoveDirection.IsZero())
        {
            DetermineNextAction();
        }
    }

    void DetermineNextAction()
    {
        DiscreteAction desiredAction = GetDesiredAction();

        if (desiredAction == DiscreteAction.Attack)
        {
            Attack();
        }
        else if (desiredAction == DiscreteAction.Idle)
        {
            EnterIdleState();
        }
        else
        {
            Vector3 desiredMoveDir = GetDesiredMoveDirection(desiredAction);
            if (desiredAction == DiscreteAction.Jump)
            {
                MoveDirection = new TileDirection(desiredMoveDir);

                _enemy.Jump(MoveDirection.ToVector3());
            }
            else
            {
                MoveDirection = new TileDirection(DetermineActualMoveDirection(desiredMoveDir));
            }
        }

        _justFinishedIdling = false;
    }

    void LateUpdate()
    {
        _wasJumping = _enemy.IsJumping;
    }


    void OnLanded()
    {
        MoveDirection = TileDirection.Zero;
    }

    void OnTargetPositionReached(EnemyMove sender, Vector3 moveDirection)
    {
        DetermineNextAction();
    }

    void Attack()
    {
        Vector3 dir = aimAttacksAtPlayer ? ToPlayer : Vector3.zero;
        _enemy.Attack(dir);
    }

    void EnterIdleState()
    {
        MoveDirection = TileDirection.Zero;

        _idleStartTime = Time.time;
        _idleDuration = Random.Range(minIdleDuration, maxIdleDuration);
        _isIdling = true;
    }


    DiscreteAction GetDesiredAction()
    {
        DiscreteAction action;

        int rand = Random.Range(0, 100);
        if (_enemy.HasWeapon && rand < chanceToAttack)
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
        Vector3 desiredMoveDir = Vector3.zero;
        Vector3 toPlayer = Vector3.zero;

        _enemyMove.Speed = _baseSpeed;

        if (_enemy.ShouldFollowBait())
        {
            Vector3 toBait = Bait.ActiveBait.transform.position - transform.position;
            Vector2 toBaitXZ = new Vector2(toBait.x, toBait.z);
            toBaitXZ = toBaitXZ.GetNearestNormalizedAxisDirection();
            desiredMoveDir = new Vector3(toBaitXZ.x, 0, toBaitXZ.y);
        }
        else if (chasePlayerIfInSight && IsPlayerInSight(out toPlayer))
        {
            desiredMoveDir = toPlayer;
            _enemyMove.Speed = _baseSpeed * chaseSpeedMultiplier;
        }
        else
        {
            if (MoveDirection.IsZero())
            {
                desiredMoveDir = GetRandomTileDirection();
            }
            else
            {
                if (action == DiscreteAction.ChangeDirection)
                {
                    if (avoidsReversingDirections)
                    {
                        desiredMoveDir = GetRandomMoveDirectionExcluding(MoveDirection.ToVector3(), true);
                    }
                    else
                    {
                        desiredMoveDir = GetRandomMoveDirectionExcluding(MoveDirection.ToVector3());
                    }
                }
                else if (action == DiscreteAction.Jump)
                {
                    desiredMoveDir = GetRandomTileDirection();
                }
                else
                {
                    desiredMoveDir = MoveDirection.ToVector3();
                }
            }
        }

        if (!WorldInfo.Instance.IsInDungeon)
        {
            desiredMoveDir = EnforceBoundary(desiredMoveDir);
        }

        return desiredMoveDir;
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
            int nextTileX = (int)(_enemy.TileX + dir.x + EPSILON);
            int nextTileZ = (int)(_enemy.TileZ + dir.z + EPSILON);

            TileMap tileMap = CommonObjects.OverworldTileMap;
            int nextTileCode = tileMap.Tile(nextTileX, nextTileZ);

            float turnAngle = 90;
            while (!TileInfo.IsTilePassable(nextTileCode) && turnAngle < 360)
            {
                dir = Quaternion.Euler(0, turnAngle, 0) * desiredMoveDirection;
                nextTileX = (int)(_enemy.TileX + dir.x + EPSILON);
                nextTileZ = (int)(_enemy.TileZ + dir.z + EPSILON);
                nextTileCode = tileMap.Tile(nextTileX, nextTileZ);

                turnAngle += 90;
            }
        }
        else    // Dungeon
        {
            RaycastHit hitInfo;

            int layerMask;
            if (flying) { layerMask = Extensions.GetLayerMaskIncludingLayers("Blocks", "Walls"); }
            else { layerMask = Extensions.GetLayerMaskIncludingLayers("Blocks", "Walls", "InvisibleBlocks"); }

            bool hit = true;
            float turnAngle = 0;
            do
            {
                dir = Quaternion.Euler(0, turnAngle, 0) * desiredMoveDirection;
                Ray ray = new Ray(pos, dir);
                hit = Physics.Raycast(ray, out hitInfo, feelerLength, layerMask);
                turnAngle += 90;
            }
            while (hit && turnAngle < 360);
        }

        dir.Normalize();
        return dir;
    }


    public static Vector3 GetRandomTileDirection()
    {
        TileDirection.Direction[] allDirections = TileDirection.AllDirections;
        TileDirection.Direction d = allDirections[Random.Range(0, allDirections.Length)];
        return new TileDirection(d).ToVector3();
    }

    public static Vector3 GetRandomMoveDirectionExcluding(Vector3 excludeDirection, bool alsoExcludeReverse = false)
    {
        if (excludeDirection == Vector3.zero)
        {
            return GetRandomTileDirection();
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