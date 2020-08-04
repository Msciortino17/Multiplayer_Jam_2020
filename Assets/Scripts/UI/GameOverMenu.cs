using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameOverMenu : MonoBehaviour
{
	public MainMenu MainMenuRef;
	public SetupGameMenu SetupGameRef;

	// Start is called before the first frame update
	void Start()
	{

	}

	// Update is called once per frame
	void Update()
	{

	}

	public void Continue()
	{
		SetupGameRef.gameObject.SetActive(true);
		gameObject.SetActive(false);
	}

	public void Exit()
	{
		MainMenuRef.gameObject.SetActive(true);
		gameObject.SetActive(false);
	}
}
