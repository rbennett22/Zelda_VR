using UnityEngine;

public class ExitBlock_Underground : MonoBehaviour
{
    public Grotto Grotto { get; set; }


    void OnTriggerEnter(Collider otherCollider)
    {
        if (!CommonObjects.IsPlayer(otherCollider.gameObject)) { return; }

        CommonObjects.Player_C.PlayerGroundCollisionEnabled = false;

        if (Grotto != null)
        {
            Grotto.OnPlayerExit();
        }
    }

    void OnTriggerExit(Collider otherCollider)
    {
        if (!CommonObjects.IsPlayer(otherCollider.gameObject)) { return; }

        if (PlayerAtTopOfStairs())
        {
            CommonObjects.Player_C.PlayerGroundCollisionEnabled = true;
        }
    }

    bool PlayerAtTopOfStairs()
    {
        return CommonObjects.Player_C.Position.y > -1f;
    }
}