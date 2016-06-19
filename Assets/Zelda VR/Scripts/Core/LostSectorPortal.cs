using UnityEngine;

public class LostSectorPortal : MonoBehaviour
{
    [SerializeField]
    LostSector _lostSector;

    void OnTriggerEnter(Collider otherCollider)
    {
        if (!CommonObjects.IsPlayer(otherCollider.gameObject)) { return; }

        _lostSector.PlayerEnteredPortal(this);
    }
}