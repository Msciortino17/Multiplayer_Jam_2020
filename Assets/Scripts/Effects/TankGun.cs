using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankGun : MonoBehaviour
{
	private bool Recoiling;

	void Update()
	{
		if (Recoiling)
		{
			transform.Translate(0f, 0.5f * Time.deltaTime, 0f, Space.Self);
			if (transform.localPosition.x > 0.75f)
			{
				Recoiling = false;
				transform.localPosition = new Vector3(0.75f, 0f, 0f);
			}
		}
	}

	public void FireGun()
	{
		Recoiling = true;
		transform.Translate(0f, -0.5f, 0f, Space.Self);
	}
}
