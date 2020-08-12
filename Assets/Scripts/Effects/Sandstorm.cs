using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sandstorm : MonoBehaviour
{
	CameraEffects cameraEffects;
	DarkOverlay darkness;
	public ParticleSystem LeftWind;
	public ParticleSystem RightWind;
	private AudioSource MyAudio;
	public int CurrentDirection;

	// Start is called before the first frame update
	void Start()
	{
		MyAudio = GetComponent<AudioSource>();
		darkness = DarkOverlay.GetReference();
		cameraEffects = CameraEffects.GetReference();
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
			darkness.SetDarkness(0f);
			cameraEffects.SetSandStorm(false);
			Stop();
		}
		else if (direction == -1)
		{
			darkness.SetDarkness(0.35f);
			cameraEffects.SetSandStorm(true);
			LeftWind.Stop();
			RightWind.Play();
		}
		else
		{
			darkness.SetDarkness(0.35f);
			cameraEffects.SetSandStorm(true);
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
