using Immersio.Utility;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EnemyAI_Random))]

public class EnemyAI_Leever : EnemyAI
{
    const float OFFSCREEN_OFFSET = -30;      // How far to offset the Leever's y position when it is underground


    public float undergroundDuration = 2.0f;
    public float emergeDuration = 0.5f;
    public float surfaceDuration = 3.0f;
    public float submergeDuration = 0.5f;


    Vector3 _origin;
    List<Index2> _warpableTiles = new List<Index2>();

    int _normalMeleeDamage;


    public bool IsUnderground { get { return IsAnimationState("Underground"); } }
    public bool IsEmerging { get { return IsAnimationState("Emerge"); } }
    public bool IsSubmerging { get { return IsAnimationState("Submerge"); } }
    public bool IsSurfaced { get { return IsAnimationState("Surface"); } }

    bool IsAnimationState(string tag) { return AnimatorInstance.GetCurrentAnimatorStateInfo(0).IsTag(tag); }


    Renderer Renderer { get { return AnimatorInstance.GetComponent<Renderer>(); } }


    protected override void Awake()
    {
        base.Awake();

        _enemyMove.enabled = false;
    }

    void Start()
    {
        _origin = transform.position;
        _normalMeleeDamage = _enemy.meleeDamage;

        AssignWarpableTiles();

        ProceedToNextState();
    }


    void WaitThenProceedToNextState(float delay)
    {
        Invoke("ProceedToNextState", delay);

        //_timerDuration = duration;
        //_startTime = Time.time;
    }

    void ProceedToNextState()
    {
        _enemy.meleeDamage = IsEmerging ? _normalMeleeDamage : 0;
        _enemyMove.enabled = IsEmerging;
        _healthController.isIndestructible = !IsEmerging;

        Renderer.enabled = !IsSubmerging;

        if (IsUnderground)
        {
            transform.SetY(_origin.y);
            WarpToRandomNearbySandTile();

            AnimatorInstance.SetTrigger("Emerge");
            WaitThenProceedToNextState(emergeDuration);
        }
        else if (IsEmerging)
        {
            MoveDirection_Tile = new IndexDirection2(EnemyAI_Random.GetRandomTileDirection());

            AnimatorInstance.SetTrigger("Surface");
            WaitThenProceedToNextState(surfaceDuration);
        }
        else if (IsSurfaced)
        {
            AnimatorInstance.SetTrigger("Submerge");
            WaitThenProceedToNextState(submergeDuration);
        }
        else if (IsSubmerging)
        {
            transform.SetY(_origin.y - OFFSCREEN_OFFSET);  // Move offscreen to prevent collision with player

            AnimatorInstance.SetTrigger("Underground");
            WaitThenProceedToNextState(undergroundDuration);
        }
    }


    void AssignWarpableTiles()
    {
        TileMap tileMap = CommonObjects.OverworldTileMap;
        if (tileMap == null) { return; }

        _warpableTiles = tileMap.GetTilesInArea(Boundary, TileInfo.SandTiles);
    }

    void WarpToRandomNearbySandTile()
    {
        if (WorldInfo.Instance.IsSpecial) { return; }

        _enemy.Tile = _warpableTiles[Random.Range(0, _warpableTiles.Count)];
    }
}