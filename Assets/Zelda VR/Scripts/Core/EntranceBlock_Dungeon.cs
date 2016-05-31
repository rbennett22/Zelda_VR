using UnityEngine;

public class EntranceBlock_Dungeon : MonoBehaviour
{
    public int dungeon;

    void OnTriggerEnter(Collider otherCollider)
    {
        GameObject other = otherCollider.gameObject;
        if (!CommonObjects.IsPlayer(other)) { return; }

        Locations.Instance.WarpToDungeonEntranceStairs(dungeon);
    }
}