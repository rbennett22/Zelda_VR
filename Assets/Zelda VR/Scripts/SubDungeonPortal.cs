using UnityEngine;


public class SubDungeonPortal : MonoBehaviour 
{
    public SubDungeon subDungeon;

    void OnTriggerEnter(Collider otherCollider)
    {
        if (!CommonObjects.IsPlayer(otherCollider.gameObject)) { return; }

        subDungeon.OnPlayerEnteredPortal(this);
    }
}