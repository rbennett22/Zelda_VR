using UnityEngine;


public class ExitBlock_Dungeon : MonoBehaviour
{
    void OnTriggerEnter(Collider otherCollider)
    {
        GameObject other = otherCollider.gameObject;
        //print("ExitBlock_Dungeon --> OnTriggerEnter: " + other.name);
        if (!CommonObjects.IsPlayer(other)) { return; }

        Locations.Instance.WarpToOverworldDungeonEntrance();
    }

}
