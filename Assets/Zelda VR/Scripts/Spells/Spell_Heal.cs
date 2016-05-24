using UnityEngine;

public class Spell_Heal : Spell_Base 
{
    [SerializeField]
    uint amount = 1;


    public override void Cast(GameObject target)
    {
        base.Cast(target);

        HealthController hc = target.GetComponent<HealthController>();
        if(hc == null)
        {
            Debug.LogError("Cannot heal target '" + target.name + "'");
            return;
        }
        hc.RestoreHealth(amount);

        //CommonObjects.Player_C.RestoreHearts(1);
    }
}