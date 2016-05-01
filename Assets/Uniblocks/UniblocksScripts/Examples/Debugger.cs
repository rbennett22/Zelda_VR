using UnityEngine;
using System.Collections;

// keyboard shortcuts for some common tasks, and saving the world periodically

namespace Uniblocks {

public class Debugger : MonoBehaviour {
	
	public GameObject Flashlight, Torch;
	private float saveTimer = 60.0f;
	public bool ShowGUI;

	
	
	void Update () {
	
		// keyboard shortcuts
		
		if (Input.GetKeyDown("space") && Time.realtimeSinceStartup > 3.0f) {
			GetComponent<CharacterMotor>().enabled = true;
		}
	
		if (Input.GetKeyDown("v")) {
			Engine.SaveWorldInstant();
		}
				
		if (Input.GetKeyDown ("f")) {
			if (Flashlight.GetComponent<Light>().enabled == true) 	Flashlight.GetComponent<Light>().enabled = false;
			else Flashlight.GetComponent<Light>().enabled = true;
		}
		
		if (Input.GetKeyDown ("t")) {
			if (Torch.GetComponent<Light>().enabled == true) 	Torch.GetComponent<Light>().enabled = false;
			else Torch.GetComponent<Light>().enabled = true;
		}
		
		
		// world save timer
		if (saveTimer < 0.0f) {
			saveTimer = 60.0f;
			Engine.SaveWorld();
		}
		else {
			saveTimer -= Time.deltaTime;
		}
		
	}
	
	
	void OnGUI () {
	
		// GUI info box
		if (ShowGUI) {
			GUILayout.BeginHorizontal ();
				GUILayout.Space (20);
				GUILayout.BeginVertical();
					GUILayout.Space (Screen.height - 200);
					GUILayout.BeginVertical("Box");
						GUILayout.Label ("1-9 - select block");
						GUILayout.Label ("RMB - place block");
						GUILayout.Label ("LMB - remove block");
						GUILayout.Label ("F - toggle flashlight");
						GUILayout.Label ("T - toggle torch");
						GUILayout.Label ("R - toggle speed boost");
						GUILayout.Label ("V - save world");
					GUILayout.EndVertical();
				GUILayout.EndVertical();
			GUILayout.EndHorizontal();
		}
	}
	

}

}
