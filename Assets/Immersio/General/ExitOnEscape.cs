using UnityEngine;
using System.Collections;

public class ExitOnEscape : MonoBehaviour 
{
	void Update ()
    {
		if(Input.GetKeyUp(KeyCode.Escape))
			Application.Quit();     // TODO: "Are you sure?"
	}
}
