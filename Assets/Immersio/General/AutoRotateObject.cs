using UnityEngine;
using System.Collections;

public class AutoRotateObject : MonoBehaviour
{
	public float speed = 1;
	public Vector3 axis = Vector3.forward;

	void Update () {
		transform.Rotate(axis, speed * Time.deltaTime);
	}
}
