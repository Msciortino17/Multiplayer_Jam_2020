using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
	private Terrain terrain;

	public Vector3 velocity;

	// Start is called before the first frame update
	void Start()
	{
		terrain = Terrain.GetReference();

	}

	// Update is called once per frame
	void Update()
	{
		transform.Translate(velocity * Time.deltaTime);
		velocity += Physics.gravity * Time.deltaTime;

		CheckGround();
	}

	private void OnTriggerEnter(Collider other)
	{
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
		Debug.Log("kaboom!");
		Destroy(gameObject);
		// todo - cause different effects here depending on the bullet
	}
}
