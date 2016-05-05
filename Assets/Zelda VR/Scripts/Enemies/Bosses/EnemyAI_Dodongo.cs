using UnityEngine;
using System.Collections;

public class EnemyAI_Dodongo : EnemyAI 
{
    public bool StunnedByBomb { get { return AnimatorInstance.GetBool("Bombed"); } }


    void OnTriggerEnter(Collider otherCollider)
    {
        GameObject other = otherCollider.gameObject;
        //print("EnemyAI_Dodongo::OnTriggerEnter: " + other.name);

        Bomb bomb = other.GetComponent<Bomb>();
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

    IEnumerator EatBomb(Bomb bomb)
    {
        _healthController.TakeDamage((uint)bomb.explosionDamage, bomb.gameObject);
        Destroy(bomb.gameObject);

        float duration = _healthController.tempInvincibilityDuration;

        _enemy.Paralyze(duration);
        AnimatorInstance.SetBool("Bombed", true);

        yield return new WaitForSeconds(duration);

        AnimatorInstance.SetBool("Bombed", false);
    }

}