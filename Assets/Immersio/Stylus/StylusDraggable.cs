using UnityEngine;


public class StylusDraggable : MonoBehaviour
{
    public bool verbose = false;


    bool _isBeingDragged = false;
    public bool IsBeingDragged
    {
        get { return _isBeingDragged; }
        set
        {
            if (value != _isBeingDragged)
            {
                _isBeingDragged = value;
                SendNotificationOfStateChange(_isBeingDragged);
            }
        }
    }

    void SendNotificationOfStateChange(bool newState)
    {
        string methodName = newState ? "OnDragBegan" : "OnDragEnded";
        LogStatus(methodName);
        SendMessage(methodName, SendMessageOptions.DontRequireReceiver);
    }


    #region Stylus Event handlers

    Stylus SharedStylus { get { return Stylus.Instance; } }
    float _initialHitDistance;
    Vector3 _selectionOffset;

    void OnStylusDown()
    {
        RaycastHit hit = SharedStylus.StylusRayHit;
        _initialHitDistance = hit.distance;
        _selectionOffset = transform.position - hit.point;

        IsBeingDragged = true;
    }
	
	void OnStylusDrag()
    {
        transform.position = StylusRayEndPosition() + _selectionOffset;
        SendMessage("OnDrag", SendMessageOptions.DontRequireReceiver);
    }

    void OnStylusUpAsButton()
    {
        IsBeingDragged = false;
    }

    #endregion


    Vector3 StylusRayEndPosition()
    {
        Ray ray = SharedStylus.StylusRay;
        return ray.origin + (ray.direction * _initialHitDistance);
    }

    void LogStatus(string msg)
    {
        if (verbose) { print(msg); }
    }

}
