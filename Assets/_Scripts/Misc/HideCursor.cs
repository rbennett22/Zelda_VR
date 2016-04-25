using UnityEngine;

public class HideCursor : MonoBehaviour
{
    [SerializeField]
    bool _disableInEditor = true;

	void Start ()
    {
        if (_disableInEditor && Application.isEditor)
            return;
        
        Cursor.visible = false;
    }
}