using UnityEngine;
using System.Collections;

public class DamageDealer_AOE : DamageDealer_Base
{
    public float radius = 1.0f;


    public void AttackAllWithinReach()
    {
        AttackAllWithinReach(this.radius);
    }
    public void AttackAllWithinReach(float maxDistance)
    {
        StartCoroutine("AttackAllWithinReach_CR", maxDistance);
    }
    IEnumerator AttackAllWithinReach_CR(float maxDistance)
    {
        yield return new WaitForFixedUpdate();

        Vector3 center = transform.position;

        Collider[] colliders = Physics.OverlapSphere(center, maxDistance);
        foreach (Collider hit in colliders)
        {
            if (hit == null) { continue; }

            GameObject other = hit.gameObject;
            HealthController_Abstract hc = TryGetHealthController(other);
            if (hc != null)
            {
                Attack(hc);
            }
        }
    }
}