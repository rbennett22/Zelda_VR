using UnityEngine;


public class EntranceBlock_Underground : MonoBehaviour
{

    Grotto _grotto;


    void Awake()
    {
        _grotto = transform.parent.GetComponent<Grotto>();
    }


    void OnTriggerEnter(Collider otherCollider)
    {
        if (!CommonObjects.IsPlayer(otherCollider.gameObject)) { return; }

        _grotto.OnPlayerEnter();
    }

}
