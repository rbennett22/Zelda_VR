using System.Collections;
using UnityEngine;

public class EnemyAI_ManhandlaMouth : EnemyAI
{
    bool _delayingAttack;
    float _chanceToDelayAttack = 70;


    void Update()
    {
        if (!_doUpdate) { return; }
        if (IsPreoccupied) { return; }

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

    IEnumerator DelayAttack()
    {
        _delayingAttack = true;
        yield return new WaitForSeconds(0.5f);
        _delayingAttack = false;
    }

    void Attack()
    {
        _enemy.Attack(ToPlayer);
    }
}