using UnityEngine;

public class Shield_Base : MonoBehaviour 
{
    [SerializeField]
    float _blockDotThreshold = 0.6f;   // [0-1].  Closer to 1 means player has to be facing an incoming attack more directly in order to block it.


    public bool CanBlockAttack(Vector3 directionOfAttack)
    {
        return Vector3.Dot(transform.forward, -directionOfAttack) > _blockDotThreshold;
    }
}