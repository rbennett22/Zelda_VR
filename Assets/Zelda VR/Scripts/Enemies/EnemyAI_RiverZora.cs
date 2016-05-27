using Immersio.Utility;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI_RiverZora : EnemyAI
{
    const int WARP_RANGE = 5;


    public float underwaterDuration = 2.0f;
    public float emergeDuration = 1.0f;
    public float surfaceDuration = 3.0f;
    public float submergeDuration = 1.0f;


    float _startTime = float.NegativeInfinity;
    float _timerDuration;
    Vector3 _origin;
    List<Index2> _warpableTiles = new List<Index2>();


    public bool IsUnderwater { get { return AnimatorInstance.GetCurrentAnimatorStateInfo(0).IsTag("Underwater"); } }
    public bool IsEmerging { get { return AnimatorInstance.GetCurrentAnimatorStateInfo(0).IsTag("Emerge"); } }
    public bool IsSubmerging { get { return AnimatorInstance.GetCurrentAnimatorStateInfo(0).IsTag("Submerge"); } }
    public bool IsSurfaced { get { return AnimatorInstance.GetCurrentAnimatorStateInfo(0).IsTag("Surface"); } }


    Renderer Renderer { get { return AnimatorInstance.GetComponent<Renderer>(); } }


    void Start()
    {
        transform.SetY(WorldOffsetY);
        _origin = transform.position;

        Renderer.enabled = false;
        _healthController.isIndestructible = true;

        AssignWarpableTiles();
    }


    void Update()
    {
        if (!_doUpdate) { return; }
        if (IsPreoccupied) { return; }

        bool timesUp = (Time.time - _startTime >= _timerDuration);
        if (timesUp)
        {
            if (IsUnderwater)
            {
                AnimatorInstance.SetTrigger("Emerge");
                _timerDuration = emergeDuration;

                Renderer.enabled = true;
                WarpToRandomNearbyWaterTile();
            }
            else if (IsEmerging)
            {
                AnimatorInstance.SetTrigger("Surface");
                _timerDuration = surfaceDuration;

                _healthController.isIndestructible = false;
                FacePlayer();
                _enemy.Attack();
            }
            else if (IsSurfaced)
            {
                AnimatorInstance.SetTrigger("Submerge");
                _timerDuration = submergeDuration;

                _healthController.isIndestructible = true;
            }
            else if (IsSubmerging)
            {
                AnimatorInstance.SetTrigger("Underwater");
                _timerDuration = underwaterDuration;

                Renderer.enabled = false;
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
        _healthController.RestoreHealth();
    }

    void AssignWarpableTiles()
    {
        TileMap tileMap = CommonObjects.OverworldTileMap;
        if (tileMap == null) { return; }

        Rect area = new Rect(
            (int)_origin.x - WARP_RANGE - EPSILON,
            (int)_origin.z - WARP_RANGE - EPSILON,
            2 * WARP_RANGE + EPSILON,
            2 * WARP_RANGE + EPSILON);
        _warpableTiles = tileMap.GetTilesInArea(area, TileInfo.WaterTiles);
    }

    void WarpToRandomNearbyWaterTile()
    {
        if (WorldInfo.Instance.IsSpecial) { return; }

        Index2 tile = _warpableTiles[Random.Range(0, _warpableTiles.Count)];
        SetEnemyPositionXZToTile(tile);
    }
}