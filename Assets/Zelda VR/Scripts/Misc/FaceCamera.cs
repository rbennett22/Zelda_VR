using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    public bool yAxisOnly;

    Transform _target;


    void Awake()
    {
        _target = WorldInfo.Instance.GetPrimaryCamera().transform;
    }

    void LateUpdate()
    {
        transform.LookAt(_target);

        if (yAxisOnly)
        {
            Vector3 fwd = transform.forward;
            fwd.y = 0;
            transform.forward = fwd;
        }
    }
}