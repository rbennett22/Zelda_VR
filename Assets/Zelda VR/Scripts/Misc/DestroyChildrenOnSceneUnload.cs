using UnityEngine;
using UnityEngine.SceneManagement;

public class DestroyChildrenOnSceneUnload : MonoBehaviour
{
    void Awake()
    {
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    void OnSceneUnloaded(Scene scene)
    {
        //print(" OnSceneUnloaded: " + scene);

        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }
}