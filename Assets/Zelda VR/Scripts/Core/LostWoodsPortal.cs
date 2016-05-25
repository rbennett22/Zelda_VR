using UnityEngine;

public class LostWoodsPortal : MonoBehaviour
{
    [SerializeField]
    LostWoods _lostWoods;

    void OnTriggerEnter(Collider otherCollider)
    {
        GameObject other = otherCollider.gameObject;
        if (!CommonObjects.IsPlayer(other)) { return; }

        _lostWoods.PlayerEnteredPortal(this);
    }
}