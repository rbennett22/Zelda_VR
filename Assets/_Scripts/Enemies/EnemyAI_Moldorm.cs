using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class EnemyAI_Moldorm : EnemyAI 
{
    const int MaxPositionHistoryLengthInFrames = 350;


    public float turnSpeed = 60.0f;
    public float waveSpeed = 2.0f;
    public float jitter = 10.0f;
    public int startWormLength = 5;
    public float wormSeparationTime = 0.4f;

    public Sprite segmentSprite;


    HealthController _healthController;

    Vector3 _moveDirection;
    float _radialAcceleration;

    Queue<Vector3> _positionHistory = new Queue<Vector3>(MaxPositionHistoryLengthInFrames);
    

    public EnemyAI_Moldorm Next { get; set; }
    public EnemyAI_Moldorm Prev { get; set; }
    public bool IsHead { get { return Next == null; } }
    public bool IsTail { get { return Prev == null; } }
    public bool IsVulnerable { get { return !_healthController.isIndestructible; } set { _healthController.isIndestructible = !value; } }
    public bool IsLastWormPiece { get { return (Next == null && Prev == null); } }
    public int WormSeparationTimeInFrames
    { 
        get 
        {
            //print("dT: " + Time.deltaTime + ", smooth dT: " + Time.smoothDeltaTime);
            if (Time.smoothDeltaTime == 0) { return 0; }
            return (int)(wormSeparationTime / Time.smoothDeltaTime); 
        }
    }

    public Vector3 PositionInHistory(int framesBack) 
    {
        Vector3[] ph = _positionHistory.ToArray();
        if (ph.Length == 0) { return transform.position; }

        if (framesBack < 0) { framesBack = 0; }
        if (framesBack > ph.Length - 1) { framesBack = ph.Length - 1; }
        return ph[ph.Length - 1 - framesBack];
    }

    public Vector3 MoveDirection 
    {
        get { return _moveDirection; }
        set
        {
            _moveDirection = value;
            _moveDirection.Normalize();
        }
    }


    protected void Awake()
    {
        base.Awake();

        _healthController = GetComponent<HealthController>();
    }

	void Start () 
    {
        MoveDirection = GetRandomMoveDirection();

        if (IsHead)
        {
            CreateWormBody();
        }

        IsVulnerable = (IsHead || IsTail);
	}

    void CreateWormBody()
    {
        EnemyAI_Moldorm mX = this;
        for (int i = 1; i < startWormLength; i++)
        {
            EnemyAI_Moldorm mNew = (Instantiate(gameObject, transform.position, transform.rotation) as GameObject).GetComponent<EnemyAI_Moldorm>();
            mNew.transform.parent = transform.parent;
            mX.Prev = mNew;
            mNew.Next = mX;

            if (_enemy.DungeonRoomRef != null)
            {
                _enemy.DungeonRoomRef.AddEnemy(mNew.GetComponent<Enemy>());
            }

            if (segmentSprite != null)
            {
                mNew.GetComponent<Enemy>().enemyAnim.GetComponent<SpriteRenderer>().sprite = segmentSprite;
            }

            mX = mNew;
        }
    }

    public void OnDeath()
    {
        if (Next != null)
        {
            Next.Prev = Prev;
            Next.IsVulnerable = (Next.IsHead || Next.IsTail);
        }
        if (Prev != null)
        {
            Prev.Next = Next;
            Prev.IsVulnerable = (Prev.IsHead || Prev.IsTail);
        }
    }


    void Update()
    {
        if (!_doUpdate) { return; }

        bool isPreoccupied = (_enemy.IsAttacking || _enemy.IsJumping || _enemy.IsSpawning || _enemy.IsParalyzed || _enemy.IsStunned);
        if (isPreoccupied) { return; }

        if (Pause.Instance.IsTimeFrozen) { return; }

        if (IsHead)
        {
            if (_moveDirection != Vector3.zero)
            {
                _enemy.MoveInDirection(_moveDirection);
            }

            UpdateMoveDirection();
        }
        else
        {
            FollowNext();
        }

        UpdatePositionHistory();
    }

    void UpdateMoveDirection()
    {
        if (DetectWall(transform.position, _moveDirection))
        {
            _moveDirection *= -1;
        }

        float dT = Time.deltaTime;

        float jitterAmount = Random.Range(-jitter, jitter);
        _radialAcceleration += dT * (waveSpeed + jitterAmount);
        float sin = dT * turnSpeed * Mathf.Sin(_radialAcceleration);
        Quaternion deltaRad = Quaternion.Euler(0, sin, 0);

        MoveDirection = deltaRad * MoveDirection;
    }

    void FollowNext()
    {
        transform.position = Next.PositionInHistory(WormSeparationTimeInFrames);
    }

    void UpdatePositionHistory()
    {
        string c;
        if (_positionHistory.Count >= MaxPositionHistoryLengthInFrames)
        {
            _positionHistory.Dequeue();
        }
        
        _positionHistory.Enqueue(transform.position);
    }


    bool DetectWall(Vector3 position, Vector3 direction)
    {
        float feelerLength = 0.75f;
        RaycastHit hitInfo;
        bool hit = true;

        Ray ray = new Ray(position, direction);
        LayerMask mask = Extensions.GetLayerMaskExcludingLayers("Enemies");
        hit = Physics.Raycast(ray, out hitInfo, feelerLength, mask);
        if (hit)
        {
            if (CommonObjects.IsPlayer(hitInfo.collider.gameObject))
            {
                hit = false;
            }
        }

        return hit;
    }


    Vector3 GetRandomMoveDirection()
    {
        int angle = Random.Range(0, 4) * 90;
        Vector3 dir = Quaternion.Euler(0, angle, 0) * new Vector3(1, 0, 0);
        return dir;
    }

    Vector3 GetRandomMoveDirectionExcluding(Vector3 excludeDirection)
    {
        if (excludeDirection == Vector3.zero)
        {
            return GetRandomMoveDirection();
        }

        int angle = Random.Range(1, 4) * 90;
        Vector3 dir = Quaternion.Euler(0, angle, 0) * excludeDirection;
        return dir;
    }


    void OnStun()
    {
        if (Prev != null) { Prev.gameObject.GetComponent<Enemy>().Stun(); }
        if (Next != null) { Next.gameObject.GetComponent<Enemy>().Stun(); }
    }

}