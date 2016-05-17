using System.Collections;
using UnityEngine;

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
        AnimatorInstance.SetInteger("DungeonNum", num);
    }


    public void SpawnGels()
    {
        SpawnGel();
        SpawnGel();
    }
    void SpawnGel()
    {
        GameObject gel = Instantiate(gelPrefab, transform.position, Quaternion.identity) as GameObject;
        gel.name = gelPrefab.name;
        gel.transform.parent = transform.parent;

        gel.GetComponent<HealthController>().ActivateTempInvinc();

        if (_enemy.DungeonRoomRef != null)
        {
            _enemy.DungeonRoomRef.AddEnemy(gel.GetComponent<Enemy>());
        }
    }
}