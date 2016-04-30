using UnityEngine;

public class DestroyChildrenOnLevelLoad : MonoBehaviour 
{
    void OnLevelWasLoaded(int level)
    {
        //print(" OnLevelWasLoaded: " + level);

        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }
}