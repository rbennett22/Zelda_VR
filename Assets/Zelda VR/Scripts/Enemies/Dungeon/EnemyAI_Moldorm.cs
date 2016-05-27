using System.Collections.Generic;
using UnityEngine;

public class EnemyAI_Moldorm : EnemyAI
{
    const int MAX_POS_HISTORY_LENGTH = 350;     // (in frames)


    public float turnSpeed = 60.0f;
    public float waveSpeed = 2.0f;
    public float jitter = 10.0f;
    public int startWormLength = 5;
    public float wormSeparationTime = 0.4f;

    public Sprite segmentSprite;


    float _radialAcceleration;

    Queue<Vector3> _positionHistory = new Queue<Vector3>(MAX_POS_HISTORY_LENGTH);


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

    public new Vector3 MoveDirection
    {
        get { return _enemyMove.MoveDirection; }
        set { _enemyMove.MoveDirection = value.normalized; }
    }

    void ReverseMoveDirection() { MoveDirection *= -1; }


    void Start()
    {
        _enemyMove.Mode = EnemyMove.MovementMode.DirectionOnly;
        _enemyMove.AlwaysFaceTowardsMoveDirection = false;

        MoveDirection = EnemyAI_Random.GetRandomDirectionXZ();

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
        if (IsPreoccupied) { return; }

        if (PauseManager.Instance.IsPaused_Any) { return; }

        if (IsHead)
        {
            UpdateMoveDirection();
        }
        else
        {
            MoveDirection = Vector3.zero;
            FollowNext();
        }

        UpdatePositionHistory();
    }

    void UpdateMoveDirection()
    {
        if (DetectObstructions(MoveDirection, DEFAULT_OBSTRUCTION_FEELER_LENGTH))
        {
            ReverseMoveDirection();
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
        if (_positionHistory.Count >= MAX_POS_HISTORY_LENGTH)
        {
            _positionHistory.Dequeue();
        }

        _positionHistory.Enqueue(transform.position);
    }


    void OnStun()
    {
        if (Prev != null) { Prev._enemy.Stun(); }
        if (Next != null) { Next._enemy.Stun(); }
    }
}