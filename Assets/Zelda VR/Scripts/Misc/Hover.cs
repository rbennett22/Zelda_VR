using UnityEngine;

public class Hover : MonoBehaviour
{
    public float speed = 1;
    public float amplitude = 1;
    public Vector3 up = Vector3.up;


    float _prevT;


    void Start()
    {
        up.Normalize();
    }

	void Update () 
    {
        float t = amplitude * Mathf.Sin(speed * Time.time);
        
        transform.localPosition += (t - _prevT) * up;

        _prevT = t;
	}
}