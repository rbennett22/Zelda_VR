using UnityEngine;

public class DungeonDoorTrigger : MonoBehaviour
{

    public DungeonRoom dungeonRoom;
    public DungeonRoomInfo.WallDirection wallDirection;


    void OnTriggerEnter(Collider otherCollider)
    {
        GameObject other = otherCollider.gameObject;
        //print("DungeonDoorTrigger --> OnTriggerEnter: " + other.name);
        if (!CommonObjects.IsPlayer(other)) { return; }

        if (dungeonRoom.IsDoorLocked(wallDirection))
        {
            if (CommonObjects.Player_C.Inventory.GetItem("MagicKey").count > 0)
            {
                dungeonRoom.UnlockDoor(wallDirection);
            }
            else if (CommonObjects.Player_C.Inventory.GetItem("Key").count > 0)
            {
                CommonObjects.Player_C.Inventory.UseItem("Key");
                dungeonRoom.UnlockDoor(wallDirection);
            }
        }

        if (dungeonRoom.CanPassThrough(wallDirection))
        {
            IgnorePlayerWallCollision();
        }
    }

    void OnTriggerExit(Collider otherCollider)
    {
        GameObject other = otherCollider.gameObject;
        //print("DungeonDoorTrigger --> OnTriggerExit: " + other.name);
        if (!CommonObjects.IsPlayer(other)) { return; }

        IgnorePlayerWallCollision(false);
    }

    void IgnorePlayerWallCollision(bool ignore = true)
    {
        if (Cheats.Instance.GhostModeIsEnabled) { return; }

        GameObject g = CommonObjects.Player_G;
        Physics.IgnoreLayerCollision(g.layer, LayerMask.NameToLayer("Walls"), ignore);
    }

}
