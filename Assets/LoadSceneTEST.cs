using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LoadSceneTEST : MonoBehaviour 
{
    IEnumerator Start()
    //void Start()
    {
        yield return new WaitForSecondsRealtime(2.5f);     // seems to be necessary to prevent OVRAvatar errors

        SceneManager.LoadScene("LoadSceneTEST", LoadSceneMode.Additive);
	}
}
