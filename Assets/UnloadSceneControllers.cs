using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class UnloadSceneControllers : MonoBehaviour 
{
    void Start()
    {
        SceneManager.UnloadScene("Controllers");
	}
}
