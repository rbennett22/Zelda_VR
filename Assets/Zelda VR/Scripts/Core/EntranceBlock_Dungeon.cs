using UnityEngine;

public class EntranceBlock_Dungeon : MonoBehaviour
{
    public int dungeon;

    void OnTriggerEnter(Collider otherCollider)
    {
        if (!CommonObjects.IsPlayer(otherCollider.gameObject)) { return; }

        SetPlayerGroundCollisionEnabled(true);

        Locations.Instance.WarpToDungeonEntranceStairs(dungeon);
    }

    void SetPlayerGroundCollisionEnabled(bool enabled)
    {
        if (Cheats.Instance.GhostModeIsEnabled)
        {
            return;
        }

        int playerLayer = CommonObjects.Player_G.layer;
        int groundLayer = LayerMask.NameToLayer("Ground");
        int blocksLayer = LayerMask.NameToLayer("Blocks");

        Physics.IgnoreLayerCollision(playerLayer, groundLayer, !enabled);
        Physics.IgnoreLayerCollision(playerLayer, blocksLayer, !enabled);
    }
}