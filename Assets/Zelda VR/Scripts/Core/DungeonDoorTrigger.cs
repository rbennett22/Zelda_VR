using UnityEngine;

public class DungeonDoorTrigger : MonoBehaviour
{
    public DungeonRoom dungeonRoom;
    public DungeonRoomInfo.WallDirection wallDirection;


    void OnTriggerEnter(Collider otherCollider)
    {
        if (!CommonObjects.IsPlayer(otherCollider.gameObject)) { return; }

        if (dungeonRoom.IsDoorLocked(wallDirection))
        {
            Inventory inv = CommonObjects.Player_C.Inventory;

            if (inv.HasItem("MagicKey"))
            {
                UnlockDoor();
            }
            else if (inv.HasItem("Key"))
            {
                inv.UseItem("Key");
                UnlockDoor();
            }
        }

        if (dungeonRoom.CanPassThrough(wallDirection) || PlayerIsOnOuterSideOfDoor())
        {
            SetPlayerWallCollisionEnabled(false);
        }
    }

    void UnlockDoor()
    {
        dungeonRoom.UnlockDoor(wallDirection);
    }

    void OnTriggerExit(Collider otherCollider)
    {
        if (!CommonObjects.IsPlayer(otherCollider.gameObject))
        {
            return;
        }

        if (!PlayerIsOnOuterSideOfDoor())
        {
            SetPlayerWallCollisionEnabled(true);
        }
    }

    bool PlayerIsOnOuterSideOfDoor()
    {
        Player player = CommonObjects.Player_C;
        Vector2 p = new Vector2(player.Position.x, player.Position.z);
        return !dungeonRoom.Bounds.Contains(p);
    }

    void SetPlayerWallCollisionEnabled(bool enabled)
    {
        if (Cheats.Instance.GhostModeIsEnabled)
        {
            return;
        }

        int playerLayer = CommonObjects.Player_G.layer;
        int wallLayer = LayerMask.NameToLayer("Walls");

        Physics.IgnoreLayerCollision(playerLayer, wallLayer, !enabled);
    }
}