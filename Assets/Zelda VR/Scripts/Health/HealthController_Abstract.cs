using UnityEngine;

public abstract class HealthController_Abstract : MonoBehaviour
{
    public abstract void SetHealth(int newHealth);
    public abstract bool RestoreHealth(uint healAmount);
    public abstract bool TakeDamage(uint damageAmount, GameObject damageDealer);        // TODO: IDamageDealer
    public abstract bool Kill(GameObject killer);                                       // TODO: IDamageDealer
}