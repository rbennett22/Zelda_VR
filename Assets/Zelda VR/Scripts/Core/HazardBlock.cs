using UnityEngine;
using Immersio.Utility;

public class HazardBlock : MonoBehaviour
{
    static HazardBlock LadderBlock;     // The block, if any, that currently has the Ladder over it (only one allowed at a time)


    public GameObject ladderOverlayPrefab;


    Collider _physicsCollider;
    public bool PhysicsColliderEnabled { get { return _physicsCollider.enabled; } set { _physicsCollider.enabled = value; } }

    GameObject _ladderOverlay;


    void Awake()
    {
        _physicsCollider = GetComponent<BoxCollider>();
    }


    void OnTriggerEnter(Collider otherCollider)
    {
        OnTriggerEnterOrStay(otherCollider);
    }
    void OnTriggerStay(Collider otherCollider)
    {
        OnTriggerEnterOrStay(otherCollider);
    }
    void OnTriggerEnterOrStay(Collider otherCollider)
    {
        GameObject other = otherCollider.gameObject;
        if (!CommonObjects.IsPlayer(other) || !PlayerHasLadder())
        {
            return;
        }
        if (LadderBlock != null)
        {
            return;
        }

        Player player = CommonObjects.Player_C;

        if (IsPlayerMovingTowardsThisBlock(player))
        {
            if (CanCross(player))
            {
                Vector3 toPlayer = player.Position - transform.position;
                bool layHorizontally = Mathf.Abs(toPlayer.z / toPlayer.x) < 1;
                LayDownLadder(layHorizontally);
            }
        }
    }
    void OnTriggerExit(Collider otherCollider)
    {
        GameObject other = otherCollider.gameObject;
        //print("HazardBlock --> OnTriggerExit: " + other.name);

        if (!CommonObjects.IsPlayer(other) || !PlayerHasLadder())
        {
            return;
        }

        if (LadderBlock == this)
        {
            RemoveLadder();
        }
    }

    bool CanCross(Player player)
    {
        const float MAX_RAY_DIST = 1.0f;

        if (RaftBlock.PlayerIsOnRaft) { return false; }

        Vector3 pos = transform.position;
        Vector3 toPlayer = player.Position - pos;
        toPlayer.y = 0;

        // Cast to opposite tile.  If there are no Blocks or InvisibleBlocks
        //  (ie. other HazardBlocks), then player can cross
        LayerMask mask = Extensions.GetLayerMaskIncludingLayers("Blocks", "InvisibleBlocks");
        RaycastHit[] hits = Physics.RaycastAll(pos, toPlayer, MAX_RAY_DIST, mask);
        if (hits.Length > 0)
        {
            return false;
        }

        Vector3 toOppositeTile = -1 * toPlayer;
        hits = Physics.RaycastAll(pos, toOppositeTile, MAX_RAY_DIST, mask);
        if (hits.Length > 0)
        {
            return false;
        }

        return true;
    }
    bool IsPlayerMovingTowardsThisBlock(Player player)
    {
        Vector3 toPlayer = (player.Position - transform.position).normalized;
        toPlayer.y = 0;
        toPlayer.Normalize();

        Vector3 playerMoveDir = player.LastAttemptedMoveDirection;
        playerMoveDir.y = 0;
        playerMoveDir.Normalize();

        return Vector3.Dot(playerMoveDir, toPlayer) < 0.9f;
    }

    bool PlayerHasLadder()
    {
        return Inventory.Instance.HasItem("Ladder");
    }

    void LayDownLadder(bool horizontal)
    {
        if(LadderBlock != null)
        {
            LadderBlock.RemoveLadder();
        }

        PhysicsColliderEnabled = false;
        LadderBlock = this;

        _ladderOverlay = Instantiate(ladderOverlayPrefab) as GameObject;

        Transform t = _ladderOverlay.transform;
        t.SetParent(transform);
        t.localPosition = Vector3.zero;
        t.AddToY(0.05f);

        float rotY = horizontal ? 90 : 0;
        _ladderOverlay.transform.Rotate(new Vector3(0, rotY, 0));
    }


    public void RemoveLadder()
    {
        PhysicsColliderEnabled = true;
        if (LadderBlock == this)
        {
            LadderBlock = null;
        }

        Destroy(_ladderOverlay);
        _ladderOverlay = null;
    }
}