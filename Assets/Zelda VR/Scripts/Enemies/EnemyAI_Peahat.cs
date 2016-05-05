using UnityEngine;
using System.Collections;

public class EnemyAI_Peahat : EnemyAI
{
    const float MIN_ALTITUDE = 0.5f;            // Relative to ground
    const float ROTATION_SPEED = 0.5f;


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
        _flyDirection = EnforceBoundary(EnemyAI_Random.GetRandomTileDirection());
        
        yield return new WaitForSeconds(_flyDuration);

        StartCoroutine("Idle");
    }

    IEnumerator Idle()
    {
        _state = State.Idle;
        SetPropellarSpeed(0.0f);
        _flyDirection = Vector3.zero;

        yield return new WaitForSeconds(_idleDuration);

        StartCoroutine("Fly");
    }


	void Update ()
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
            _flyDirection = Quaternion.Euler(0, ROTATION_SPEED, 0) * _flyDirection;

            Vector3 pos = transform.position;

            float speed = animSpeed * _enemy.speed;
            pos.x += _flyDirection.x * speed;
            pos.z += _flyDirection.z * speed;

            transform.position = pos;
        }
	}


    void SetPropellarSpeed(float speedPcnt)
    {
        AnimatorInstance.speed = speedPcnt;

        // The faster Peahat goes, the higher he flies
        float altitude = WorldOffsetY + MIN_ALTITUDE + (maxAltitude - MIN_ALTITUDE) * (speedPcnt * speedPcnt);
        transform.SetY(altitude);
    }
}