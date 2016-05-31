using UnityEngine;

public class Spell_ParalyzeNearbyEnemies : Spell_Base 
{
    public override void Cast(GameObject target)
    {
        base.Cast(target);

        foreach (Enemy enemy in GetEnemiesInCurrentRoomOrSector())
        {
            enemy.Paralyze(duration);
        }
    }
}