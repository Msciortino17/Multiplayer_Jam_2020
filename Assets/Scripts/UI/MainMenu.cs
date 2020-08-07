using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
	// References
	public SetupGameMenu SetupGameMenuRef;
	public SetupOnlineMenu SetupOnlineRef;
	public OptionsMenu OptionsMenuRef;
	public GameObject CreditsMenu;
	public GameObject ExitButton;

	// Start is called before the first frame update
	void Start()
	{
		// Just need to turn off exit button if we're in the web build.
		if (Application.platform == RuntimePlatform.WebGLPlayer)
		{
			ExitButton.SetActive(false);
		}
	}

	// Update is called once per frame
	void Update()
	{

	}

	public void Play()
	{
		SetupGameMenuRef.FromOnlineSetup = false;
		SetupGameMenuRef.gameObject.SetActive(true);
		gameObject.SetActive(false);
	}

	public void PlayOnline()
	{
		SetupOnlineRef.gameObject.SetActive(true);
		SetupOnlineRef.Init();
		gameObject.SetActive(false);
	}

	public void Options()
	{
		OptionsMenuRef.FromMainMenu = true;
		OptionsMenuRef.gameObject.SetActive(true);
		gameObject.SetActive(false);
	}

	public void Credits()
	{
		CreditsMenu.SetActive(true);
		gameObject.SetActive(false);
	}

	public void Exit()
	{
		Application.Quit();
	}
}
