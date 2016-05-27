using UnityEngine;

public class EnemyAI_Keese : EnemyAI
{
    public float speedShiftFrequency = 1;
    public float phaseOffset = Mathf.PI;


    float _baseSpeed;
    float _startTime;


    void Start()
    {
        _baseSpeed = _enemyMove.speed;
        _startTime = Time.time;
    }


    void Update()
    {
        if (!_doUpdate) { return; }
        if (IsPreoccupied) { return; }

        float time = Time.time - _startTime;
        float theta = time * speedShiftFrequency + phaseOffset;
        _enemyMove.speed = _baseSpeed * (1 + Mathf.Cos(theta));
    }
}