using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
	private Terrain terrain;
	private GameManager manager;

	public Vector3 velocity;
	public ParticleSystem MyParticles;

	public bool PrevUseParticles;
	public float ParticleTimer;

	// Start is called before the first frame update
	void Start()
	{
		terrain = Terrain.GetReference();
		manager = GameManager.GetReference();
	}

	// Update is called once per frame
	void Update()
	{
		if (ParticleTimer > 0f)
		{
			ParticleTimer -= Time.deltaTime;
			if (ParticleTimer <= 0f)
			{
				MyParticles.Play();
			}
		}

		transform.Translate(velocity * Time.deltaTime);
		velocity += Physics.gravity * Time.deltaTime;
		velocity += manager.GetWind() * Time.deltaTime;

		WrapWorld();
		CheckGround();
	}

	private void OnTriggerEnter(Collider other)
	{
		Tank otherTank = other.GetComponent<Tank>();
		if (otherTank != null)
		{
			otherTank.Health -= 40f;
		}

		Explode();
	}

	private void CheckGround()
	{
		Vector3 position = transform.position;
		float ground = terrain.GetHeightAtX(position.x) + 0.5f;
		if (position.y < ground - 1f)
		{
			Explode();
		}
	}

	private void Explode()
	{
		MyParticles.Stop();
		MyParticles.gameObject.AddComponent<ParticleKiller>();
		MyParticles.transform.parent = null;

		Destroy(gameObject);
		manager.BulletLanded();
		// todo - cause different effects here depending on the bullet
	}

	private void WrapWorld()
	{
		if (transform.position.x > terrain.MapWidth)
		{
			Vector3 position = transform.position;
			position.x = 0.001f;
			transform.position = position;
		}

		if (transform.position.x < 0f)
		{
			Vector3 position = transform.position;
			position.x = terrain.MapWidth - 0.001f;
			transform.position = position;
		}

		bool useParticles = (transform.position.x < terrain.MapWidth * 0.99f && transform.position.x > 0.1f);
		if (!useParticles && PrevUseParticles)
		{
			MyParticles.Stop();
		}
		if (useParticles && !PrevUseParticles)
		{
			MyParticles.Play();
		}
		PrevUseParticles = useParticles;
	}
}
