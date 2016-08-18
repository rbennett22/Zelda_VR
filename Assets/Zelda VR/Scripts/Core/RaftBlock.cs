using UnityEngine;

public class RaftBlock : MonoBehaviour
{
    const iTween.EaseType EASE_TYPE = iTween.EaseType.linear;
    const float SPEED = 2.0f;
    const float HOVER_HEIGHT = 0.05f;


    public static bool PlayerIsOnRaft;

    public readonly static Vector3 VerticalRotation = new Vector3(0, 0, 0);
    public readonly static Vector3 HorizontalRotation = new Vector3(0, 90, 0);


    public GameObject raftOverlayPrefab;
    public RaftBlock destination;


    GameObject _raftOverlay;


    bool PlayerHasRaft { get { return Inventory.Instance.HasItem("Raft"); } }


    void Awake()
    {
        GetComponent<Renderer>().enabled = false;
    }


    void OnTriggerEnter(Collider otherCollider)
    {
        if (!CommonObjects.IsPlayer(otherCollider.gameObject)) { return; }
        if (!PlayerHasRaft || PlayerIsOnRaft) { return; }

        TravelToDestination();
    }

    

    void TravelToDestination()
    {
        SetHazardBlocksEnabled(false);

        Vector3 startPos = transform.position;
        Vector3 endPos = destination.transform.position;
        endPos.y = HOVER_HEIGHT;

        Vector3 rot = DetermineRotation(startPos, endPos);
        LayDownRaft(rot);

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

        PlayKeySound();

        PlayerIsOnRaft = true;
    }

    Vector3 DetermineRotation(Vector3 startPos, Vector3 endPos)
    {
        Vector3 v = endPos - startPos;
        bool horz = Mathf.Abs(v.z / v.x) < 1;
        return horz ? HorizontalRotation : VerticalRotation;
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

    void LayDownRaft(Vector3 rotation)
    {
        GameObject g = Instantiate(raftOverlayPrefab) as GameObject;

        Transform t = g.transform;
        t.SetParent(transform);
        t.localPosition = HOVER_HEIGHT * Vector3.up;
        t.Rotate(rotation);

        _raftOverlay = g;
    }


    void PlayKeySound()
    {
        SoundFx.Instance.PlayOneShot(SoundFx.Instance.key);
    }
}