using UnityEngine;
using System.Collections.Generic;


[RequireComponent(typeof(EnemySpawnPoint))]

public class ArmosSpawnPoint : MonoBehaviour
{
    public EnemyAI_Armos.StatueType statueType;
    public bool hidesCollectibleItem;


    void OnEnemySpawned(Enemy enemy)
    {
        EnemyAI_Armos armos = enemy.GetComponent<EnemyAI_Armos>();
        armos.Type = statueType;
        armos.HidesCollectibleItem = hidesCollectibleItem;

        // Any children of the spawnPoint are considered linkedTiles
        if (transform.childCount > 0)
        {
            List<GameObject> linkedTiles = new List<GameObject>();
            foreach (Transform child in transform)
            {
                linkedTiles.Add(child.gameObject);
            }
            armos.linkedTiles = linkedTiles.ToArray();
        }
	}

}