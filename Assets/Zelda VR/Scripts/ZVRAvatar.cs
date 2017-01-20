using UnityEngine;

public class ZVRAvatar : MonoBehaviour 
{
    [SerializeField]
    Transform _weaponContainerLeft, _weaponContainerRight;
    [SerializeField]
    Transform _menuContainerLeft, _menuContainerRight;


    public Transform WeaponContainerLeft { get { return _weaponContainerLeft; } }
    public Transform WeaponContainerRight { get { return _weaponContainerRight; } }
    public Transform MenuContainerLeft { get { return _menuContainerLeft; } }
    public Transform MenuContainerRight { get { return _menuContainerRight; } }
}