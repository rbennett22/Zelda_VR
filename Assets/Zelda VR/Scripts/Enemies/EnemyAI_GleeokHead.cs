using UnityEngine;
using System.Collections;


public class EnemyAI_GleeokHead : EnemyAI 
{
    const float HorizontalAmplitude = 1.0f;
    const float VerticalAmplitude = 0.4f;


    public EnemyAI_Gleeok gleeok;
    public Transform neckMarker;
    public Transform[] neckSegments = new Transform[3];
    public float phaseOffset;


    float _startTime;
    bool _delayingAttack;
    float _chanceToDelayAttack = 70;
    float _baseLocalY, _baseLocalZ;


    void Start()
    {
        _startTime = Time.time;
        _baseLocalY = transform.localPosition.y;
        _baseLocalZ = transform.localPosition.z;
    }


	void Update ()
    {
        if (!_doUpdate) { return; }

        bool isPreoccupied = (_enemy.IsSpawning || _enemy.IsParalyzed || _enemy.IsStunned);
        if (isPreoccupied) { return; }

        Oscillate();

        if (!_enemy.IsAttacking)
        {
            if (!_delayingAttack)
            {
                int rand = Random.Range(0, 100);
                if (rand < _chanceToDelayAttack)
                {
                    StartCoroutine("DelayAttack");
                }
                else
                {
                    Attack();
                }
            }
        }
	}

    void Oscillate()
    {
        float time = Time.time - _startTime;
        float x = HorizontalAmplitude * Mathf.Sin(time * _enemy.speed + phaseOffset);
        float y = _baseLocalY + VerticalAmplitude * Mathf.Sin(time * _enemy.speed * 1.3f + phaseOffset);
        float z = _baseLocalZ + 0.2f * Mathf.Sin(time * _enemy.speed * 1.3f + phaseOffset);
        transform.localPosition = new Vector3(x, y, z);

        // Neck Segments
        Vector3 headPos = transform.position;
        Vector3 toBody = neckMarker.position - headPos;
        for (int i = 0; i < neckSegments.Length; i++)
        {
            neckSegments[i].transform.position = headPos + toBody * ((i + 1.0f) / (neckSegments.Length + 1));
        }
    }


    IEnumerator DelayAttack()
    {
        _delayingAttack = true;
        yield return new WaitForSeconds(0.5f);
        _delayingAttack = false;
    }

    void Attack()
    {
        Vector3 toPlayer = _enemy.PlayerController.transform.position - transform.position;
        toPlayer.Normalize();

        _enemy.weapon.Fire(toPlayer);
    }

}