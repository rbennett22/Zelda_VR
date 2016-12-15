using UnityEngine;

public class EnemyAI_Ghini_Gravestone : EnemyAI
{
    public float turnSpeed = 60.0f;
    public float waveSpeed = 2.0f;
    public float jitter = 10.0f;


    float _radialAccel;


    public new Vector3 MoveDirection
    {
        get { return _enemyMove.MoveDirection; }
        set { _enemyMove.MoveDirection = value; }
    }


    void Start()
    {
        _enemyMove.Mode = EnemyMove.MovementMode.DirectionOnly;

        MoveDirection = EnemyAI_Random.GetRandomDirectionXZ();
    }


    void Update()
    {
        if (!_doUpdate) { return; }
        if (IsPreoccupied) { return; }
        if (PauseManager.Instance.IsPaused_Any) { return; }

        UpdateMoveDirection();
    }

    void UpdateMoveDirection()
    {
        float dT = Time.deltaTime;

        float jitterAmount = Random.Range(-jitter, jitter);
        _radialAccel += dT * (waveSpeed + jitterAmount);
        float sin = dT * turnSpeed * Mathf.Sin(_radialAccel);
        Quaternion deltaRad = Quaternion.Euler(0, sin, 0);

        MoveDirection = deltaRad * MoveDirection;

        MoveDirection = EnforceBoundary(MoveDirection);
    }
}
