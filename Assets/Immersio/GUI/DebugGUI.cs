using UnityEngine;

public class DebugGUI : MonoBehaviour {

	public GUISkin guiSkin;


	static string _debugOutput = "";
	static public string DebugOutput {
		get { return _debugOutput; }
		set { _debugOutput = value; Debug.Log(_debugOutput); }
	}
	void OnGUI () {
		GUI.skin = guiSkin;

		if(!string.IsNullOrEmpty(_debugOutput))
			GUI.Box(new Rect(20, 400, 200, 200), "DEBUG LOG \n\n" + _debugOutput);
	}
}
