using UnityEngine;
using System.Diagnostics;
using System.Collections;
using Immersio.Utility;

public class EnemySpawnManager : Singleton<MonoBehaviour>
{
    public float updateInterval_ms = 500;
    public float enemyRemovalDistance;      // How far away Enemy must be from player before it is destroyed (Overworld only)


    OVRPlayerController _ovrPlayerController;
    Transform _enemiesContainer;
    float _enemyRemovalDistanceSq;


	void Start () 
    {
        _ovrPlayerController = GameObject.Find("OVRPlayerController").GetComponent<OVRPlayerController>();
        _enemiesContainer = GameObject.Find("Enemies").transform;

        _enemyRemovalDistanceSq = enemyRemovalDistance * enemyRemovalDistance;

        StartCoroutine("Update_Coroutine");
    }


    IEnumerator Update_Coroutine()
    {
        while (true)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            {
                Tick();
            }
            stopWatch.Stop();

            int elapsedTime = stopWatch.Elapsed.Milliseconds;
            float waitTime = 0.001f * Mathf.Max(0, updateInterval_ms - elapsedTime);
            yield return new WaitForSeconds(waitTime);
        }
    }

    void Tick()
    {
        foreach (Transform child in transform)
        {
            child.GetComponent<EnemySpawnPoint>().DoUpdate();
        }

        Vector3 playerPos = _ovrPlayerController.transform.position;

        foreach (Transform child in _enemiesContainer)
        {
            Vector3 toPlayer = playerPos - child.position;
            float distToPlayerSqr = toPlayer.sqrMagnitude;
            if (distToPlayerSqr > _enemyRemovalDistanceSq)
            {
                Destroy(child.gameObject);  
            }
        }   
    }

}