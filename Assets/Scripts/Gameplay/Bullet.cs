using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
	private Terrain terrain;
	private GameManager manager;

	public Vector3 velocity;

	// Start is called before the first frame update
	void Start()
	{
		terrain = Terrain.GetReference();
		manager = GameManager.GetReference();
	}

	// Update is called once per frame
	void Update()
	{
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
			otherTank.Health -= 20f;
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
	}
}
