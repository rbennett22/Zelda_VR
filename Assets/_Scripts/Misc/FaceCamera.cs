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
            Vector3 euler = transform.rotation.eulerAngles;
            euler.x = 0;
            euler.z = 0;
            transform.rotation = Quaternion.Euler(euler);
        }
    }
}
