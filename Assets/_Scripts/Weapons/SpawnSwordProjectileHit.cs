using UnityEngine;

public class SpawnSwordProjectileHit : MonoBehaviour 
{
    public GameObject hitAnimationPrefab;


    void OnCollisionEnter(Collision collision)
    {
        GameObject g = Instantiate(hitAnimationPrefab, transform.position, Quaternion.identity) as GameObject;
    }

}