using UnityEngine;

public class FairyWander : MonoBehaviour
{
    const float DIRECTION_CHANGE_INTERVAL = 1.5f;
    const float MAX_LIFETIME = 30;


    public float speed;


    Vector3 _moveDirection;


    void Start()
    {
        InvokeRepeating("ChangeDirection", 0, DIRECTION_CHANGE_INTERVAL);

        Destroy(gameObject, MAX_LIFETIME);
    }


    void ChangeDirection()
    {
        _moveDirection = EnemyAI_Random.GetRandomDirectionXZ();
    }

    void Update()
    {
        if (WorldInfo.Instance.IsInDungeon) // TODO: Remove this, and ensure fairies don't move outside of dungeon rooms
        {
            return;
        }

        transform.position += (Time.deltaTime * speed) * _moveDirection;
    }
}