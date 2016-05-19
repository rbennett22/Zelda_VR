using UnityEngine;

public class DamageDealer_OnCollisionEnter : DamageDealer_Base 
{
    void OnCollisionEnter(Collision collision)
    {
        GameObject other = collision.gameObject;
        HealthController_Abstract hc = TryGetHealthController(other);
        if (hc != null)
        {
            Attack(hc);
        }
    }
}