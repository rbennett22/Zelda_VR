using UnityEngine;
using System.Collections;

public class EnemyAI_RiverZora : EnemyAI 
{
    public float underwaterDuration = 2.0f;
    public float emergeDuration = 1.0f;
    public float surfaceDuration = 3.0f;
    public float submergeDuration = 1.0f;

    public Animator animator;


    float _startTime = float.NegativeInfinity;
    float _timerDuration;
    Vector3 _origin;


    public bool IsUnderwater { get { return animator.GetCurrentAnimatorStateInfo(0).IsTag("Underwater"); } }
    public bool IsEmerging { get { return animator.GetCurrentAnimatorStateInfo(0).IsTag("Emerge"); } }
    public bool IsSubmerging { get { return animator.GetCurrentAnimatorStateInfo(0).IsTag("Submerge"); } }
    public bool IsSurfaced { get { return animator.GetCurrentAnimatorStateInfo(0).IsTag("Surface"); } }


    void Start()
    {
        transform.SetY(0);
        _origin = transform.position;
        animator.GetComponent<Renderer>().enabled = false;
        GetComponent<HealthController>().isIndestructible = true;
    }


    void Update()
    {
        transform.SetY(0);

        if (!_doUpdate) { return; }

        bool isPreoccupied = (_enemy.IsStunned || _enemy.IsParalyzed);
        if (isPreoccupied) { return; }

        bool timesUp = (Time.time - _startTime >= _timerDuration);
        if (timesUp)
        {
            if (IsUnderwater)
            {
                animator.SetTrigger("Emerge");
                _timerDuration = emergeDuration;
                animator.GetComponent<Renderer>().enabled = true;
                WarpToRandomNearbyWaterTile();
            }
            else if (IsEmerging)
            {
                animator.SetTrigger("Surface");
                _timerDuration = surfaceDuration;
                GetComponent<HealthController>().isIndestructible = false;
                FacePlayer();
                _enemy.Attack();
            }
            else if (IsSurfaced)
            {
                animator.SetTrigger("Submerge");
                _timerDuration = submergeDuration;
                GetComponent<HealthController>().isIndestructible = true;
            }
            else if (IsSubmerging)
            {
                animator.SetTrigger("Underwater");
                _timerDuration = underwaterDuration;
                animator.GetComponent<Renderer>().enabled = false;
                ReplenishHealth();
            }

            _startTime = Time.time;
        }

        if (IsSurfaced)
        {
            FacePlayer();
        }
    }

    void ReplenishHealth()
    {
        GetComponent<HealthController>().RestoreHealth();
    }

    void FacePlayer()
    {
        Vector3 toPlayer = _enemy.PlayerController.transform.position - transform.position;
        toPlayer.Normalize();
        transform.forward = toPlayer;
    }

    int maxWarpDistanceFromOrigin = 5;
    int maxAttempts = 20;
    void WarpToRandomNearbyWaterTile()
    {
        if (WorldInfo.Instance.IsSpecial) { return; }

        TileMap tileMap = TileProliferator.Instance.tileMap;

        int newX, newZ;
        bool isWater;
        int count = 0;
        do {
            newX = (int)((int)_origin.x + Random.Range(-maxWarpDistanceFromOrigin, maxWarpDistanceFromOrigin + 1) + Epsilon);
            newZ = (int)((int)_origin.z + Random.Range(-maxWarpDistanceFromOrigin, maxWarpDistanceFromOrigin + 1) + Epsilon);
            int tileCode = tileMap.Tile(newX, newZ);
            isWater = TileInfo.IsTileWater(tileCode);
        } while (!isWater && ++count < maxAttempts);

        if (isWater)
        {
            transform.SetX(newX + TileOffset);
            transform.SetZ(newZ + TileOffset);
        }
    }

}