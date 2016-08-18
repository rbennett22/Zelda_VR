using UnityEngine;
using Immersio.Utility;

public class HazardBlock : MonoBehaviour
{
    static HazardBlock LadderBlock;     // The block, if any, that currently has the Ladder over it (only one allowed at a time)

    public readonly static Vector3 VerticalRotation = new Vector3(0, 0, 0);
    public readonly static Vector3 HorizontalRotation = new Vector3(0, 90, 0);


    public GameObject ladderOverlayPrefab;
    GameObject _ladderOverlay;

    Collider _physicsCollider;
    public bool PhysicsColliderEnabled { get { return _physicsCollider.enabled; } set { _physicsCollider.enabled = value; } }


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
        if (!CommonObjects.IsPlayer(otherCollider.gameObject)) { return; }
        if (!PlayerHasLadder || LadderBlock != null) { return; }

        Player player = CommonObjects.Player_C;
        if (IsPlayerMovingTowardsThisBlock(player) && CanCross(player))
        {
            Vector3 rot = DetermineLadderRotation(player);
            LayDownLadder(rot);
        }
    }
    Vector3 DetermineLadderRotation(Player player)
    {
        Vector3 v = player.Position - transform.position;
        bool horz = Mathf.Abs(v.z / v.x) < 1;
        return horz ? HorizontalRotation : VerticalRotation;
    }

    void OnTriggerExit(Collider otherCollider)
    {
        if (!CommonObjects.IsPlayer(otherCollider.gameObject)) { return; }
        if (!PlayerHasLadder) { return; }

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

        return hits.Length == 0;
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

    bool PlayerHasLadder { get { return Inventory.Instance.HasItem("Ladder"); } }

    void LayDownLadder(Vector3 rotation)
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
        t.localPosition = 0.05f * Vector3.up;
        t.Rotate(rotation);
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