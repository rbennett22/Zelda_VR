using UnityEngine;
using System.Linq;

[RequireComponent(typeof(EnemySpawnPoint))]

public class ArmosSpawnPoint : MonoBehaviour
{
    public EnemyAI_Armos.StatueType statueType;
    public bool hidesCollectibleItem;


    void OnEnemySpawned(Enemy enemy)
    {
        EnemyAI_Armos r = enemy.GetComponent<EnemyAI_Armos>();
        r.Type = statueType;
        r.HidesCollectibleItem = hidesCollectibleItem;
        r.linkedTiles = GetLinkedTilesFromChildren();
    }

    GameObject[] GetLinkedTilesFromChildren()
    {
        // (we assume all children are linked tiles)
        return GetComponentsInChildren<Transform>()
            .Where(c => c != this.transform)
            .Select(t => t.gameObject)
            .ToArray();
    }
}