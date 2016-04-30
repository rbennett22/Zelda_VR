using UnityEngine;

public class DisableOnAwake : MonoBehaviour 
{
	void Awake ()
    {
        gameObject.SetActive(false);
	}
}
