using UnityEngine;
using UnityEngine.SceneManagement;

public class ZVRAvatar : MonoBehaviour 
{
    [SerializeField]
    Transform _weaponContainerLeft, _weaponContainerRight;

    public Transform WeaponContainerLeft { get { return _weaponContainerLeft; } }
    public Transform WeaponContainerRight { get { return _weaponContainerRight; } }
}