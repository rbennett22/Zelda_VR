using UnityEngine;

public class RaftBlock : MonoBehaviour
{
    const iTween.EaseType EASE_TYPE = iTween.EaseType.linear;
    const float SPEED = 2.0f;
    const float HOVER_HEIGHT = 0.05f;


    public static bool PlayerIsOnRaft;


    public GameObject raftOverlayPrefab;
    public RaftBlock destination;


    GameObject _raftOverlay;


    void Awake()
    {
        GetComponent<Renderer>().enabled = false;
    }


    void OnTriggerEnter(Collider otherCollider)
    {
        if (!CommonObjects.IsPlayer(otherCollider.gameObject)) { return; }
        if (PlayerIsOnRaft) { return; }

        if (Inventory.Instance.HasItem("Raft"))
        {
            TravelToDestination();
        }
    }

    void TravelToDestination()
    {
        SetHazardBlocksEnabled(false);

        Vector3 startPos = transform.position;
        Vector3 endPos = destination.transform.position;
        endPos.y = HOVER_HEIGHT;

        Vector3 toDest = endPos - startPos;
        bool layHorizontally = Mathf.Abs(toDest.z / toDest.x) < 1;
        LayDownRaft(layHorizontally);

        iTween.MoveTo(_raftOverlay, iTween.Hash(
            "position", endPos, 
            "speed", SPEED, 
            "easetype", EASE_TYPE,

            "oncomplete", "OnReachedDestination", 
            "oncompletetarget", gameObject)
            );

        Player player = CommonObjects.Player_C;
        player.PositionXZ = new Vector2(startPos.x, startPos.z);
        endPos.y = player.Position.y;

        iTween.MoveTo(player.PlayerController.gameObject, iTween.Hash(
            "position", endPos, 
            "speed", SPEED, 
            "easetype", EASE_TYPE)
            );

        PlayerIsOnRaft = true;
    }

    void SetHazardBlocksEnabled(bool enable)
    {
        Vector3 startPos = transform.position;
        Vector3 endPos = destination.transform.position;
        float distance = Vector3.Distance(startPos, endPos);

        RaycastHit[] hits = Physics.RaycastAll(startPos, endPos - startPos, distance);
        foreach (var hit in hits)
        {
            HazardBlock hb = hit.collider.GetComponent<HazardBlock>();
            if (hb != null)
            {
                hb.PhysicsColliderEnabled = enable;
            }
        }
    }

    void OnReachedDestination()
    {
        Destroy(_raftOverlay);
        SetHazardBlocksEnabled(true);

        PlayerIsOnRaft = false;
    }

    void LayDownRaft(bool horizontal)
    {
        GameObject g = Instantiate(raftOverlayPrefab) as GameObject;

        Transform t = g.transform;
        t.SetParent(transform);
        t.localPosition = Vector3.zero;
        t.SetY(HOVER_HEIGHT);

        float rotY = horizontal ? 90 : 0;
        t.Rotate(new Vector3(0, rotY, 0));

        _raftOverlay = g;
    }
}