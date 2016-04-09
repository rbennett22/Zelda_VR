using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Immersio.Utility;


public class TileProliferator : Singleton<TileProliferator>
{
    public TileMap tileMap;
    public int UpdateInterval_ms = 1000;     
    public int tileRemovalDistance;


    Transform _playerTransform;
    int _tileRemovalDistanceSq;


    void Awake()
    {
        _tileRemovalDistanceSq = tileRemovalDistance * tileRemovalDistance;

        _playerTransform = CommonObjects.PlayerController_G.transform;
    }

	void Start () 
    {
        tileMap.LoadMap();

        StartCoroutine("UpdateTiles_Coroutine");
	}


    IEnumerator UpdateTiles_Coroutine()
    {
        while (true)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            {
                UpdateTiles_Tick();
            }
            stopWatch.Stop();

            int elapsedTime = stopWatch.Elapsed.Milliseconds;
            float waitTime = 0.001f * Mathf.Max(0, UpdateInterval_ms - elapsedTime);
            yield return new WaitForSeconds(waitTime);
        }
	}

    void UpdateTiles_Tick()
    {
        Vector3 playerPos = _playerTransform.position;
        Transform blockContainer = GameObject.Find("Blocks").transform;

        // Remove blocks
        foreach (Transform block in blockContainer)
        {
            Vector3 blockPos = block.position;
            Vector3 toPlayer = playerPos - blockPos;
            float distToPlayerSqr = toPlayer.sqrMagnitude;
            if (distToPlayerSqr > _tileRemovalDistanceSq)
            {
                tileMap.RemoveBlock((int)blockPos.x, (int)blockPos.z, block.gameObject);
            }
        }

        // Populate blocks
        int extent = (int)(tileRemovalDistance / Mathf.Sqrt(2));
        tileMap.PopulateWorld(
            (int)playerPos.x - extent, 
            (int)playerPos.z - extent,
            extent * 2, extent * 2
            );


        // Special Collectibles      TODO: place this code elsewhere
        OverworldInfo owInfo = GameObject.FindGameObjectWithTag("OverworldInfo").GetComponent<OverworldInfo>();

        foreach (Transform child in owInfo.collectibleSPs)
        {
            CollectibleSpawnPoint csp = child.GetComponent<CollectibleSpawnPoint>();
            if (csp == null) { continue; }

            float distSqr = (csp.transform.position - playerPos).sqrMagnitude;
            if (distSqr > _tileRemovalDistanceSq)
            {
                if (csp.SpawnedCollectible != null)
                {
                    csp.DestroySpawnedCollectible();
                }
            }
            else
            {
                if (csp.SpawnedCollectible == null && !csp.HasBeenCollected)
                {
                    csp.SpawnCollectible();
                }
            }
        }
    }

}
