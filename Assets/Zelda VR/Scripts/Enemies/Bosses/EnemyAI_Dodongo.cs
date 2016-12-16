using System.Collections;
using UnityEngine;

public class EnemyAI_Dodongo : EnemyAI
{
    public bool StunnedByBomb {
        get { return AnimatorInstance.GetBool("Bombed"); }
        private set { AnimatorInstance.SetBool("Bombed", value); }
    }


    void OnTriggerEnter(Collider otherCollider)
    {
        GameObject other = otherCollider.gameObject;
        //print("EnemyAI_Dodongo::OnTriggerEnter: " + other.name);

        Projectile_Bomb bomb = other.GetComponent<Projectile_Bomb>();
        if (bomb != null)
        {
            if (!StunnedByBomb && IsBombInFrontOfDodongo(bomb.transform))
            {
                StartCoroutine("EatBomb", bomb);
            }
        }
    }

    bool IsBombInFrontOfDodongo(Transform bomb)
    {
        Vector3 toBomb = bomb.position - transform.position;
        toBomb.Normalize();
        return Vector3.Dot(toBomb, transform.forward) > 0.5f;
    }

    IEnumerator EatBomb(Projectile_Bomb bomb)
    {
        _healthController.TakeDamage((uint)bomb.DamageDealerAOE.damage, bomb.gameObject);
        Destroy(bomb.gameObject);

        float duration = _healthController.tempInvincibilityDuration;

        _enemy.Paralyze(duration, false);
        StunnedByBomb = true;

        yield return new WaitForSeconds(duration);

        StunnedByBomb = false;
    }
}