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
        _moveDirection = EnemyAI_Random.GetRandomDirectionXZ();
    }

    void Update()
    {
        transform.position += (Time.deltaTime * speed) * _moveDirection;
    }
}