using UnityEngine;
using System.Collections.Generic;

public class Spell_Base : MonoBehaviour 
{
    public Actor caster;
    public float duration = 1.0f;


    protected Player PlayerC { get { return CommonObjects.Player_C; } }

    public virtual void Cast(GameObject target = null) { }


    public List<Enemy> GetEnemiesInCurrentRoomOrSector()
    {
        List<Enemy> enemies = new List<Enemy>();

        if (WorldInfo.Instance.IsOverworld)
        {
            TileMap tm = CommonObjects.OverworldTileMap;
            Rect sBounds = tm.GetBoundsForSector(PlayerC.GetOccupiedOverworldSector());
            GameObject enemiesContainer = GameObject.FindGameObjectWithTag("Enemies");
            foreach (Enemy e in enemiesContainer.GetComponentsInChildren<Enemy>())
            {
                Vector3 p = e.transform.position;
                Vector2 p2 = new Vector2(p.x, p.z);
                if (sBounds.Contains(p2))
                {
                    enemies.Add(e);
                }
            }
        }
        else if (WorldInfo.Instance.IsInDungeon)
        {
            enemies = PlayerC.GetOccupiedDungeonRoom().Enemies;
        }

        return enemies;
    }
}