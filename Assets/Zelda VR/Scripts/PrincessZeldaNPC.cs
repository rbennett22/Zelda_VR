using UnityEngine;

public class PrincessZeldaNPC : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (CommonObjects.IsPlayer(other.gameObject))
        {
            PlayEndOfGameSequence();
        }
    }

    void PlayEndOfGameSequence()
    {
        // TODO
    }
}