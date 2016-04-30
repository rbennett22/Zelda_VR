using UnityEngine;


public class SwordProjectileHit : MonoBehaviour
{
    public float speed = 5;
    public float lifetime = 0.3f;

    public Transform TL, TR, BL, BR;


    void Start()
    {
        Destroy(gameObject, lifetime);
    }

	void Update () 
    {
        float moveDist = speed * Time.deltaTime;

        TL.localPosition += new Vector3(-1, 1, 0) * moveDist;
        TR.localPosition += new Vector3( 1, 1, 0) * moveDist;
        BL.localPosition += new Vector3(-1,-1, 0) * moveDist;
        BR.localPosition += new Vector3( 1,-1, 0) * moveDist;
	}

}