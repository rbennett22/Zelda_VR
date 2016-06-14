using UnityEngine;
using Immersio.Utility;

public class GhiniHealthDelegate : MonoBehaviour
{
    void OnEnemyDeath()
    {
        Enemy enemy = GetComponent<Enemy>();
        Index2 mySector = enemy.Sector;

        foreach (Enemy e in Actor.FindObjectsInSector<Enemy>(mySector))
        {
            if (e.name.Contains("grave"))
            {
                HealthController hc = e.GetComponent<HealthController>();
                hc.isIndestructible = false;
                hc.Kill(null, true);
            }
        }

        foreach (EnemySpawnPoint sp in Actor.FindObjectsInSector<EnemySpawnPoint>(mySector))
        {
            if (sp.name.Contains("grave"))
            {
                sp.ForceCooldown();
            }
        }
    }
}