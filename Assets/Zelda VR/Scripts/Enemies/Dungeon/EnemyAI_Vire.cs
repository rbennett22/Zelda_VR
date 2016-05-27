using UnityEngine;

public class EnemyAI_Vire : EnemyAI
{
    const float KEESE_SPAWN_POS_Y = 0.5f;


    public GameObject redKeesePrefab;


    public void SpawnKeese()
    {
        SpawnSingleKeese();
        SpawnSingleKeese();
    }
    void SpawnSingleKeese()
    {
        GameObject keese = Instantiate(redKeesePrefab, transform.position, Quaternion.identity) as GameObject;
        keese.name = redKeesePrefab.name;

        keese.transform.parent = transform.parent;
        keese.transform.SetY(WorldOffsetY + KEESE_SPAWN_POS_Y);

        keese.GetComponent<HealthController>().ActivateTempInvinc();

        if (_enemy.DungeonRoomRef != null)
        {
            _enemy.DungeonRoomRef.AddEnemy(keese.GetComponent<Enemy>());
        }
    }
}