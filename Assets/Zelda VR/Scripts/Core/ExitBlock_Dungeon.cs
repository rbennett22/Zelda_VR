using UnityEngine;

public class ExitBlock_Dungeon : MonoBehaviour
{
    void OnTriggerEnter(Collider otherCollider)
    {
        if (!CommonObjects.IsPlayer(otherCollider.gameObject)) { return; }

        Locations.Instance.WarpToOverworldDungeonEntrance();
    }
}