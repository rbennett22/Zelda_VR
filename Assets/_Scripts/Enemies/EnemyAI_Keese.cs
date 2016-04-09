using UnityEngine;
using System.Collections;

public class EnemyAI_Keese : EnemyAI 
{
    public float speedShiftFrequency = 1;
    public float phaseOffset = Mathf.PI; 


    float _baseSpeed;
    float _startTime;


    void Start()
    {
        _baseSpeed = _enemy.speed;
        _startTime = Time.time;
    }


    void Update()
    {
        if (!_doUpdate) { return; }

        float time = Time.time - _startTime;
        _enemy.speed = _baseSpeed * (1 + Mathf.Cos(time * speedShiftFrequency + phaseOffset));
    }

}