using UnityEngine;


public class LostWoodsPortal : MonoBehaviour 
{

    public LostWoods lostWoods;


    void OnTriggerEnter(Collider otherCollider)
    {
        GameObject other = otherCollider.gameObject;
        if (!CommonObjects.IsPlayer(other)) { return; }

        lostWoods.OnPlayerEnteredPortal(this);
    }

}