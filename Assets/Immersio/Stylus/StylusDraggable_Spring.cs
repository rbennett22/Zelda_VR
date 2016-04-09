using UnityEngine;
using System.Collections;


public class StylusDraggable_Spring : MonoBehaviour
{
    const float Distance = 0.0f;


    public float spring = 1500.0f;
    public float damper = 50.0f;
    public float drag = 10.0f;
    public float angularDrag = 5.0f;
    public float maxDistToSpringJoint = 1.0f;
    public bool attachToCenterOfMass = false;

    public bool applyRotation = true;
    public bool tweenToEndTarget = false;

    public bool snapToGrid = false;
    public Vector3 gridIncrements = new Vector3(1, 1, 1);
    public Vector3 gridOffset = Vector3.zero;

    public bool verbose = false;


    bool _draggingEnabled = true;
    public bool DraggingEnabled
    {
        get { return _draggingEnabled; }
        set
        {
            if (_draggingEnabled == value)
                return;

            _draggingEnabled = value;
            if (_isBeingDragged && !_draggingEnabled)
            {
                EndDrag(false);
            }
        }
    }

    bool _isBeingDragged = false;
    public bool IsBeingDragged
    {
        get { return _isBeingDragged; }
        set
        {
            if (value == _isBeingDragged)
                return;

            _isBeingDragged = value;
            SendNotificationOfStateChange(_isBeingDragged);
        }
    }


    Stylus SharedStylus
    {
        get { return Stylus.Instance; }
    }

    static SpringJoint springJoint;


    void SendNotificationOfStateChange(bool newState)
    {
        string methodName = newState ? "OnDragBegan" : "OnDragEnded";
        LogStatus(methodName);
        SendMessage(methodName, this, SendMessageOptions.DontRequireReceiver);
    }


    #region Drag Logic

    void OnStylusDown()
    {
        if (!_draggingEnabled) { return; }

        RaycastHit hit = SharedStylus.StylusRayHit;

        if (!springJoint)
        {
            GameObject go = new GameObject("Rigidbody Dragger");
            go.transform.parent = gameObject.transform.parent;
            Rigidbody body = go.AddComponent<Rigidbody>() as Rigidbody;
            springJoint = go.AddComponent<SpringJoint>();
            body.isKinematic = true;
        }

        springJoint.transform.position = hit.point;
        if (attachToCenterOfMass)
        {
            Vector3 anchor = transform.TransformDirection(hit.rigidbody.centerOfMass) + hit.rigidbody.transform.position;
            anchor = springJoint.transform.InverseTransformPoint(anchor);
            springJoint.anchor = anchor;
        }
        else
        {
            springJoint.anchor = Vector3.zero;
        }

        springJoint.spring = spring;
        springJoint.damper = damper;
        springJoint.maxDistance = Distance;
        springJoint.connectedBody = hit.rigidbody;

        StartCoroutine(DragObjectCoroutineName, hit.distance);
    }

    const string DragObjectCoroutineName = "DragObject_Coroutine";
    IEnumerator DragObject_Coroutine(float distance)
    {
        BeginDrag();

        Ray ray;
        Vector3 targetPos;
        Vector3 currPos;

        while (SharedStylus.GetStylusButton())
        {
            ray = SharedStylus.StylusRay;
            currPos = transform.position;
            targetPos = ray.GetPoint(distance);

            // Constrain distance
            Vector3 vec = targetPos - currPos;
            if (vec.sqrMagnitude >= maxDistToSpringJoint * maxDistToSpringJoint)
            {
                vec.Normalize();
                vec *= maxDistToSpringJoint;
                targetPos = currPos + vec;
            }

            if (snapToGrid) { targetPos = targetPos.SnappedToGrid(gridIncrements, gridOffset); }

            springJoint.transform.position = targetPos;


            if (applyRotation)
            {
                Quaternion currRot = transform.rotation;
                Ray prevRay = SharedStylus.PrevStylusRay;
                Quaternion deltaRot = Quaternion.FromToRotation(prevRay.direction, ray.direction);
                transform.rotation = deltaRot * currRot;
            }

            SendMessage("OnDrag", this, SendMessageOptions.DontRequireReceiver);

            yield return null;
        }

        EndDrag(tweenToEndTarget);
    }

    float _oldDrag;
    float _oldAngularDrag;
    void BeginDrag()
    {
        _oldDrag = springJoint.connectedBody.drag;
        _oldAngularDrag = springJoint.connectedBody.angularDrag;
        springJoint.connectedBody.drag = drag;
        springJoint.connectedBody.angularDrag = angularDrag;

        IsBeingDragged = true;
    }

    void EndDrag(bool doTweenToEndTarget = false)
    {
        if (springJoint.connectedBody)
        {
            springJoint.connectedBody.drag = _oldDrag;
            springJoint.connectedBody.angularDrag = _oldAngularDrag;
            springJoint.connectedBody = null;
        }

        if (doTweenToEndTarget)
        {
            Vector3 targetPos = transform.position;
            if (snapToGrid)
            { targetPos = targetPos.SnappedToGrid(gridIncrements, gridOffset); }
            iTween.MoveTo(gameObject, targetPos, 0.5f);
        }

        IsBeingDragged = false;

        StopCoroutine(DragObjectCoroutineName);
    }

    #endregion


    void OnDestroy()
    {
        iTween.Stop(gameObject, true);
        StopAllCoroutines();
    }


    void LogStatus(string msg)
    {
        if (verbose) { print(msg); }
    }
}
