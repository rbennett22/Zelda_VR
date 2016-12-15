using UnityEngine;


public class EnemyAI_Boulder : EnemyAI
{
    const float LIFE_TIME_MIN = 3.5f;
    const float LIFE_TIME_MAX = 4.5f;
    const float PUSH_FORCE = 135.0f;
    const float SPAWN_HEIGHT_OFFSET = 2.5f;


    Rigidbody _rigidbody;


    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        
        PositionAtopBlocks();
        Push(Vector3.back);     // Blocks always fall towards the "south"

        float lifeTime = Random.Range(LIFE_TIME_MIN, LIFE_TIME_MAX);
        Destroy(gameObject, lifeTime);
    }

    void PositionAtopBlocks()
    {
        RaycastHit hitInfo;
        bool hit = true;

        Vector3 rayOrigin = transform.position;
        rayOrigin.y += 100;

        Ray ray = new Ray(rayOrigin, Vector3.down);
        LayerMask mask = Extensions.GetLayerMaskIncludingLayers("Blocks");
        hit = Physics.Raycast(ray, out hitInfo, 999, mask);
        if (hit)
        {
            Vector3 p = hitInfo.point;
            p.y += SPAWN_HEIGHT_OFFSET;
            transform.position = p;
        }
    }

    void Push(Vector3 direction)
    {
        direction.Normalize();
        _rigidbody.AddForce(direction * PUSH_FORCE);
    }


    void Update()
    {
        if (!_doUpdate) { return; }
        if (IsPreoccupied) { return; }

        AnimatorInstance.speed = (_rigidbody.velocity.magnitude < 0.5f) ? 0 : 0.5f;
    }



}