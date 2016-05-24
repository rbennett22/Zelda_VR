using UnityEngine;

public class RupeeTrigger : MonoBehaviour
{
    public int id;

    void OnTriggerEnter(Collider otherCollider)
    {
        if (!CommonObjects.IsPlayer(otherCollider.gameObject)) { return; }

        // TODO: Don't use SendMessage
        SendMessageUpwards("OnRupeeTrigger", this, SendMessageOptions.RequireReceiver);
    }
}