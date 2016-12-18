using Immersio.Utility;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class EnemyAI_Random : EnemyAI
{
    const float PCNT_TO_RATIO = 0.01f;


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
    public bool onlyAttackIfPlayerIsInLineOfSight;
    public bool flying;                 // If true, the enemy can pass over HazardBlocks (ie. water, lava, etc.)
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
        _enemyMove.targetPositionReached_Callback += OnTargetPositionReached;

        _baseSpeed = _enemyMove.speed;

        MoveDirection_Tile = new IndexDirection2(DetermineActualMoveDirection(GetRandomTileDirection()));
    }


    void Update()
    {
        if (!_doUpdate) { return; }

        if (_wasJumping && !_enemy.IsJumping)
        {
            OnLanded();
        }
        _wasJumping = _enemy.IsJumping;

        if (IsPreoccupied) { return; }

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

    void OnLanded()
    {
        _enemyMove.enabled = true;
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
                Jump(desiredMoveDir);
            }
            else
            {
                MoveDirection_Tile = DetermineActualMoveDirection(desiredMoveDir);
            }
        }

        _justFinishedIdling = false;
    }

    void Jump(IndexDirection2 dir)
    {
        MoveDirection_Tile = dir;
        _enemyMove.enabled = false;
        _enemy.Jump(MoveDirection_Tile.ToVector3());
    }

    DiscreteAction GetDesiredAction()
    {
        if (FlipCoin(chanceToAttack * PCNT_TO_RATIO) && IsAttackingAnOption())
        {
            return DiscreteAction.Attack;
        }
        else if (FlipCoin(chanceToIdle * PCNT_TO_RATIO) && !_justFinishedIdling)
        {
            return DiscreteAction.Idle;
        }
        else if (FlipCoin(chanceToJump * PCNT_TO_RATIO))
        {
            return DiscreteAction.Jump;
        }
        else if (FlipCoin(chanceToChangeDirection * PCNT_TO_RATIO))
        {
            return DiscreteAction.ChangeDirection;
        }

        return DiscreteAction.ContinueInSameDirection;
    }

    bool IsAttackingAnOption()
    {
        if (!_enemy.HasWeapon || !_enemy.weapon.CanAttack)
        {
            return false;
        }
        if (onlyAttackIfPlayerIsInLineOfSight && !IsPlayerInSight())
        {
            return false;
        }
        return true;
    }

    IndexDirection2 GetDesiredMoveDirection(DiscreteAction action)
    {
        IndexDirection2 desiredMoveDir = IndexDirection2.zero;
        IndexDirection2 toPlayer = IndexDirection2.zero;

        _enemyMove.speed = _baseSpeed;

        if (_enemy.ShouldFollowBait())
        {
            desiredMoveDir = GetDirectionToBait();
        }
        else if (chasePlayerIfInSight && IsPlayerInSight(MAX_DISTANCE_PLAYER_CAN_BE_SEEN, out toPlayer))
        {
            _enemyMove.speed = _baseSpeed * chaseSpeedMultiplier;

            desiredMoveDir = toPlayer;
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

    IndexDirection2 GetDirectionToBait()
    {
        Vector3 toBait = Bait.ActiveBait.transform.position - transform.position;
        Vector2 toBaitXZ = new Vector2(toBait.x, toBait.z);
        toBaitXZ = toBaitXZ.GetNearestNormalizedAxisDirection();

        return new IndexDirection2(toBaitXZ);
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
            LayerMask mask = Extensions.GetLayerMaskIncludingLayers("Blocks", "Walls", "Stairs");
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

    public static IndexDirection2 GetRandomTileDirectionExcluding(List<IndexDirection2> excludeDirections)
    {
        List<IndexDirection2> availableDirections = IndexDirection2.AllValidNonZeroDirections.Where(d => !excludeDirections.Contains(d)).ToList();
        return availableDirections[Random.Range(0, availableDirections.Count)];
    }


    static bool FlipCoin(float chanceOfTrue = 0.5f)
    {
        return Extensions.FlipCoin(chanceOfTrue);
    }
}