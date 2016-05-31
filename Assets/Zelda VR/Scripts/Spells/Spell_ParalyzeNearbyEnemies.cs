using UnityEngine;

public class Spell_ParalyzeNearbyEnemies : Spell_Base 
{
    public override void Cast(GameObject target)
    {
        base.Cast(target);

        PlayerC.ParalyzeAllNearbyEnemies(duration);
    }
}