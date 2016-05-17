using Immersio.Utility;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EnemyAI_Random))]

public class EnemyAI_Leever : EnemyAI
{
    const int WARP_RANGE = 5;
    const float OFFSCREEN_OFFSET = -30;      // How far to offset the Leever's y position when it is underground


    public float undergroundDuration = 2.0f;
    public float emergeDuration = 1.0f;
    public float surfaceDuration = 3.0f;
    public float submergeDuration = 1.0f;


    float _startTime = float.NegativeInfinity;
    float _timerDuration;
    Vector3 _origin;
    List<Index2> _warpableTiles = new List<Index2>();


    public bool IsUnderground { get { return AnimatorInstance.GetCurrentAnimatorStateInfo(0).IsTag("Underground"); } }
    public bool IsEmerging { get { return AnimatorInstance.GetCurrentAnimatorStateInfo(0).IsTag("Emerge"); } }
    public bool IsSubmerging { get { return AnimatorInstance.GetCurrentAnimatorStateInfo(0).IsTag("Submerge"); } }
    public bool IsSurfaced { get { return AnimatorInstance.GetCurrentAnimatorStateInfo(0).IsTag("Surface"); } }


    protected override void Awake()
    {
        base.Awake();

        _enemyMove.enabled = false;
    }

    void Start()
    {
        _origin = transform.position;
        AnimatorInstance.GetComponent<Renderer>().enabled = false;
        _healthController.isIndestructible = true;

        AssignWarpableTiles();
    }


    void Update()
    {
        transform.SetY(WorldOffsetY);     // hack?

        if (!_doUpdate) { return; }
        if (IsPreoccupied) { return; }

        bool timesUp = (Time.time - _startTime >= _timerDuration);
        if (timesUp)
        {
            if (IsUnderground)
            {
                AnimatorInstance.SetTrigger("Emerge");
                _timerDuration = emergeDuration;

                AnimatorInstance.GetComponent<Renderer>().enabled = true;
                transform.SetY(_origin.y);
                WarpToRandomNearbySandTile();
            }
            else if (IsEmerging)
            {
                AnimatorInstance.SetTrigger("Surface");
                _timerDuration = surfaceDuration;

                _healthController.isIndestructible = false;
                _enemyMove.enabled = true;
                MoveDirection = new TileDirection(EnemyAI_Random.GetRandomTileDirection());
            }
            else if (IsSurfaced)
            {
                AnimatorInstance.SetTrigger("Submerge");
                _timerDuration = submergeDuration;

                _healthController.isIndestructible = true;
                _enemyMove.enabled = false;
            }
            else if (IsSubmerging)
            {
                AnimatorInstance.SetTrigger("Underground");
                _timerDuration = undergroundDuration;

                AnimatorInstance.GetComponent<Renderer>().enabled = false;
                transform.SetY(_origin.y - OFFSCREEN_OFFSET);  // Move offscreen to prevent collision with player
            }

            _startTime = Time.time;
        }
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
        _warpableTiles = tileMap.GetTilesInArea(area, TileInfo.SandTiles);
    }

    void WarpToRandomNearbySandTile()
    {
        if (WorldInfo.Instance.IsSpecial) { return; }

        Index2 tile = _warpableTiles[Random.Range(0, _warpableTiles.Count)];
        SetEnemyPositionXZToTile(tile);
    }
}