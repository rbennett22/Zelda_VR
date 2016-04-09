using UnityEngine;


public class Hover : MonoBehaviour
{

    public float speed = 1;
    public float amplitude = 1;
    public Vector3 up = Vector3.up;


    Vector3 _origin;
    float _prevT;


    void Start()
    {
        _origin = transform.localPosition;
        up.Normalize();
    }

	void Update () 
    {
        float t = amplitude * Mathf.Sin(speed * Time.time);
        
        //transform.localPosition = _origin + t * up;
        transform.localPosition += (t - _prevT) * up;

        _prevT = t;
	}

}
