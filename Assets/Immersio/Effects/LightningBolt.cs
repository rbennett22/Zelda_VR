/*
	This script is placed in public domain. The author takes no responsibility for any possible harm.
	Contributed by Jonathan Czeck
*/
using UnityEngine;
using System.Collections;

public class LightningBolt : MonoBehaviour
{
	public Transform target;
	public Vector3 originOffset;
	public Vector3 targetOffset;

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

	public Color color = Color.white;
	public Light startLight;
	public Light endLight;

	public bool restrainZigX = false;
	public bool restrainZigY = false;
	public bool restrainZigZ = false;
	

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

		if(startLight)
			startLight.enabled = doActivate;
		if(endLight)
			endLight.enabled = doActivate;

		IsActive = doActivate;
	}
	
	void Update ()
	{
		if(!target)
			return;
		if(!IsActive)
			return;
		if (_noise == null)
			_noise = new Perlin();
			
		float timex = Time.time * speed * 0.1365143f;
		float timey = Time.time * speed * 1.21688f;
		float timez = Time.time * speed * 2.5564f;
		
		for (int i=0; i < _particles.Length; i++)
		{
			Vector3 position = Vector3.Lerp(transform.position + originOffset, target.position + targetOffset, _oneOverZigs * (float)i);
			float offsetX = restrainZigX ? 0 : _noise.Noise(timex + position.x, timex + position.y, timex + position.z);
			float offsetY = restrainZigY ? 0 : _noise.Noise(timey + position.x, timey + position.y, timey + position.z);
			float offsetZ = restrainZigZ ? 0 : _noise.Noise(timez + position.x, timez + position.y, timez + position.z);
			Vector3 offset = new Vector3(offsetX, offsetY, offsetZ);

			position += (offset * scale * ((float)i * _oneOverZigs));
			
			_particles[i].position = position;
			_particles[i].color = color;
			_particles[i].energy = 1f;
		}
		
		GetComponent<ParticleEmitter>().particles = _particles;
		
		if (GetComponent<ParticleEmitter>().particleCount >= 2)
		{
			if (startLight)
				startLight.transform.position = _particles[0].position;
			if (endLight)
				endLight.transform.position = _particles[_particles.Length - 1].position;
		}
	}	
}