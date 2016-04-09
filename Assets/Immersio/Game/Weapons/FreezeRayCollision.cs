using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Vectrosity;

public class FreezeRayCollision : MonoBehaviour
{
	public bool doToggleFreezeOnHit = false;
	public float freezeDuration = 15;

	public LayerMask freezeLayers;			// Objects on these layers will be frozen upon collision with this Projectile


	void OnCollisionEnter (Collision collision) {
		//if(!GetComponent<Projectile>()) return;

		GameObject hitObj = collision.gameObject;
		LayerMask hitObjLayerMask = 1 << hitObj.layer;
		//print("ProjectileCollision OnCollisionEnter: " + hitObj.name);

		if(freezeLayers.IncludesLayerMask(hitObjLayerMask))
			OnHitFreezableObject(hitObj.GetComponent<Freezable>());
	}

	void OnHitFreezableObject (Freezable freeze) {
		if(!freeze)
			return;
		//print("FreezeObject: " + freeze.name);

		if(freezeDuration < 0)
			freezeDuration = float.PositiveInfinity;

		if(doToggleFreezeOnHit) {
			if(freeze.IsActive)
				freeze.Deactivate();
			else
				freeze.Activate(freezeDuration);
		}
		else {
			freeze.Activate(freezeDuration);
		}
	}
}
