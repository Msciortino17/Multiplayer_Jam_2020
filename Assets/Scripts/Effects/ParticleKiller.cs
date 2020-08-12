using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleKiller : MonoBehaviour
{
	public List<ParticleSystem> MySystems;

	// Start is called before the first frame update
	void Start()
	{
		MySystems = new List<ParticleSystem>();
		ParticleSystem mySystem = GetComponent<ParticleSystem>();
		if (mySystem != null)
		{
			MySystems.Add(mySystem);
		}
		MySystems.AddRange(GetComponentsInChildren<ParticleSystem>());
	}

	// Update is called once per frame
	void Update()
	{
		bool destroy = true;
		foreach (ParticleSystem system in MySystems)
		{
			if (!system.isStopped || system.IsAlive())
			{
				destroy = false;
			}
		}
		if (destroy)
		{
			Destroy(gameObject);
		}
	}
}
