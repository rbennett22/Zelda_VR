using UnityEngine;


[ExecuteInEditMode]
public class DrawGrid : MonoBehaviour
{

	public int tileWidth = 1, tileHeight = 1;
	public int tilesWide = 30, tilesHigh = 30;
	public Vector3 localOrigin = Vector3.zero;

	public Color lineColor = Color.blue;
	public float lineAlpha = 0.3f;


	void Update () 
    {
		Color color = lineColor;
		color.a = lineAlpha;

		Vector3 worldOrigin = transform.position + localOrigin;

		Vector3 start = worldOrigin;
        Vector3 dir = Vector3.forward * tilesHigh * tileHeight;
		for(int x = 0; x <= tilesWide; x++)
        {
			Debug.DrawRay(start, dir, color);
			start.x += tileWidth;
		}

		start = worldOrigin;
        dir = Vector3.right * tilesWide * tileWidth;
		for(int z = 0; z <= tilesHigh; z++) 
        {
			Debug.DrawRay(start, dir, color);
            start.z += tileHeight;
		}
	}

}
