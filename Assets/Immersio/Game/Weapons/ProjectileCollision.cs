using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Vectrosity;

[RequireComponent(typeof(Projectile))]
public class ProjectileCollision : MonoBehaviour {

	public int damage = 1;
	public bool doDrawHitIndicator = true;
	public int ricochets = 1;

	public LayerMask damageLayers;			// Objects on these layers will be damaged upon collision with this Projectile (assuming object has HealthController component)
	public LayerMask deactivationLayers;	// This Projectile will deactivate upon colliding with Objects on these layers (regardless of remaining ricochets)


	int _ricochetCount = 0;


	virtual protected void OnCollisionEnter (Collision collision) {
		GameObject mainHitObj = collision.gameObject;
		//print("ProjectileCollision OnCollisionEnter: " + mainHitObj.name);
		LayerMask mainHitObjLayerMask = 1 << mainHitObj.layer;

		//print("***-------------");
		foreach(ContactPoint contact in collision.contacts) {
			if(contact.otherCollider.isTrigger)
				continue;
			//print(contact.thisCollider.name + " hit " + contact.otherCollider.name);

			GameObject subHitObj = contact.otherCollider.gameObject;

			if(damageLayers.IncludesLayerMask(mainHitObjLayerMask)) {
				HealthController health = subHitObj.GetComponent<HealthController>();
				if(health)
					DealDamage(health);
			}

			break;
		}
		//print("-------------***");

		if(++_ricochetCount > ricochets || deactivationLayers.IncludesLayerMask(mainHitObjLayerMask))
			GetComponent<Projectile>().Deactivate();

		mainHitObj.BroadcastMessage("OnProjectileHit", collision, SendMessageOptions.DontRequireReceiver);
	}

	virtual protected bool DealDamage (HealthController healthController) {
		if(healthController == null)
			return false;
		//print("DealDamage: " + healthController.gameObject.name);
		
		if(doDrawHitIndicator)
			DrawHitIndicator(transform.position);

		return healthController.TakeDamage((uint)damage, gameObject);
	}


	virtual protected void DrawHitIndicator (Vector3 position, float duration = 0.2f) {
		const int NumSegments = 24;
		const int lineWidth = 30;
		Color color = Color.red;
		color.a = 0.1f;

		VectorLine vecLine = new VectorLine("SpitBallVec", new Vector3[NumSegments + 1], color, null, lineWidth, LineType.Continuous, Joins.Fill);
		vecLine.MakeCircle(position, 0.15f, NumSegments);
		vecLine.Draw3DAuto(duration);
	}
}
