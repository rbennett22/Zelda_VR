using System.Collections;
using UnityEngine;

public class EnemyAI_Peahat : EnemyAI
{
    const float MIN_ALTITUDE = 0.5f;            // Relative to ground
    const float ROTATION_SPEED = 0.5f;


    public float baseSpeed = 1;
    public float maxAltitude = 5.0f;            // Relative to ground


    float _idleDuration = 2.0f;
    float _flyDuration = 10.0f;
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
            _flyDirection = deltaRot * _flyDirection;

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
        float altitude = WorldOffsetY + Mathf.Lerp(MIN_ALTITUDE, maxAltitude, pcnt);
        transform.SetY(altitude);
    }
}