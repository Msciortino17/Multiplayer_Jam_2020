using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleKiller : MonoBehaviour
{
	ParticleSystem MySystem;

	// Start is called before the first frame update
	void Start()
	{
		MySystem = GetComponent<ParticleSystem>();
	}

	// Update is called once per frame
	void Update()
	{
		if (MySystem.isStopped && !MySystem.IsAlive())
		{
			Destroy(gameObject);
		}
	}
}
