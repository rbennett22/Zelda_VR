using UnityEngine;

public class EntranceBlock_Underground : MonoBehaviour
{
    public Grotto Grotto { get; set; }


    void OnTriggerEnter(Collider otherCollider)
    {
        if (!CommonObjects.IsPlayer(otherCollider.gameObject)) { return; }
        if (Grotto == null) { return; }

        Grotto.OnPlayerEnter();
    }
}