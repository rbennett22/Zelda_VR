using UnityEngine;


[RequireComponent(typeof(EnemySpawnPoint))]

public class DigdoggerSpawnPoint : MonoBehaviour
{

    public int numBabies = 3;


    void OnEnemySpawned(Enemy enemy)
    {
        EnemyAI_Digdogger digdogger = enemy.GetComponent<EnemyAI_Digdogger>();
        digdogger.numBabies = numBabies;
	}

}