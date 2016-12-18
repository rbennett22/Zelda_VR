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
    EnemyAI_Random _enemyAI_Random;

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

        _enemyAI_Random = GetComponent<EnemyAI_Random>();
        _enemyAI_Random.enabled = false;

        _enemyMove.enabled = false;
    }

    void Start()
    {
        _origin = transform.position;
        _normalMeleeDamage = _enemy.meleeDamage;

        AssignWarpableTiles();

        ProceedToNextState();
    }


    void ProceedToNextStateAfterDelay(float delay)
    {
        Invoke("ProceedToNextState", delay);
    }
    void ProceedToNextState()
    {
        if(_enemy.IsParalyzed)
        {
            ProceedToNextStateAfterDelay(0.01f);
            return;
        }

        _enemy.meleeDamage = IsEmerging ? _normalMeleeDamage : 0;
        _enemyMove.enabled = IsEmerging;
        _enemyAI_Random.enabled = IsEmerging;
        _healthController.isIndestructible = !IsEmerging;

        Renderer.enabled = !IsSubmerging;

        if (IsUnderground)
        {
            PlaceOnscreen();
            WarpToRandomNearbySandTile();

            AnimatorInstance.SetTrigger("Emerge");
            ProceedToNextStateAfterDelay(emergeDuration);
        }
        else if (IsEmerging)
        {
            AnimatorInstance.SetTrigger("Surface");
            ProceedToNextStateAfterDelay(surfaceDuration);
        }
        else if (IsSurfaced)
        {
            AnimatorInstance.SetTrigger("Submerge");
            ProceedToNextStateAfterDelay(submergeDuration);
        }
        else if (IsSubmerging)
        {
            PlaceOffscreen();

            AnimatorInstance.SetTrigger("Underground");
            ProceedToNextStateAfterDelay(undergroundDuration);
        }
    }

    void PlaceOffscreen()
    {
        transform.SetY(_origin.y + OFFSCREEN_OFFSET);  // Move offscreen to prevent collision with player
    }
    void PlaceOnscreen()
    {
        transform.SetY(_origin.y);
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