using UnityEngine;

public class VRDynamicSword : MonoBehaviour 
{
    const float CONTROLLER_VELOCITY_FACTOR = 2.0f;


    [SerializeField]
    Vector3 _scaleMultiplier = Vector3.one;
    [SerializeField]
    float _smoothing = 1.0f;


    Vector3 _scaleMin, _scaleMax;
    Vector3 _posMin, _posMax;
    Vector3 _controllerPos, _prevControllerPos;
    Vector3 _controllerVelocity;
    Vector3 _targetScale;


    void Start()
    {
        _scaleMin = transform.localScale;
        _scaleMax = _scaleMin;
        _scaleMax.Scale(_scaleMultiplier);

        _posMin = transform.localPosition;
        _posMax = _posMin;
        _posMax.Scale(_scaleMultiplier);
    }


	void Update () 
	{
        if (!ZeldaInput.AreAnyTouchControllersActive())
        {
            return;
        }
        if (!OVRInput.GetControllerPositionTracked(OVRInput.Controller.RTouch))
        {
            return;
        }

        _controllerPos = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch);

        if (_prevControllerPos != Vector3.zero)
        {
            _controllerVelocity = (_controllerPos - _prevControllerPos) / Time.deltaTime;
        }

        UpdateScale(_controllerVelocity);
    }

    void UpdateScale(Vector3 velocity)
    {
        float m = velocity.magnitude * CONTROLLER_VELOCITY_FACTOR;

        //print(" ~~~ m = " + m);

        _targetScale = Vector3.Lerp(_scaleMin, _scaleMax, m);
        Vector3 currentScale = transform.localScale;
        Vector3 scale = Vector3.Lerp(currentScale, _targetScale, 1 - _smoothing);

        transform.localScale = scale;
    }

    void LateUpdate()
    {
        _prevControllerPos = _controllerPos;
    }   
}
