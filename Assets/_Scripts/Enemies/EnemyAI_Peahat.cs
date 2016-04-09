using UnityEngine;
using System.Collections;


public class EnemyAI_Peahat : EnemyAI
{
    const float BaseY = 0.5f;
    const float RotationSpeed = 0.5f;


    public EnemyAnimation enemyAnimation;
    public float maxAltitude = 5.0f;


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

        _flyDirection = EnforceBoundary(GetRandomMoveDirection());
        
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
        if (_enemy.IsParalyzed || _enemy.IsStunned) { return; }

        if (_state == State.Flying)
        {
            // Speed
            float dT = ((Time.time - _flyStartTime) / _flyDuration) * Mathf.PI;
            float animSpeed = Mathf.Sin(dT);
            SetPropellarSpeed(animSpeed);

            // Direction
            _flyDirection = Quaternion.Euler(0, RotationSpeed, 0) * _flyDirection;
            Vector3 pos = transform.position;
            pos.x += _flyDirection.x * animSpeed * _enemy.speed;
            pos.y += _flyDirection.y * animSpeed * _enemy.speed;
            transform.position = pos;
        }
	}


    void SetPropellarSpeed(float speedPcnt)
    {
        enemyAnimation.AnimatorInstance.speed = speedPcnt;

        // The faster Peahat goes, the higher he flies
        float altitude = BaseY + (maxAltitude - BaseY) * (speedPcnt * speedPcnt);
        transform.SetY(altitude);
    }

    Vector3 GetRandomMoveDirection()
    {
        int angle = Random.Range(0, 4) * 90;
        Vector3 dir = Quaternion.Euler(0, angle, 0) * new Vector3(1, 0, 0);
        return dir;
    }

}