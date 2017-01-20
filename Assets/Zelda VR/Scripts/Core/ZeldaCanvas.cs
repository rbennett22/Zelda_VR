using UnityEngine;

public class ZeldaCanvas : MonoBehaviour 
{
    /*[SerializeField]
    Transform _playerDiedViewContainer;
    [SerializeField]
    Transform _triforceAcquiuredViewContainer;
    [SerializeField]
    Transform _shuttersViewContainer;*/
    [SerializeField]
    Transform _inventoryViewContainer;
    [SerializeField]
    Transform _gameplayHUDViewContainer;
    [SerializeField]
    Transform _gameplayHUDPausedTransform;
    [SerializeField]
    Transform _optionsViewContainer;
    [SerializeField]
    Transform _controlsViewContainer;

    //public Transform PlayerDiedViewContainer { get { return _playerDiedViewContainer; } }
    //public Transform TriforceAcquiuredViewContainer { get { return _triforceAcquiuredViewContainer; } }
    //public Transform ShuttersViewContainer { get { return _shuttersViewContainer; } }
    public Transform InventoryViewContainer { get { return _inventoryViewContainer; } }
    public Transform GameplayHUDViewContainer { get { return _gameplayHUDViewContainer; } }
    public Transform GameplayHUDPausedTransform { get { return _gameplayHUDPausedTransform; } }
    public Transform OptionsViewContainer { get { return _optionsViewContainer; } }
    public Transform ControlsViewContainer { get { return _controlsViewContainer; } }
}
