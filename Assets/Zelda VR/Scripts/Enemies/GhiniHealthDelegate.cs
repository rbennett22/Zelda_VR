using UnityEngine;
using System.Collections;


public class GhiniHealthDelegate : MonoBehaviour
{

    void OnEnemyDeath()
    {
        GameObject enemies = GameObject.FindGameObjectWithTag("Enemies");
        foreach (Transform enemy in enemies.transform)
        {
            if (enemy.name.Contains("Ghini grave"))
            {
                HealthController health = enemy.GetComponent<HealthController>();
                health.isIndestructible = false;
                health.Kill(null, true);
            }
        }
    }

}