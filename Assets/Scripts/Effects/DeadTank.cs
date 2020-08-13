using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadTank : MonoBehaviour
{
	public Transform TurretPivot;

	public SpriteRenderer TurretSprite;
	public ParticleSystem SmokeParticles;
	public List<ParticleSystem> SparkParticles;
	public List<ParticleSystem> ExplosionParticles;
	public List<AudioSource> SparkSounds;
	public AudioSource ExplosionAudio;
	public float ParticleTimer;
	public float ParticleTimerSeconds;

	// Start is called before the first frame update
	void Start()
	{

	}

	// Update is called once per frame
	void Update()
	{
		if (ParticleTimerSeconds > 0)
		{
			ParticleTimer -= Time.deltaTime;
			if (ParticleTimer <= 0f)
			{
				ParticleTimerSeconds--;
				ParticleTimer = 1f;

				if (ParticleTimerSeconds > 4)
				{
					int spark = Random.Range(0, SparkParticles.Count);
					SparkParticles[spark].Play();
					SparkSounds[spark].Play();
				}
				if (ParticleTimerSeconds == 4)
				{
					for (int i = 0; i < ExplosionParticles.Count; i++)
					{
						ExplosionParticles[i].Play();
					}
					CameraEffects.GetReference().Shake(0.5f);
					ExplosionAudio.Play();
				}

				if (ParticleTimerSeconds <= 1)
				{
					SmokeParticles.Stop();
				}
			}
		}
	}

	public void Init(Vector3 position, Quaternion rotation, Quaternion turretRotation, Color color)
	{
		transform.position = position;
		transform.rotation = rotation;
		TurretPivot.transform.rotation = turretRotation;
		TurretSprite.color = color;

		ParticleTimer = 1f;
		ParticleTimerSeconds = 10;
	}
}
