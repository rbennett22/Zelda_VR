using UnityEngine;

public class DeathMountainExit : MonoBehaviour
{
    void OnTriggerEnter(Collider otherCollider)
    {
        GameObject other = otherCollider.gameObject;
        if (!CommonObjects.IsPlayer(other)) { return; }

        Music.Instance.PlayOverworld();
    }
}