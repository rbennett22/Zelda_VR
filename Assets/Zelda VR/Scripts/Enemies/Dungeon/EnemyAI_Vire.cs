using UnityEngine;

public class EnemyAI_Vire : EnemyAI
{
    const float KEESE_SPAWN_POS_Y = 0.5f;
    const uint NUM_KEESE_TO_SPAWN_ON_DEATH = 2;


    public GameObject redKeesePrefab;


    public void SpawnKeese()
    {
        for (int i = 0; i < NUM_KEESE_TO_SPAWN_ON_DEATH; i++)
        {
            SpawnSingleKeese();
        }
    }

    void SpawnSingleKeese()
    {
        GameObject g = Instantiate(redKeesePrefab, transform.position, Quaternion.identity) as GameObject;
        g.name = redKeesePrefab.name;

        Transform t = g.transform;
        t.SetParent(transform.parent);
        t.SetY(WorldOffsetY + KEESE_SPAWN_POS_Y);

        g.GetComponent<HealthController>().ActivateTempInvinc();

        DungeonRoom dr = _enemy.DungeonRoomRef;
        if (dr != null)
        {
            dr.AddEnemy(g.GetComponent<Enemy>());
        }
    }
}