using UnityEngine;

public class ExitOnEscape : MonoBehaviour 
{
    [SerializeField]
    bool _requireShiftKey;


	void Update ()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (_requireShiftKey && !(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
            {
                return;
            }

            Quit();     // TODO: "Are you sure?"
        }
	}

    void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
