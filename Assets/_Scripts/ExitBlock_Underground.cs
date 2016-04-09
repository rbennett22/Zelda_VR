using UnityEngine;


public class ExitBlock_Underground : MonoBehaviour
{

    Grotto _grotto;


    void Awake()
    {
        _grotto = transform.parent.GetComponent<Grotto>();
    }


    void OnTriggerEnter(Collider otherCollider)
    {
        GameObject other = otherCollider.gameObject;
        if (!CommonObjects.IsPlayer(other)) { return; }

        IgnorePlayerGroundCollision();

        if (_grotto != null)
        {
            _grotto.OnPlayerExit();
        }
    }

    void OnTriggerExit(Collider otherCollider)
    {
        if (!CommonObjects.IsPlayer(otherCollider.gameObject)) { return; }

        IgnorePlayerGroundCollision(false);
    }

    void IgnorePlayerGroundCollision(bool ignore = true)
    {
        GameObject g = CommonObjects.Player_G;
        Physics.IgnoreLayerCollision(g.layer, LayerMask.NameToLayer("Ground"), ignore);
    }

}
