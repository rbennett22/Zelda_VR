using UnityEngine;

public class FairyWander : MonoBehaviour
{
    public float speed;

    Vector3 _direction;


    void Start()
    {
        _direction = Vector3.zero;
        _direction.x = Random.RandomRange(-1.0f, 1.0f);
        _direction.z = Random.RandomRange(-1.0f, 1.0f);
        _direction.Normalize();
    }
	
	void Update () 
    {
        transform.position += (Time.deltaTime * speed) * _direction;
	}

}