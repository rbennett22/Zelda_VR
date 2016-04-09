using UnityEngine;

[ExecuteInEditMode]

public class AutoSnapToGrid : MonoBehaviour 
{

    public float xSnap = 0;
    public float ySnap = 0;
    public float zSnap = 0;

    public float xOffset = 0;
    public float yOffset = 0;
    public float zOffset = 0;


    void Awake()
    {
        if (Application.isPlaying) 
        {
            enabled = false;
        }
    }


	void Update ()
    {
        foreach (Transform child in transform)
        {
            /*Vector3 pos = child.localPosition;
            if (xSnap > 0) { pos.x = (((int)((pos.x - xOffset) / xSnap)) * xSnap) + xOffset; }
            if (ySnap > 0) { pos.y = (((int)((pos.y - yOffset) / ySnap)) * ySnap) + yOffset; }
            if (zSnap > 0) { pos.z = (((int)((pos.z - zOffset) / zSnap)) * zSnap) + zOffset; }*/
            child.localPosition = child.localPosition.SnappedToGrid(new Vector3(xSnap, ySnap, zSnap), new Vector3(xOffset, yOffset, zOffset));
        }
	}

}