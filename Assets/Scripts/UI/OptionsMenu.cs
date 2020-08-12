using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class OptionsMenu : MonoBehaviour
{
	public bool FromMainMenu;
	private const string effectVolumeKey = "EffectVolume";
	private const string musicVolumeKey = "MusicVolume";

	// References
	public MainMenu MainMenuRef;
	public PauseMenu PauseMenuRef;
	public Slider MusicSlider;
	public Slider EffectSlider;
	public AudioMixer MasterMixer;

	// Start is called before the first frame update
	void Start()
	{
		float effectVolume = -10f;
		if (PlayerPrefs.HasKey(effectVolumeKey))
		{
			effectVolume = PlayerPrefs.GetFloat(effectVolumeKey);
		}
		MasterMixer.SetFloat("EffectVolume", effectVolume);

		float musicVolume = -10f;
		if (PlayerPrefs.HasKey(musicVolumeKey))
		{
			musicVolume = PlayerPrefs.GetFloat(musicVolumeKey);
		}
		MasterMixer.SetFloat("MusicVolume", musicVolume);

		EffectSlider.value = effectVolume;
		MusicSlider.value = musicVolume;
	}

	// Update is called once per frame
	void Update()
	{

	}

	public void SetEffectVolume(float volume)
	{
		MasterMixer.SetFloat("EffectVolume", volume);
		PlayerPrefs.SetFloat(effectVolumeKey, volume);
	}

	public void SetMusicVolume(float volume)
	{
		MasterMixer.SetFloat("MusicVolume", volume);
		PlayerPrefs.SetFloat(musicVolumeKey, volume);
	}

	public void Exit()
	{
		if (FromMainMenu)
		{
			MainMenuRef.gameObject.SetActive(true);
		}
		else
		{
			PauseMenuRef.gameObject.SetActive(true);
		}

		gameObject.SetActive(false);
	}
}
