using UnityEngine;

public class AutoDestroy : MonoBehaviour 
{
    public float delay = 0.1f;

	void Start ()
    {
        Destroy(gameObject, delay);
	}
}