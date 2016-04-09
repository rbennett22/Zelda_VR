using UnityEngine;


[RequireComponent(typeof(EnemySpawnPoint))]

public class GleeokSpawnPoint : MonoBehaviour
{

    public int numHeads = 2;


    void OnEnemySpawned(Enemy enemy)
    {
        EnemyAI_Gleeok gleeok = enemy.GetComponent<EnemyAI_Gleeok>();
        gleeok.numHeads = numHeads;
	}
}