using UnityEngine;
using System.Collections;

public class EnemyAI_Leever : EnemyAI 
{
    const float OffscreenOffset = -30;      // How far to offset the Leever's y position when it is underground
    const float TileOffset = 0.5f;
    const float Epsilon = 0.001f;


    public float undergroundDuration = 2.0f;
    public float emergeDuration = 1.0f;
    public float surfaceDuration = 3.0f;
    public float submergeDuration = 1.0f;

    public Animator animator;
    public EnemyAI_Random enemyAI_Random;


    float _startTime = float.NegativeInfinity;
    float _timerDuration;
    Vector3 _origin;


    public bool IsUnderground { get { return animator.GetCurrentAnimatorStateInfo(0).IsTag("Underground"); } }
    public bool IsEmerging { get { return animator.GetCurrentAnimatorStateInfo(0).IsTag("Emerge"); } }
    public bool IsSubmerging { get { return animator.GetCurrentAnimatorStateInfo(0).IsTag("Submerge"); } }
    public bool IsSurfaced { get { return animator.GetCurrentAnimatorStateInfo(0).IsTag("Surface"); } }


    protected void Awake()
    {
        base.Awake();

        if (enemyAI_Random != null) { enemyAI_Random.enabled = false; }
    }

    void Start()
    {
        _origin = transform.position;
        animator.GetComponent<Renderer>().enabled = false;
        GetComponent<HealthController>().isIndestructible = true;
    }


    void Update()
    {
        if (!_doUpdate) { return; }

        transform.SetY(0);

        bool isPreoccupied = (_enemy.IsStunned || _enemy.IsParalyzed);
        if (isPreoccupied) { return; }

        bool timesUp = (Time.time - _startTime >= _timerDuration);
        if (timesUp)
        {
            if (IsUnderground)
            {
                animator.SetTrigger("Emerge");
                _timerDuration = emergeDuration;
                animator.GetComponent<Renderer>().enabled = true;
                transform.SetY(_origin.y);
                WarpToRandomNearbySandTile();
            }
            else if (IsEmerging)
            {
                animator.SetTrigger("Surface");
                _timerDuration = surfaceDuration;
                GetComponent<HealthController>().isIndestructible = false;
                enemyAI_Random.enabled = true;
                enemyAI_Random.TargetPosition = transform.position;
            }
            else if (IsSurfaced)
            {
                animator.SetTrigger("Submerge");
                _timerDuration = submergeDuration;
                GetComponent<HealthController>().isIndestructible = true;
                enemyAI_Random.enabled = false;
            }
            else if (IsSubmerging)
            {
                animator.SetTrigger("Underground");
                _timerDuration = undergroundDuration;
                animator.GetComponent<Renderer>().enabled = false;
                transform.SetY(_origin.y - OffscreenOffset);  // Move offscreen to prevent collision with player
            }

            _startTime = Time.time;
        }
    }


    int maxWarpDistanceFromOrigin = 5;
    int maxAttempts = 20;
    void WarpToRandomNearbySandTile()
    {
        if (WorldInfo.Instance.IsSpecial) { return; }

        TileMap tileMap = CommonObjects.OverworldTileMap;
        if (tileMap == null) { return; }

        int newX, newZ;
        bool isSand;
        int count = 0;
        do {
            newX = (int)((int)_origin.x + Random.Range(-maxWarpDistanceFromOrigin, maxWarpDistanceFromOrigin + 1) + Epsilon);
            newZ = (int)((int)_origin.z + Random.Range(-maxWarpDistanceFromOrigin, maxWarpDistanceFromOrigin + 1) + Epsilon);
            int tileCode = tileMap.Tile(newX, newZ);
            isSand = TileInfo.IsTileSand(tileCode);
        } while (!isSand && ++count < maxAttempts);

        if (isSand)
        {
            transform.SetX(newX + TileOffset);
            transform.SetZ(newZ + TileOffset);
        }
    }

}