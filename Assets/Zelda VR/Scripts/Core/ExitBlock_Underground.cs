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
        if (!CommonObjects.IsPlayer(other))
        {
            return;
        }

        SetPlayerGroundCollisionEnabled(false);

        if (_grotto != null)
        {
            _grotto.OnPlayerExit();
        }
    }

    void OnTriggerExit(Collider otherCollider)
    {
        GameObject other = otherCollider.gameObject;
        if (!CommonObjects.IsPlayer(other))
        {
            return;
        }

        SetPlayerGroundCollisionEnabled(true);
    }

    void SetPlayerGroundCollisionEnabled(bool enabled)
    {
        if (Cheats.Instance.GhostModeIsEnabled)
        {
            return;
        }

        int playerLayer = CommonObjects.Player_G.layer;
        int groundLayer = LayerMask.NameToLayer("Ground");
        int blocksLayer = LayerMask.NameToLayer("Blocks");

        Physics.IgnoreLayerCollision(playerLayer, groundLayer, !enabled);
        Physics.IgnoreLayerCollision(playerLayer, blocksLayer, !enabled);
    }
}