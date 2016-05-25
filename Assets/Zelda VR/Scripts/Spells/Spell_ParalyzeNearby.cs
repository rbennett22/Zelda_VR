using UnityEngine;

public class Spell_ParalyzeNearby : Spell_Base 
{
    public override void Cast(GameObject target)
    {
        base.Cast(target);

        // TODO

        if (WorldInfo.Instance.IsOverworld)
        {
            GameObject enemiesContainer = GameObject.FindGameObjectWithTag("Enemies");
            foreach (Transform child in enemiesContainer.transform)
            {
                Enemy enemy = child.GetComponent<Enemy>();
                if (enemy != null)
                {
                    enemy.Paralyze(duration);
                }
            }
        }
        else if (WorldInfo.Instance.IsInDungeon)
        {
            DungeonRoom room = PlayerC.GetOccupiedDungeonRoom();
            foreach (Enemy enemy in room.Enemies)
            {
                enemy.Paralyze(duration);
            }
        }
    }
}