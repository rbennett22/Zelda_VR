using System.Collections;
using UnityEngine;

public class EnemyAI_GleeokHead : EnemyAI
{
    const int NUM_NECK_SEGMENTS = 3;

    const float HORZ_AMPLITUDE = 1.0f;
    const float VERT_AMPLITUDE = 0.4f;
    const float Z_AMPLITUDE = 0.2f;

    const float CHANCE_TO_DELAY_ATTACK = 0.07f;
    const float DELAY_ATTACK_DURATION = 0.5f;


    public EnemyAI_Gleeok gleeok;
    public Transform neckMarker;
    public Transform[] neckSegments = new Transform[NUM_NECK_SEGMENTS];
    public float phaseOffset;


    float _startTime;
    bool _delayingAttack;
    
    float _baseLocalY, _baseLocalZ;


    void Start()
    {
        _startTime = Time.time;

        _baseLocalY = transform.localPosition.y;
        _baseLocalZ = transform.localPosition.z;
    }


    void Update()
    {
        if (!_doUpdate) { return; }

        Oscillate();

        if (IsPreoccupied) { return; }

        if (!_enemy.IsAttacking && !_delayingAttack)
        {
            if (Extensions.FlipCoin(CHANCE_TO_DELAY_ATTACK))
            {
                StartCoroutine("DelayAttack");
            }
            else
            {
                Attack();
            }
        }
    }

    void Oscillate()
    {
        float t = Time.time - _startTime;
        float thetaX = t * _enemyMove.speed + phaseOffset;
        float thetaY = t * _enemyMove.speed * 1.3f + phaseOffset;

        float x = HORZ_AMPLITUDE * Mathf.Sin(thetaX);
        float y = _baseLocalY + VERT_AMPLITUDE * Mathf.Sin(thetaY);
        float z = _baseLocalZ + Z_AMPLITUDE * Mathf.Sin(thetaY);
        transform.localPosition = new Vector3(x, y, z);

        // Neck Segments
        Vector3 headPos = transform.position;
        Vector3 toBody = neckMarker.position - headPos;
        for (int i = 0; i < neckSegments.Length; i++)
        {
            neckSegments[i].position = headPos + toBody * ((i + 1.0f) / (neckSegments.Length + 1));
        }
    }


    IEnumerator DelayAttack()
    {
        _delayingAttack = true;
        yield return new WaitForSeconds(DELAY_ATTACK_DURATION);
        _delayingAttack = false;
    }

    void Attack()
    {
        _enemy.Attack(DirectionToPlayer);
    }
}