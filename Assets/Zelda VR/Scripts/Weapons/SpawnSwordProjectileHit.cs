using UnityEngine;

public class SpawnSwordProjectileHit : MonoBehaviour 
{
    public GameObject hitAnimationPrefab;

    void OnCollisionEnter(Collision collision)
    {
        Instantiate(hitAnimationPrefab, transform.position, Quaternion.identity);
    }
}