using UnityEngine;

public class ZeldaNPC : MonoBehaviour 
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