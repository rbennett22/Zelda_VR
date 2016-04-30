using UnityEngine;
using System.Collections;

public class EnemyAI_ManhandlaMouth : EnemyAI 
{

    bool _delayingAttack;
    float _chanceToDelayAttack = 70;


	void Update ()
    {
        if (!_doUpdate) { return; }

        bool isPreoccupied = (_enemy.IsAttacking || _enemy.IsSpawning || _enemy.IsParalyzed || _enemy.IsStunned);
        if (isPreoccupied) { return; }

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
        Vector3 toPlayer = _enemy.PlayerController.transform.position - transform.position;
        toPlayer.Normalize();

        _enemy.weapon.Fire(toPlayer);
    }

}