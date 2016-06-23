using System.Collections.Generic;
using UnityEngine;

public class EnemyAI_Moldorm : EnemyAI
{
    const float FRAME_LENGTH = 0.01667f;
    const int MAX_POS_HISTORY_LENGTH = 350;     // (in frames)


    public float turnSpeed = 60.0f;
    public float waveSpeed = 2.0f;
    public float jitter = 10.0f;
    public int startWormLength = 5;
    public float wormSeparationTime = 0.4f;

    public Sprite segmentSprite;


    float _radialAccel;

    Queue<Vector3> _positionHistory = new Queue<Vector3>(MAX_POS_HISTORY_LENGTH);


    public EnemyAI_Moldorm Next { get; private set; }
    public EnemyAI_Moldorm Prev { get; private set; }
    public bool IsHead { get { return Next == null; } }
    public bool IsTail { get { return Prev == null; } }
    public bool IsVulnerable { get { return !_healthController.isIndestructible; } set { _healthController.isIndestructible = !value; } }
    public bool IsLastWormPiece { get { return (Next == null && Prev == null); } }

    int WormSeparationTimeInFrames
    {
        get
        {
            return (int)(wormSeparationTime / FRAME_LENGTH);
        }
    }

    public Vector3 PositionInHistory(int framesBack)
    {
        Vector3[] ph = _positionHistory.ToArray();
        if (ph.Length == 0) { return transform.position; }

        int n = ph.Length - 1;
        framesBack = Mathf.Clamp(framesBack, 0, n);
        return ph[n - framesBack];
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
            GameObject g = Instantiate(gameObject, transform.position, transform.rotation) as GameObject;

            EnemyAI_Moldorm mNew = g.GetComponent<EnemyAI_Moldorm>();
            mNew.transform.parent = transform.parent;
            mX.Prev = mNew;
            mNew.Next = mX;

            Enemy e = g.GetComponent<Enemy>();
            if (_enemy.DungeonRoomRef != null)
            {
                _enemy.DungeonRoomRef.AddEnemy(e);
            }

            if (segmentSprite != null)
            {
                e.enemyAnim.GetComponent<SpriteRenderer>().sprite = segmentSprite;
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
            Prev.MoveDirection = MoveDirection;
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
        _radialAccel += dT * (waveSpeed + jitterAmount);
        float sin = dT * turnSpeed * Mathf.Sin(_radialAccel);
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