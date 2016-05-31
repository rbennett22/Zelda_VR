using Immersio.Utility;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

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
    public float feelerLength = DEFAULT_OBSTRUCTION_FEELER_LENGTH;
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

        _baseSpeed = _enemyMove.speed;

        MoveDirection_Tile = new IndexDirection2(DetermineActualMoveDirection(GetRandomTileDirection()));
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
        else if (MoveDirection_Tile.IsZero())
        {
            DetermineNextAction();
        }
    }

    void LateUpdate()
    {
        _wasJumping = _enemy.IsJumping;
    }


    void OnLanded()
    {
        MoveDirection_Tile = IndexDirection2.zero;
    }

    void OnTargetPositionReached(EnemyMove sender, Vector3 moveDirection)
    {
        DetermineNextAction();
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
            IndexDirection2 desiredMoveDir = GetDesiredMoveDirection(desiredAction);
            if (desiredAction == DiscreteAction.Jump)
            {
                MoveDirection_Tile = desiredMoveDir;
                _enemy.Jump(MoveDirection_Tile.ToVector3());
            }
            else
            {
                MoveDirection_Tile = DetermineActualMoveDirection(desiredMoveDir);
            }
        }

        _justFinishedIdling = false;
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

    IndexDirection2 GetDesiredMoveDirection(DiscreteAction action)
    {
        IndexDirection2 desiredMoveDir = IndexDirection2.zero;
        Vector3 toPlayer = Vector3.zero;

        _enemyMove.speed = _baseSpeed;

        if (_enemy.ShouldFollowBait())
        {
            Vector3 toBait = Bait.ActiveBait.transform.position - transform.position;
            Vector2 toBaitXZ = new Vector2(toBait.x, toBait.z);
            toBaitXZ = toBaitXZ.GetNearestNormalizedAxisDirection();

            desiredMoveDir = new IndexDirection2(toBaitXZ);
        }
        else if (chasePlayerIfInSight && IsPlayerInSight(MAX_DISTANCE_PLAYER_CAN_BE_SEEN, out toPlayer))
        {
            _enemyMove.speed = _baseSpeed * chaseSpeedMultiplier;

            desiredMoveDir = new IndexDirection2(toPlayer);
        }
        else
        {
            if (MoveDirection_Tile.IsZero())
            {
                desiredMoveDir = GetRandomTileDirection();
            }
            else
            {
                if (action == DiscreteAction.ChangeDirection)
                {
                    List<IndexDirection2> excludeDirections = new List<IndexDirection2>();
                    excludeDirections.Add(MoveDirection_Tile);
                    if (avoidsReversingDirections)
                    {
                        excludeDirections.Add(MoveDirection_Tile.Reversed);
                    }

                    desiredMoveDir = GetRandomTileDirectionExcluding(excludeDirections);
                }
                else if (action == DiscreteAction.Jump)
                {
                    desiredMoveDir = GetRandomTileDirection();
                }
                else
                {
                    desiredMoveDir = MoveDirection_Tile;
                }
            }
        }

        if (!WorldInfo.Instance.IsInDungeon)
        {
            desiredMoveDir = EnforceBoundary(desiredMoveDir);
        }

        return desiredMoveDir;
    }

    IndexDirection2 DetermineActualMoveDirection(IndexDirection2 desiredMoveDirection)
    {
        Vector3 desiredMoveDirection_Vec = desiredMoveDirection.ToVector3();

        IndexDirection2 chosenDirection;
        bool canMove;
        float turnAngle = 0;

        do
        {
            Vector3 v = Quaternion.Euler(0, turnAngle, 0) * desiredMoveDirection_Vec;
            chosenDirection = new IndexDirection2(v);

            if (WorldInfo.Instance.IsOverworld)
            {
                // TODO: Use IndexDirection2.AllValidDirections instead of turnAngle += 90
                canMove = CanMoveInDirection_Overworld(chosenDirection);
            }
            else
            {
                canMove = !DetectObstructions(chosenDirection, feelerLength);
            }

            turnAngle += 90;
        }
        while (!canMove && turnAngle < 360);

        return chosenDirection;
    }

    override protected bool CanMoveFromTo(Vector3 from, Vector3 to)
    {
        if (flying)
        {
            Vector3 dir = to - from;
            LayerMask mask = Extensions.GetLayerMaskIncludingLayers("Blocks", "Walls");
            return !Physics.Raycast(from, dir.normalized, dir.magnitude, mask);
        }

        return base.CanMoveFromTo(from, to);
    }


    void Attack()
    {
        Vector3 dir = aimAttacksAtPlayer ? DirectionToPlayer : Vector3.zero;
        _enemy.Attack(dir);
    }

    void EnterIdleState()
    {
        MoveDirection_Tile = IndexDirection2.zero;

        _idleStartTime = Time.time;
        _idleDuration = Random.Range(minIdleDuration, maxIdleDuration);
        _isIdling = true;
    }


    public static Vector3 GetRandomDirectionXZ()
    {
        Vector3 dir = new Vector3();
        dir.x = Random.Range(-1f, 1f);
        dir.y = 0;
        dir.z = Random.Range(-1f, 1f);

        return dir.normalized;
    }

    public static IndexDirection2 GetRandomTileDirection()
    {
        IndexDirection2[] allDirections = IndexDirection2.AllValidNonZeroDirections;
        IndexDirection2 dir = allDirections[Random.Range(0, allDirections.Length)];
        return dir;
    }

    public static IndexDirection2 GetRandomTileDirectionExcluding(List<IndexDirection2> excludeDirections, bool alsoExcludeReverse = false)
    {
        List<IndexDirection2> availableDirections = IndexDirection2.AllValidNonZeroDirections.Where(d => !excludeDirections.Contains(d)).ToList();
        IndexDirection2 dir = availableDirections[Random.Range(0, availableDirections.Count)];
        return dir;
    }
}