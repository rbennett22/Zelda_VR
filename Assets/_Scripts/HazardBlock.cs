using UnityEngine;
using System.Collections;

public class HazardBlock : MonoBehaviour
{
    static HazardBlock LadderBlock;     // The block, if any, that currently has the Ladder over it (only one allowed at a time)


    public GameObject ladderOverlayPrefab;


    Collider _physicsCollider;
    GameObject _ladderOverlay;


    void Awake()
    {
        _physicsCollider = GetComponent<BoxCollider>();
    }


    void OnTriggerEnter(Collider otherCollider)
    {
        GameObject other = otherCollider.gameObject;
        //print("HazardBlock --> OnTriggerEnter: " + other.name);

        if (CommonObjects.IsPlayer(other))
        {
            if (Inventory.Instance.GetItem("Ladder").count > 0)
            {
                if (LadderBlock == null)
                {
                    if (CanCross(other))
                    {
                        Vector3 vec = transform.position - other.transform.position;
                        bool layVertically = Mathf.Abs(vec.x / vec.z) < 1;
                        LayDownLadder(layVertically);
                    }
                }
            }
        }
    }

    void OnTriggerExit(Collider otherCollider)
    {
        GameObject other = otherCollider.gameObject;
        //print("HazardBlock --> OnTriggerExit: " + other.name);

        if (CommonObjects.IsPlayer(other))
        {
            if (Inventory.Instance.GetItem("Ladder").count > 0)
            {
                RemoveLadder();
            }
        }
    }


    bool CanCross(GameObject player)
    {
        if (RaftBlock.PlayerIsOnRaft) { return false; }

        bool canCross = true;

        Vector3 blockPos = transform.position;
        //blockPos.y = 0.2f;
        Vector3 playerPos = player.transform.position;
        Vector3 direction = blockPos - playerPos;
        direction.y = 0;

        RaycastHit[] hits = Physics.RaycastAll(blockPos, direction, 1.0f);
        foreach (var hit in hits)
        {
            string layerName = LayerMask.LayerToName(hit.collider.gameObject.layer);
            if (layerName == "Blocks" || layerName == "InvisibleBlocks")
            {
                canCross = false;
                break;
            }
        }

        return canCross;
    }

    void LayDownLadder(bool vertical)
    {
        DisablePhysicsCollider();
        LadderBlock = this;

        _ladderOverlay = Instantiate(ladderOverlayPrefab) as GameObject;
        _ladderOverlay.transform.parent = transform;
        _ladderOverlay.transform.localPosition = Vector3.zero;
        _ladderOverlay.transform.AddToY(0.05f);

        float rotY = vertical ? 0 : 90;
        _ladderOverlay.transform.Rotate(new Vector3(0, rotY, 0));
    }

    void RemoveLadder()
    {
        EnablePhysicsCollider();
        if (LadderBlock == this) { LadderBlock = null; }

        Destroy(_ladderOverlay);
    }


    public void EnablePhysicsCollider()
    {
        _physicsCollider.enabled = true;
    }
    public void DisablePhysicsCollider()
    {
        _physicsCollider.enabled = false;
    }

}