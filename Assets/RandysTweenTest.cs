using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandysTweenTest : MonoBehaviour
{
    const iTween.EaseType STAB_TWEEN_EASE_TYPE = iTween.EaseType.linear;


    [SerializeField]
    Vector3 _attackDirection_Local = Vector3.up;


    [SerializeField]
    float _reach = 1;       // How far sword can extend when attacking
    [SerializeField]
    float _speed = 12;


    bool _isExtending, _isRetracting;

    Vector3 _originLocal;


    private void Start()
    {
        Extend();
    }


    void Extend()
    {
        if (_isExtending)
        {
            return;
        }

        Vector3 targetPosLocal = _originLocal + _attackDirection_Local * _reach;

        iTween.MoveTo(gameObject, iTween.Hash(
            "islocal", true,
            "position", targetPosLocal,
            "speed", _speed,
            "easetype", STAB_TWEEN_EASE_TYPE,
            "oncomplete", "OnExtendedCompletely")
        );

        //CollisionEnabled = true;

        _isExtending = true;
        _isRetracting = false;
    }
    void OnExtendedCompletely()
    {
        _isExtending = false;

        Retract();
    }

    void Retract()
    {
        if (_isRetracting)
        {
            return;
        }

        iTween.MoveTo(gameObject, iTween.Hash(
            "islocal", true,
            "position", _originLocal,
            "speed", _speed,
            "easetype", STAB_TWEEN_EASE_TYPE,
            "oncomplete", "OnRetractedCompletely")
        );

        _isExtending = false;
        _isRetracting = true;
    }
    void OnRetractedCompletely()
    {
        _isExtending = _isRetracting = false;

        //Rigidbody rb = GetComponent<Rigidbody>();
        //rb.velocity = Vector3.zero;

        transform.localPosition = _originLocal;


        Extend();
    }
}
