using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreditsMenu : MonoBehaviour
{
	public MainMenu MainMenuRef;

	// Start is called before the first frame update
	void Start()
	{

	}

	// Update is called once per frame
	void Update()
	{

	}

	public void Exit()
	{
		MainMenuRef.gameObject.SetActive(true);
		gameObject.SetActive(false);
	}
}
