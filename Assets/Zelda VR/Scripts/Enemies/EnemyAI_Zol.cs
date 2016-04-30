using UnityEngine;
using System.Collections;


public class EnemyAI_Zol : EnemyAI 
{

    public GameObject gelPrefab;


    void Start()
    {
        StartCoroutine("WaitForSpawnToFinish");
    }

    IEnumerator WaitForSpawnToFinish()
    {
        while (_enemy.enemyAnim.IsSpawning)
        {
            yield return new WaitForSeconds(0.02f);
        }
        AssignSkinForDungeonNum(WorldInfo.Instance.DungeonNum);
    }

    void AssignSkinForDungeonNum(int num)
    {
        if (num < 1 || num > 9) { num = 1; }
        _enemy.enemyAnim.AnimatorInstance.SetInteger("DungeonNum", num);
    }


    public void SpawnGels()
    {
        GameObject gel1 = Instantiate(gelPrefab, transform.position, Quaternion.identity) as GameObject;
        GameObject gel2 = Instantiate(gelPrefab, transform.position, Quaternion.identity) as GameObject;
        gel1.name = gelPrefab.name;
        gel2.name = gelPrefab.name;
        gel1.transform.parent = transform.parent;
        gel2.transform.parent = transform.parent;

        gel1.GetComponent<HealthController>().ActivateTempInvinc();
        gel2.GetComponent<HealthController>().ActivateTempInvinc();

        if (_enemy.DungeonRoomRef != null)
        {
            _enemy.DungeonRoomRef.AddEnemy(gel1.GetComponent<Enemy>());
            _enemy.DungeonRoomRef.AddEnemy(gel2.GetComponent<Enemy>());
        }
    }

}