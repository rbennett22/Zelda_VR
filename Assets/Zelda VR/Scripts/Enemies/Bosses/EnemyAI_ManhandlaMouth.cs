using System.Collections;
using UnityEngine;

public class EnemyAI_ManhandlaMouth : EnemyAI
{
    const float CHANCE_TO_DELAY_ATTACK = 0.07f;


    bool _delayingAttack;


    void Update()
    {
        if (!_doUpdate) { return; }
        if (IsPreoccupied) { return; }

        if (!_delayingAttack)
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

    IEnumerator DelayAttack()
    {
        _delayingAttack = true;
        yield return new WaitForSeconds(0.5f);
        _delayingAttack = false;
    }

    void Attack()
    {
        _enemy.Attack(DirectionToPlayer);
    }
}