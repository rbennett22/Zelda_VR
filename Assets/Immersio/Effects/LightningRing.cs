/*
	This script is placed in public domain. The author takes no responsibility for any possible harm.
	Contributed by Jonathan Czeck
*/
using UnityEngine;
using System.Collections;

public class LightningRing : MonoBehaviour
{
	public Vector3 originOffset;

	public int zigs = 100;
	public int Zigs {
		get { return zigs; } 
		set { 
			zigs = value; 
			_oneOverZigs = 1f / (float)zigs;
			GetComponent<ParticleEmitter>().Emit(zigs);
			_particles = GetComponent<ParticleEmitter>().particles;
		} 
	}
	float _oneOverZigs;

	public float speed = 1f;
	public float scale = 1f;
	public float radius = 1f;

	public Color color = Color.white;
	

	Perlin _noise;
	
	Particle[] _particles;

	
	void Start()
	{
		_oneOverZigs = 1f / (float)zigs;
		GetComponent<ParticleEmitter>().emit = false;

		GetComponent<ParticleEmitter>().Emit(zigs);
		_particles = GetComponent<ParticleEmitter>().particles;

		IsActive = true;
		Activate(false);
	}

	public bool IsActive { get; private set; }
	public void Activate (bool doActivate = true) {
		if(doActivate == IsActive)
			return;
		if(!doActivate)
			GetComponent<ParticleEmitter>().ClearParticles();
		IsActive = doActivate;
	}
	
	void Update () {
		if(!IsActive)
			return;
		if (_noise == null)
			_noise = new Perlin();

		float timeAngle		= Time.time * speed * 0.14f;
		//float timeAngle		= Time.time * speed * 0.56f;
		float timeRadius	= Time.time * speed * 1.2f;
		float timeHeight	= Time.time * speed * 2.6f;

		for (int i=0; i < _particles.Length; i++)
		{
			float ratio = (float)i *_oneOverZigs;
			float angle = Mathf.LerpAngle(0, 90, ratio) * 4f;
			float angleOffset = 10f * _noise.Noise(timeAngle + angle);
			//float angleOffset = 3f * Mathf.Sin(timeAngle + angle);
			angle += angleOffset;

			Vector3 position = Quaternion.AngleAxis(angle, Vector3.forward) * Vector3.right;
			float radiusOffset = scale * _noise.Noise(timeRadius + angle);
			//float radiusOffset = scale * Mathf.Sin(timeRadius + angle);
			position *= radius + radiusOffset;

			float heightOffset = scale * _noise.Noise(timeHeight + angle);
			//float heightOffset = scale * Mathf.Cos(timeRadius + angle);
			position.z += heightOffset;

			position += transform.position + originOffset;
			
			_particles[i].position = position;
			_particles[i].color = color;
			_particles[i].energy = 1f;
		}
		
		GetComponent<ParticleEmitter>().particles = _particles;
	}	
}