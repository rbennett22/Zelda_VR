using UnityEngine;

public class Spell_Invincibility : Spell_Base 
{
    public override void Cast(GameObject target)
    {
        base.Cast(target);

        // TODO

        PlayerC.MakeInvincible(duration);
    }
}