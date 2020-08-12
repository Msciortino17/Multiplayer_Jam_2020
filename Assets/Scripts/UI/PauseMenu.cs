using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
	public MainMenu MainMenuRef;
	public OptionsMenu OptionsMenuRef;
	public SetupGameMenu SetupGameRef;

	// Start is called before the first frame update
	void Start()
	{

	}

	// Update is called once per frame
	void Update()
	{

	}

	public void Options()
	{
		OptionsMenuRef.FromMainMenu = false;
		OptionsMenuRef.gameObject.SetActive(true);
		gameObject.SetActive(false);
	}

	public void Unpause()
	{
		GameManager.GetReference().Paused = false;
		gameObject.SetActive(false);
	}

	public void Exit()
	{
		SetupGameRef.Exit();
		MainMenuRef.gameObject.SetActive(true);
		gameObject.SetActive(false);
		DarkOverlay.GetReference().SetDarkness(0f);
	}
}
