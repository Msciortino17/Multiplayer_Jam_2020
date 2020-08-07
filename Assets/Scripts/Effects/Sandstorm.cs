using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sandstorm : MonoBehaviour
{
	public ParticleSystem LeftWind;
	public ParticleSystem RightWind;

	// Start is called before the first frame update
	void Start()
	{
	}

	public void UpdateWind(int direction)
	{
		if (direction == 0)
		{
			LeftWind.Stop();
			RightWind.Stop();
		}
		else if (direction == -1)
		{
			LeftWind.Stop();
			RightWind.Play();
		}
		else
		{
			LeftWind.Play();
			RightWind.Stop();
		}
	}
}
