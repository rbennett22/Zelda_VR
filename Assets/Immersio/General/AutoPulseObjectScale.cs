using UnityEngine;
using System.Collections;

public class AutoPulseObjectScale : MonoBehaviour
{
	public float speed = 1.0f;
	public float amplitude = 0.3f;

	Vector3 _origScale;

	float _age = 0;


	void Start () {
		_origScale = transform.localScale;
	}

	void Update () {
		_age += Time.deltaTime;
		transform.localScale = _origScale * (1 + amplitude * Mathf.Sin(_age * speed));
	}
}
