using UnityEngine;

public class TileProliferator : MonoBehaviour
{
    public TileMap tileMap;
    public int UpdateInterval_ms = 1000;


    /*int _tileRemovalDistance, _tileRemovalDistanceSq;

    Transform _playerTransform;
    Transform _blocksContainer;


    void Awake()
    {
        _playerTransform = CommonObjects.PlayerController_G.transform;
        _blocksContainer = GameObject.Find("Blocks").transform;

        _tileRemovalDistance = ZeldaVRSettings.Instance.tileRemovalDistance;
        _tileRemovalDistanceSq = _tileRemovalDistance * _tileRemovalDistance;
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

                Resources.UnloadUnusedAssets();
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

        // Remove blocks
        foreach (Transform block in _blocksContainer)
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
        int extent = (int)(_tileRemovalDistance / Mathf.Sqrt(2));
        tileMap.PopulateWorld(
            (int)playerPos.x - extent,
            (int)playerPos.z - extent,
            extent * 2, extent * 2
            );
    }*/
}