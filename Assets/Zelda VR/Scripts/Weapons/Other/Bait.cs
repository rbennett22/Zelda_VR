using UnityEngine;

public class Bait : MonoBehaviour
{
    public const float MaxLureDistance = 12.0f;
    public const float MaxLureDistanceSq = MaxLureDistance * MaxLureDistance;

    public static Bait ActiveBait { get; private set; }       // Only one Bait is allowed at a time.


    void Start()
    {
        ActiveBait = this;

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