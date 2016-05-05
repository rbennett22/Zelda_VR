using UnityEngine;


public class EnemyAI_Boulder : EnemyAI
{
    const float LifeTime = 3.0f;
    const float PushForce = 135.0f;
    const float SpawnHeightOffset = 2.5f;


	void Start ()
    {
        PositionAtopBlocks();

        Push(Vector3.back);     // Blocks always fall towards the "south"

        Destroy(gameObject, LifeTime);
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
            Vector3 newPos = hitInfo.collider.transform.position;
            newPos.y += SpawnHeightOffset;
            transform.position = newPos;
        }
    }

    void Push(Vector3 direction)
    {
        GetComponent<Rigidbody>().AddForce(direction * PushForce);
    }


    void Update()
    {
        if (!_doUpdate) { return; }
        if (IsPreoccupied) { return; }

        AnimatorInstance.speed = (GetComponent<Rigidbody>().velocity.magnitude < 0.5f) ? 0 : 0.5f;
    }
}