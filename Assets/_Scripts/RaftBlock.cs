using UnityEngine;


public class RaftBlock : MonoBehaviour
{
    const float RaftSpeed = 2.0f;
    const float HoverHeight = 0.05f;

    public static bool PlayerIsOnRaft;


    public GameObject raftOverlayPrefab;
    public RaftBlock destination;


    Collider _physicsCollider;
    GameObject _raftOverlay;
    Transform _playerTransform;


    void Awake()
    {
        _playerTransform = CommonObjects.PlayerController_G.transform;
        _physicsCollider = GetComponent<BoxCollider>();
        GetComponent<Renderer>().enabled = false;
    }


    void OnTriggerEnter(Collider otherCollider)
    {
        if (PlayerIsOnRaft) { return; }

        GameObject other = otherCollider.gameObject;
        if (!CommonObjects.IsPlayer(other)) { return; }
        //print("RaftBlock --> OnTriggerEnter: " + other.name);

        if (Inventory.Instance.GetItem("Raft").count > 0)
        {
            TravelToDestination();
        }
    }

    void TravelToDestination()
    {
        EnableHazardBlocks(false);

        Vector3 startPos = transform.position;
        Vector3 endPos = destination.transform.position;
        endPos.y = HoverHeight;

        Vector3 vec = startPos - endPos;
        bool layVertically = Mathf.Abs(vec.x / vec.z) < 1;
        LayDownRaft(layVertically);

        iTween.MoveTo(_raftOverlay, iTween.Hash(
            "position", endPos, "speed", RaftSpeed, "easetype", iTween.EaseType.linear,
            "oncomplete", "OnReachedDestination", "oncompletetarget", gameObject)
            );

        _playerTransform.SetX(startPos.x);
        _playerTransform.SetZ(startPos.z);

        endPos.y = _playerTransform.position.y;
        iTween.MoveTo(_playerTransform.gameObject, iTween.Hash(
            "position", endPos, "speed", RaftSpeed, "easetype", iTween.EaseType.linear)
            );

        PlayerIsOnRaft = true;
    }

    void OnReachedDestination()
    {
        Destroy(_raftOverlay);
        EnableHazardBlocks();

        PlayerIsOnRaft = false;
    }

    void EnableHazardBlocks(bool enable = true)
    {
        Vector3 startPos = transform.position;
        Vector3 endPos = destination.transform.position;
        float distance = Vector3.Distance(startPos, endPos);

        RaycastHit[] hits = Physics.RaycastAll(startPos, endPos - startPos, distance);
        foreach (var hit in hits)
        {
            HazardBlock hb = hit.collider.GetComponent<HazardBlock>();
            if (hb == null) { continue; }

            if (enable) 
                { hb.EnablePhysicsCollider(); }
            else 
                { hb.DisablePhysicsCollider(); }
        }
    }

    void LayDownRaft(bool vertical)
    {
        _raftOverlay = Instantiate(raftOverlayPrefab) as GameObject;
        _raftOverlay.transform.parent = transform;
        _raftOverlay.transform.localPosition = Vector3.zero;
        _raftOverlay.transform.SetY(HoverHeight);

        float rotY = vertical ? 0 : 90;
        _raftOverlay.transform.Rotate(new Vector3(0, rotY, 0));
    }

}