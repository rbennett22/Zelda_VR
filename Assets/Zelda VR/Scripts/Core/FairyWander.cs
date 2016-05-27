using UnityEngine;

public class FairyWander : MonoBehaviour
{
    const float DIRECTION_CHANGE_INTERVAL = 1.5f;


    public float speed;


    Vector3 _moveDirection;


    void Start()
    {
        InvokeRepeating("ChangeDirection", 0, DIRECTION_CHANGE_INTERVAL);
    }


    void ChangeDirection()
    {
        _moveDirection = GetRandomDirection();
    }
    Vector3 GetRandomDirection()
    {
        Vector3 dir = Vector3.zero;
        dir.x = Random.Range(-1.0f, 1.0f);
        dir.z = Random.Range(-1.0f, 1.0f);

        return dir.normalized;
    }


    void Update()
    {
        transform.position += (Time.deltaTime * speed) * _moveDirection;
    }
}