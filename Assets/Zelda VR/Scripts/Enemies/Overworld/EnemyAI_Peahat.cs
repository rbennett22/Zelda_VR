using System.Collections;
using UnityEngine;

public class EnemyAI_Peahat : EnemyAI
{
    const float ROTATION_SPEED = 0.5f;
    const float BASE_FLY_DURATION = 10.0f;
    const float FLY_DURATION_VARIANCE = 1.0f;


    public float baseSpeed = 1;
    public float maxAltitude = 5.0f;            // Relative to ground


    float _idleDuration = 2.0f;
    float _flyDuration;
    float _flyStartTime = float.NegativeInfinity;
    Vector3 _flyDirection = Vector3.zero;


    enum State
    {
        Idle,
        Flying,
    }
    State _state = State.Idle;


    void Start()
    {
        StartCoroutine("Fly");
    }


    IEnumerator Fly()
    {
        _state = State.Flying;

        _flyStartTime = Time.time;
        _flyDirection = EnforceBoundary(EnemyAI_Random.GetRandomDirectionXZ());
        _flyDuration = BASE_FLY_DURATION + Random.Range(-FLY_DURATION_VARIANCE, FLY_DURATION_VARIANCE);

        yield return new WaitForSeconds(_flyDuration);

        StartCoroutine("Idle");
    }

    IEnumerator Idle()
    {
        _state = State.Idle;

        SetPropellarSpeed(0);
        _flyDirection = Vector3.zero;

        yield return new WaitForSeconds(_idleDuration);

        StartCoroutine("Fly");
    }


    void Update()
    {
        if (!_doUpdate) { return; }
        if (IsPreoccupied) { return; }

        if (_state == State.Flying)
        {
            // Speed
            float dT = Time.time - _flyStartTime;
            float theta = (dT / _flyDuration) * Mathf.PI;
            float animSpeed = Mathf.Sin(theta);
            SetPropellarSpeed(animSpeed);

            // Direction
            Quaternion deltaRot = Quaternion.Euler(0, ROTATION_SPEED, 0);
            _flyDirection = EnforceBoundary(deltaRot * _flyDirection);
            if (!CanMoveInDirection_Overworld(_flyDirection))
            {
                _flyDirection = -_flyDirection;
            }

            // Position
            float moveSpeed = animSpeed * baseSpeed;
            transform.position += _flyDirection * moveSpeed;
        }
    }

    void SetPropellarSpeed(float speed)
    {
        speed = Mathf.Clamp(speed, -1f, 1f);
        AnimatorInstance.speed = speed;

        SetAltitudePercent(speed * speed);   // The faster Peahat goes, the higher he flies
    }
    void SetAltitudePercent(float pcnt)
    {     
        float altitude = WorldOffsetY + Mathf.Lerp(Radius, maxAltitude, pcnt);
        transform.SetY(altitude);
    }
}