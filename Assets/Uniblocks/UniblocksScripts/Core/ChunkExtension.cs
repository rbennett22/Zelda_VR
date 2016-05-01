using UnityEngine;
using System.Collections;

public class ChunkExtension : MonoBehaviour {
	void Awake () {
		if (GetComponent<MeshRenderer>() == null) {
			gameObject.layer = 26;

		}
	}
}
