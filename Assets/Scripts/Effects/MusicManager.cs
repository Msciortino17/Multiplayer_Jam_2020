using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{

	public AudioClip MenuMusic;
	public List<AudioClip> Soundtrack;
	public int CurrentSong;
	private AudioSource MyAudioSource;
	public bool Menu;

	private static MusicManager reference;

	public static MusicManager GetReference()
	{
		if (reference == null)
		{
			reference = GameObject.Find("MusicManager").GetComponent<MusicManager>();
		}
		return reference;
	}

	// Start is called before the first frame update
	void Start()
	{
		MyAudioSource = GetComponent<AudioSource>();
		PlayMenuMusic();
	}

	// Update is called once per frame
	void Update()
	{
		if (!Menu && !MyAudioSource.isPlaying)
		{
			NextSong();
		}
	}

	private void NextSong()
	{
		CurrentSong++;
		if (CurrentSong >= Soundtrack.Count)
		{
			CurrentSong = 0;
		}

		MyAudioSource.clip = Soundtrack[CurrentSong];
		MyAudioSource.Play();
	}

	public void PlayMenuMusic()
	{
		if (Menu)
		{
			return;
		}

		Menu = true;
		MyAudioSource.Stop();
		MyAudioSource.clip = MenuMusic;
		MyAudioSource.Play();
	}

	public void PlayBattleMusic()
	{
		if (!Menu)
		{
			return;
		}

		CurrentSong = Random.Range(0, Soundtrack.Count);
		Menu = false;
		MyAudioSource.Stop();
		NextSong();
	}
}
