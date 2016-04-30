using UnityEngine;

public class EnemyAI_Vire : EnemyAI 
{

    public GameObject redKeesePrefab;


    public void SpawnKeese()
    {
        GameObject keese1 = Instantiate(redKeesePrefab, transform.position, Quaternion.identity) as GameObject;
        GameObject keese2 = Instantiate(redKeesePrefab, transform.position, Quaternion.identity) as GameObject;
        keese1.name = redKeesePrefab.name;
        keese2.name = redKeesePrefab.name;
        keese1.transform.parent = transform.parent;
        keese2.transform.parent = transform.parent;

        keese1.transform.SetY(0.5f);
        keese2.transform.SetY(0.5f);

        keese1.GetComponent<HealthController>().ActivateTempInvinc();
        keese2.GetComponent<HealthController>().ActivateTempInvinc();

        if (_enemy.DungeonRoomRef != null)
        {
            _enemy.DungeonRoomRef.AddEnemy(keese1.GetComponent<Enemy>());
            _enemy.DungeonRoomRef.AddEnemy(keese2.GetComponent<Enemy>());
        }
    }

}