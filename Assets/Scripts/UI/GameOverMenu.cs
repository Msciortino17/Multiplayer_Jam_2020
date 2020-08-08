using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class GameOverMenu : MonoBehaviourPunCallbacks
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
			hashPlayer["Ready"] = false;
			PhotonNetwork.LocalPlayer.SetCustomProperties(hashPlayer);
			SetupGameRef.RefreshUIRead();
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

	public override void OnPlayerLeftRoom(Player otherPlayer)
	{
		if (PhotonNetwork.IsMasterClient)
		{
			SetupGameRef.PlayerLeftRoom(otherPlayer);
			SetupGameRef.RefreshUIWrite();
		}
	}
}
