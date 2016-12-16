using UnityEngine;

public class GrottoPortal : MonoBehaviour 
{
    public enum LocationType
    {
        Left,
        Mid,
        Right
    }

    [SerializeField]
    LocationType _location; 
    public LocationType Location { get { return _location; } }

    [SerializeField]
    Grotto _grotto;


    void OnTriggerEnter(Collider otherCollider)
    {
        if (!CommonObjects.IsPlayer(otherCollider.gameObject)) { return; }

        _grotto.OnPlayerEnteredPortal(this);
    }
}