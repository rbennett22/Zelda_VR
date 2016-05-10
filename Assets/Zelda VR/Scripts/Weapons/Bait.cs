using UnityEngine;
using System.Collections;

public class Bait : MonoBehaviour
{
    public const float MaxLureDistance = 12.0f;
    public const float MaxLureDistanceSq = MaxLureDistance * MaxLureDistance;

    public static Bait ActiveBait { get; private set; }       // Only one Bait is allowed at a time.


    public float maxLifeTime = 1;
    public Animator animator;


	void Awake() 
    {
        animator.GetComponent<Renderer>().enabled = false;
        animator.SetTrigger("SkipSpawn");
	}

    IEnumerator Start()
    {
        ActiveBait = this;

        Destroy(gameObject, maxLifeTime);
        yield return new WaitForSeconds(0.01f);
        animator.GetComponent<Renderer>().enabled = true;

        DungeonRoom dr;
        if (IsHungryGoriyaNpcPresent(out dr))
        {
            dr.FeedGoriyaNpc();
            Destroy(gameObject);
        }
    }

    bool IsHungryGoriyaNpcPresent(out DungeonRoom room)
    {
        room = null;
        if (!WorldInfo.Instance.IsInDungeon) { return false;}
        
        room = DungeonRoom.GetRoomForPosition(transform.position);
        if (room == null || !room.Info.containsGoriyaNPC || room.Info.GoriyaNpcHasBeenFed) 
        {
            return false;
        }

        return true;
    }


    void OnDestroy()
    {
        ActiveBait = null;
    }
}