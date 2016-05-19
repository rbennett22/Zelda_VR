using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Animator))]

public class Bait : MonoBehaviour
{
    public const float MaxLureDistance = 12.0f;
    public const float MaxLureDistanceSq = MaxLureDistance * MaxLureDistance;

    public static Bait ActiveBait { get; private set; }       // Only one Bait is allowed at a time.


    public bool skipSpawnAnimation = true;


    Animator _animator;
    Renderer _animRenderer;
    protected bool RenderingEnabled { get { return _animRenderer.enabled; } set { _animRenderer.enabled = value; } }


    void Awake()
    {
        _animator = GetComponent<Animator>();
        _animRenderer = _animator.GetComponent<Renderer>();

        if (skipSpawnAnimation)
        {
            RenderingEnabled = false;
            _animator.SetTrigger("SkipSpawn");
        }
        else
        {
            RenderingEnabled = true;
        }
    }
    IEnumerator Start()
    {
        ActiveBait = this;

        if (skipSpawnAnimation)
        {
            yield return new WaitForSeconds(0.01f);

            RenderingEnabled = true;
        }

        DungeonRoom dr;
        if (IsHungryGoriyaNpcPresent(out dr))
        {
            FeedGoriya(dr);
        }
    }

    bool IsHungryGoriyaNpcPresent(out DungeonRoom room)
    {
        room = null;
        if (!WorldInfo.Instance.IsInDungeon) { return false; }

        room = DungeonRoom.GetRoomForPosition(transform.position);
        if (room == null || !room.Info.containsGoriyaNPC || room.Info.GoriyaNpcHasBeenFed)
        {
            return false;
        }

        return true;
    }

    void FeedGoriya(DungeonRoom dr)
    {
        if(dr == null)
            return;

        dr.FeedGoriyaNpc();
        Destroy(gameObject);
    }


    void OnDestroy()
    {
        ActiveBait = null;
    }
}