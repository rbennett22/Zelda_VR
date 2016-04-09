using UnityEngine;


public class TitleRoomRotation : MonoBehaviour
{

    public float horizontalShiftSpeed = 10.0f;


	void Update () 
    {
        Rotate();
	}

    void Rotate()
    {
        float dot = Vector3.Dot(CommonObjects.PlayerController_C.LineOfSight, Vector3.right);
        float rotY = -dot * horizontalShiftSpeed;
        Vector3 euler = transform.rotation.eulerAngles;
        transform.rotation = Quaternion.Euler(euler.x, rotY, euler.z);
    }

}