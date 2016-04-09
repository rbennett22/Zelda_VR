using UnityEngine;


public class HoleMarker : MonoBehaviour 
{
    public bool appearsOnPushBlock;

    void Awake()
    {
        GetComponent<Renderer>().enabled = false;
    }
}