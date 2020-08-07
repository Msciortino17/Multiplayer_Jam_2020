using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

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
		if (SetupGameRef.FromOnlineSetup)
		{
			Hashtable hashPlayer = PhotonNetwork.LocalPlayer.CustomProperties;
			hashPlayer["InLobby"] = true;
			PhotonNetwork.LocalPlayer.SetCustomProperties(hashPlayer);
		}

		SetupGameRef.gameObject.SetActive(true);
		gameObject.SetActive(false);
	}

	public void Exit()
	{
		SetupGameRef.Exit();
		MainMenuRef.gameObject.SetActive(true);
		gameObject.SetActive(false);
	}
}
