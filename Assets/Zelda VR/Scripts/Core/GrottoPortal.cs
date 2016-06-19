using UnityEngine;

public class GrottoPortal : MonoBehaviour 
{
    [SerializeField]
    Grotto _grotto;


    void OnTriggerEnter(Collider otherCollider)
    {
        if (!CommonObjects.IsPlayer(otherCollider.gameObject)) { return; }

        _grotto.OnPlayerEnteredPortal(this);
    }
}