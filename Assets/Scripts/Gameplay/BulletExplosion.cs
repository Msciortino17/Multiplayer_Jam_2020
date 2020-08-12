using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletExplosion : MonoBehaviour
{
	public float Damage;

	void Start()
	{
		Terrain terrain = Terrain.GetReference();
		float slope = terrain.GetSlopeAtX(transform.position.x);
		float angle = terrain.SlopeToDegrees(slope);
		transform.rotation = Quaternion.Euler(0f, 0f, angle);
	}

	private void OnTriggerEnter(Collider other)
	{
		Tank otherTank = other.GetComponent<Tank>();
		if (otherTank != null)
		{
			otherTank.Health -= Damage;
		}
	}
}
