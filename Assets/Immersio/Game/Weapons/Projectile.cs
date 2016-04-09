using UnityEngine;
using System.Collections;


[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]

public class Projectile : MonoBehaviour {

	public float maxLifeTime = 2.0f;

	public float Age { get; private set; }
	public float Age_Ratio { get { return Age/maxLifeTime; } }

	public bool IsDeactivated { get; private set; }

	
	IEnumerator Start () {
		IsDeactivated = false;
		Age = 0;

		//StartCoroutine(ActivateTemporaryNoCollide(0.03f));

		yield return new WaitForSeconds(maxLifeTime);

		Deactivate();
	}

	IEnumerator ActivateTemporaryNoCollide (float duration) {
		GetComponent<Collider>().enabled = false;
		yield return new WaitForSeconds(duration);
		GetComponent<Collider>().enabled = true;
	}

	void Update () {
		Age += Time.deltaTime;
	}

	public void Deactivate () {
		if(IsDeactivated)
			return;

		TrailRenderer tr = GetComponent<TrailRenderer>();
		if(tr && tr.autodestruct) {
			// This Projectile will be auto-destroyed when lineTrail disappears, so just stop and hide it for now.
			if(GetComponent<Renderer>()) GetComponent<Renderer>().enabled = false;
			GetComponent<Collider>().enabled = false;
			GetComponent<Rigidbody>().Sleep();
			Destroy(gameObject, 0.5f);
		}
		else
			Destroy(gameObject);

		IsDeactivated = true;
	}

	void OnDestroy () {
		IsDeactivated = true;
	}
}
