using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class OptionsMenu : MonoBehaviour
{
	public bool FromMainMenu;

	// References
	public MainMenu MainMenuRef;
	public PauseMenu PauseMenuRef;
	public Slider MusicSlider;
	public Slider EffectSlider;
	public AudioMixer MasterMixer;

	// Start is called before the first frame update
	void Start()
	{
		float effectVolume = 1f;
		float musicVolume = 1f;
		MasterMixer.GetFloat("EffectVolume", out effectVolume);
		MasterMixer.GetFloat("MusicVolume", out musicVolume);
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
	}

	public void SetMusicVolume(float volume)
	{
		MasterMixer.SetFloat("MusicVolume", volume);
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
