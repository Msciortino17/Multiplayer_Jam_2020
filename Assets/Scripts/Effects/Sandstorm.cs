using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sandstorm : MonoBehaviour
{
	public ParticleSystem LeftWind;
	public ParticleSystem RightWind;
	private AudioSource MyAudio;
	public int CurrentDirection;

	// Start is called before the first frame update
	void Start()
	{
		MyAudio = GetComponent<AudioSource>();
	}

	void Update()
	{
		if (CurrentDirection == 0)
		{
			StopAudio();
		}
		else
		{
			PlayAudio();
		}
	}

	public void UpdateWind(int direction)
	{
		CurrentDirection = direction;
		if (direction == 0)
		{
			Stop();
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

	public void Stop()
	{
		LeftWind.Stop();
		RightWind.Stop();
	}

	public void PlayAudio()
	{
		if (MyAudio.volume < 1f)
		{
			MyAudio.volume += Time.deltaTime;
		}
	}

	public void StopAudio()
	{
		if (MyAudio.volume > 0f)
		{
			MyAudio.volume -= Time.deltaTime;
		}
	}
}
