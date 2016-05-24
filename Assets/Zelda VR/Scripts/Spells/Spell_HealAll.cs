using UnityEngine;

public class Spell_HealAll : Spell_Base 
{
    public override void Cast(GameObject target)
    {
        base.Cast(target);

        HealthController hc = target.GetComponent<HealthController>();
        if(hc == null)
        {
            Debug.LogError("Cannot heal target '" + target.name + "'");
            return;
        }
        hc.RestoreHealth();
    }
}