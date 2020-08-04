using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{
	public bool FromMainMenu;

	// References
	public MainMenu MainMenuRef;
	public ControlsMenu ControlsMenuRef;
	public PauseMenu PauseMenuRef;
	public Slider MusicSlider;
	public Slider EffectSlider;

	// Start is called before the first frame update
	void Start()
	{

	}

	// Update is called once per frame
	void Update()
	{

	}

	public float GetMusicVolume()
	{
		return MusicSlider.value;
	}

	public float GetSoundEffectVolume()
	{
		return EffectSlider.value;
	}

	public void Controls()
	{
		ControlsMenuRef.gameObject.SetActive(true);
		gameObject.SetActive(false);
	}

	public void Exit()
	{
		if (FromMainMenu)
		{
			MainMenuRef.gameObject.SetActive(true);
		}
		else
		{
			ControlsMenuRef.gameObject.SetActive(true);
		}

		gameObject.SetActive(false);
	}
}
