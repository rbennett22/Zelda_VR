using UnityEngine;


public class Patrol : MonoBehaviour 
{

    public Vector3 positionA;
    public Vector3 positionB;
    public float speed = 1;


    bool movingToA = false;


	void Update () 
    {
        Vector3 pos = transform.position;
        Vector3 dest = movingToA ? positionA : positionB;
        Vector3 dir = dest - pos;
        float distSqr = dir.sqrMagnitude;
        dir.Normalize();
        Vector3 displacement = dir * speed * Time.deltaTime;
        pos += displacement;
        transform.position = pos;
        transform.forward = dir;

        if (distSqr < 0.1f)
        {
            movingToA = !movingToA;
        }
	}
}
