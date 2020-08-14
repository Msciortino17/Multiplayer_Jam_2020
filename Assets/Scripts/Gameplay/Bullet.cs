using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class Bullet : MonoBehaviour
{
	private Terrain terrain;
	private GameManager manager;

	public Vector3 velocity;
	public ParticleSystem MyParticles;

	public bool PrevUseParticles;
	public float ParticleTimer;

	public bool IsDrone;
	public bool IsHyperSonic;
	public bool IsScatter;
	public bool SpawnExplosion;
	public float Damage;
	public float ExplosionRadius;
	public GameObject ExplosionPrefab;
	public GameObject DirtParticlePrefab;
	public GameObject ScatterBulletPrefab;
	public GameObject HypersonicBurstPrefab;
	public GameObject ScatterParticlePrefab;

	private bool Descending;
	private bool GoingLeft;
	private float PrevY;
	public float HyperAccel;
	public bool IgnoreLanding;

	public AudioClip ExplosionSound;

	CameraEffects cameraEffects;

	// Start is called before the first frame update
	void Start()
	{
		terrain = Terrain.GetReference();
		manager = GameManager.GetReference();
		cameraEffects = CameraEffects.GetReference();
		PrevY = transform.position.y;
	}

	// Update is called once per frame
	void Update()
	{
		WrapWorld();
		CheckGround();

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

		if (PrevY > transform.position.y)
		{
			if (!Descending && IsHyperSonic)
			{
				Instantiate(HypersonicBurstPrefab, transform.position, Quaternion.identity, null);
			}
			if (!Descending && IsScatter)
			{
				Instantiate(ScatterParticlePrefab, transform.position, Quaternion.identity, null);
				for (int i = 0; i < 5; i++)
				{
					Bullet smallBullet = Instantiate(ScatterBulletPrefab, transform.position, Quaternion.identity, null).GetComponent<Bullet>();
					smallBullet.velocity = velocity;
					smallBullet.velocity.x -= i;
					if (i != 0)
					{
						smallBullet.IgnoreLanding = true;
					}
				}
				Destroy(gameObject);
			}
			Descending = true;
		}
		if (Descending && IsHyperSonic)
		{
			if (velocity.x > 0f)
			{
				velocity.x += HyperAccel * Time.deltaTime;
			}
			else
			{
				velocity.x -= HyperAccel * Time.deltaTime;
			}
		}
		PrevY = transform.position.y;
	}

	private void OnTriggerEnter(Collider other)
	{
		Tank otherTank = other.GetComponent<Tank>();
		if (otherTank != null)
		{
			if (!SpawnExplosion)
			{
				if (!manager.OnlineGame || PhotonNetwork.IsMasterClient)
				{
					otherTank.Health -= Damage;
					if (PhotonNetwork.IsMasterClient)
					{
						Hashtable hash = otherTank.MyPlayer.CustomProperties;
						hash["Health"] = otherTank.Health;
						otherTank.MyPlayer.SetCustomProperties(hash);
					}
				}

				BulletExplosion explosion = Instantiate(ExplosionPrefab).GetComponent<BulletExplosion>();
				explosion.transform.position = transform.position;
				explosion.transform.localScale = new Vector3(ExplosionRadius, ExplosionRadius, ExplosionRadius);
				explosion.Damage = 0;
				AudioSource audio = explosion.GetComponent<AudioSource>();
				audio.clip = ExplosionSound;
				audio.Play();
			}
			Explode();
		}
	}

	private void CheckGround()
	{
		Vector3 position = transform.position;
		float ground = terrain.GetHeightAtX(position.x);
		if (position.y < ground )
		{
			if (!SpawnExplosion)
			{
				Instantiate(DirtParticlePrefab, transform.position, Quaternion.identity, null);
			}
			Explode();
		}
	}

	private void Explode()
	{
		cameraEffects.Shake(0.25f);
		MyParticles.Stop();
		MyParticles.gameObject.AddComponent<ParticleKiller>();
		MyParticles.transform.parent = null;

		if (SpawnExplosion)
		{
			BulletExplosion explosion = Instantiate(ExplosionPrefab).GetComponent<BulletExplosion>();
			explosion.transform.position = transform.position;
			explosion.transform.localScale = new Vector3(ExplosionRadius, ExplosionRadius, ExplosionRadius);
			explosion.Damage = Damage;
			AudioSource audio = explosion.GetComponent<AudioSource>();
			audio.clip = ExplosionSound;
			audio.Play();
		}

		Destroy(gameObject);
		if (!IgnoreLanding)
		{
			manager.BulletLanded();
		}
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
