using UnityEngine;

public class EntranceBlock_Dungeon : MonoBehaviour
{
    public int dungeon;


    void OnTriggerEnter(Collider otherCollider)
    {
        if (!CommonObjects.IsPlayer(otherCollider.gameObject)) { return; }

        CommonObjects.Player_C.PlayerGroundCollisionEnabled = true;

        Locations.Instance.WarpToDungeonEntranceStairs(dungeon);
    }
}