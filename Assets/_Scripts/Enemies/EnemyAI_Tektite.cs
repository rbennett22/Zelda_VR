using UnityEngine;


public class EnemyAI_Tektite : EnemyAI
{
    public float minTimeBetweenJumps = 2.5f;
    public float maxTimeBetweenJumps = 4.5f;


    float _jumpCooldownDuration;
    float _lastJumpTime = float.NegativeInfinity;
    bool _wasJumping = false;


	void Start () 
    {
        _lastJumpTime = Time.time;
        _jumpCooldownDuration = 1.0f;
	}


    void Update()
    {
        if (!_doUpdate) { return; }

        bool isPreoccupied = (_enemy.IsAttacking || _enemy.IsJumping || _enemy.IsSpawning || _enemy.IsParalyzed);
        if (!isPreoccupied)
        {
            if (_wasJumping)
            { 
                OnLanded();
            }

            float timeSinceLastJump = Time.time - _lastJumpTime;
            if (timeSinceLastJump >= _jumpCooldownDuration || CommonObjects.Player_C.IsAttackingWithSword)
            {
                if (IsBlockingAnExit())  // Prevent trapping Link into a Grotto or Dungeon stairs entrance/exit
                {
                    JumpAwayFromPlayer();
                }
                else
                {
                    if (_enemy.ShouldFollowBait())
                    {
                        JumpToBait();
                    }
                    else
                    {
                        JumpToPlayer();
                    }
                }
            }
        }

        _wasJumping = _enemy.IsJumping;
    }

    bool IsBlockingAnExit()
    {
        bool isBlocking = false;
        int tileCode = TileProliferator.Instance.tileMap.Tile(_enemy.TileX, _enemy.TileZ);
        isBlocking = TileInfo.IsTileAnEntrance(tileCode);
        return isBlocking;
    }



    void JumpToPlayer()
    {
        Vector3 playerPos = _enemy.PlayerController.transform.position;
        Vector3 toPlayer = playerPos - transform.position;

        _enemy.Jump(EnforceBoundary(toPlayer));
    }

    void JumpAwayFromPlayer()
    {
        Vector3 playerPos = _enemy.PlayerController.transform.position;
        Vector3 toPlayer = playerPos - transform.position;

        _enemy.Jump(-toPlayer);
    }

    void JumpToBait()
    {
        Vector3 toBait = Bait.ActiveBait.transform.position - transform.position;
        _enemy.Jump(toBait);
    }

    void OnLanded()
    {
        _lastJumpTime = Time.time;
        _jumpCooldownDuration = Random.Range(minTimeBetweenJumps, maxTimeBetweenJumps);
    }
}
