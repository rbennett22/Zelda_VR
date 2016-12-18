using Immersio.Utility;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI_RiverZora : EnemyAI
{
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
            ProceedToNextState();

            _startTime = Time.time;
        }

        if (IsSurfaced)
        {
            FacePlayer();
        }
    }

    void ProceedToNextState()
    {
        _healthController.isIndestructible = !IsEmerging;
        Renderer.enabled = !IsSubmerging;

        if (IsUnderwater)
        {
            AnimatorInstance.SetTrigger("Emerge");
            _timerDuration = emergeDuration;

            WarpToRandomNearbyWaterTile();
        }
        else if (IsEmerging)
        {
            AnimatorInstance.SetTrigger("Surface");
            _timerDuration = surfaceDuration;

            FacePlayer();
            _enemy.Attack();
        }
        else if (IsSurfaced)
        {
            AnimatorInstance.SetTrigger("Submerge");
            _timerDuration = submergeDuration;
        }
        else if (IsSubmerging)
        {
            AnimatorInstance.SetTrigger("Underwater");
            _timerDuration = underwaterDuration;

            ReplenishHealth();
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

        _warpableTiles = tileMap.GetTilesInArea(Boundary, TileInfo.WaterTiles);
    }

    void WarpToRandomNearbyWaterTile()
    {
        if (WorldInfo.Instance.IsSpecial) { return; }

        _enemy.Tile = _warpableTiles[Random.Range(0, _warpableTiles.Count)];
    }
}