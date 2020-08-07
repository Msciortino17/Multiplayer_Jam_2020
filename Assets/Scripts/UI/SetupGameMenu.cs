using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.IO;
using System;
using System.Text;
using ExitGames.Client.Photon;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class SetupGameMenu : MonoBehaviourPunCallbacks
{
	public bool FromOnlineSetup;

	public MainMenu MainMenuRef;
	public GameObject Players;
	public GameObject OnlinePlayerDataPrefab;
	public List<PlayerSetup> PlayerDataList;
	public List<Color> ColorOptions;

	private Terrain terrain;
	private GameManager manager;
	NetworkManager network;
	public Transform TankParent;
	public GameObject TankPrefab;

	void Awake()
	{
	}

	// Start is called before the first frame update
	void Start()
	{
		terrain = Terrain.GetReference();
		manager = GameManager.GetReference();
	}

	// Update is called once per frame
	void Update()
	{
	}

	public void InitNetwork()
	{
		network = NetworkManager.GetReference();

		if (PhotonNetwork.IsMasterClient)
		{
			RefreshUIHost();
		}
		else
		{
			RefreshUIClient(PhotonNetwork.CurrentRoom.CustomProperties);
		}
	}

	public void StartGame()
	{
		int playerCount = GetPlayerCount();
		Debug.Log("count: " + playerCount);
		if (playerCount < 2)
		{
			return;
		}

		terrain.Init();
		manager.Players = new List<Tank>();

		float offset = terrain.MapWidth / 5;
		for (int i = 0; i < PlayerDataList.Count; i++)
		{
			PlayerSetup playerData = PlayerDataList[i];
			if (playerData.Active.activeInHierarchy)
			{
				Tank tank = Instantiate(TankPrefab, TankParent).GetComponent<Tank>(); // todo - init with photon
				tank.transform.position = new Vector3((i + 1) * offset, 0f, 0f);
				Tank.ControlType controlType = playerData.GetControlType();
				tank.Init(playerData.GetName(), i, playerData.GetColor(), controlType);
				manager.Players.Add(tank);
			}
		}

		manager.Init();

		gameObject.SetActive(false);
	}

	public void Exit()
	{
		if (FromOnlineSetup)
		{
			PhotonNetwork.Disconnect();
			MainMenuRef.gameObject.SetActive(true);
		}
		else
		{
			MainMenuRef.gameObject.SetActive(true);
		}
		ResetPlayerData();
		gameObject.SetActive(false);
	}

	private int GetPlayerCount()
	{
		int activeCount = 0;
		foreach (PlayerSetup player in PlayerDataList)
		{
			if (player.Active.activeInHierarchy)
			{
				activeCount++;
			}
		}
		return activeCount;
	}

	public void DebugNetwork()
	{
		string message = "There are currently " + PhotonNetwork.CurrentRoom.PlayerCount + " players in this room.\n";
		message += "Your name is " + PhotonNetwork.NickName + "\n";
		if (network.IsHost)
		{
			message += "And you are the host.";
		}
		DebugText.SetText(message);
	}

	public override void OnPlayerEnteredRoom(Player newPlayer)
	{
		Debug.Log("someone else joined room");
		// Send update to everyone else here
		/**
		 * For each PlayerSetup:
		 * -If it's active
		 * -The online number
		 * -The nick name
		 * -The color
		 * -The control type
		 */
		if (PhotonNetwork.IsMasterClient)
		{
			PlayerJoinedroom(newPlayer);
			RefreshUIHost();
			//PlayerSetupInfo[] playerInfo = new PlayerSetupInfo[4];
			//for (int i = 0; i < PlayerDataList.Count; i++)
			//{
			//	PlayerSetup player = PlayerDataList[i];
			//	playerInfo[i].IsActive = player.Active.activeInHierarchy;
			//	playerInfo[i].OnlineNumber = player.OnlineNumber;
			//	playerInfo[i].NickNameSize = player.GetName().Length;
			//	playerInfo[i].NickName = player.GetName();
			//	playerInfo[i].Color = player.MyColor;
			//	playerInfo[i].ControlType = (int)player.GetControlType();
			//}
			//photonView.RPC("RefreshUIData", RpcTarget.Others, playerInfo);
		}
	}

	private void RefreshUIHost()
	{
		Hashtable hash = new Hashtable();
		for (int i = 0; i < PlayerDataList.Count; i++)
		{
			string prefix = "Player_" + i + "_";
			PlayerSetup player = PlayerDataList[i];
			hash.Add(prefix + "Active", player.Active.activeInHierarchy);
			hash.Add(prefix + "OnlineNumber", player.OnlineNumber);
			hash.Add(prefix + "Nickname", player.GetName());
			hash.Add(prefix + "Color", player.MyColor);
			hash.Add(prefix + "ControlType", (int)player.GetControlType());
		}
		PhotonNetwork.CurrentRoom.SetCustomProperties(hash);
	}

	public void RefreshUIClient(Hashtable hash)
	{
		for (int i = 0; i < PlayerDataList.Count; i++)
		{
			string prefix = "Player_" + i + "_";
			PlayerSetup player = PlayerDataList[i];
			bool Active = (bool)hash[prefix + "Active"];
			int OnlineNumber = (int)hash[prefix + "OnlineNumber"];
			string Nickname = (string)hash[prefix + "Nickname"];
			int color = (int)hash[prefix + "Color"];
			int controlType = (int)hash[prefix + "ControlType"];

			if (Active)
			{
				player.TurnOn();
			}
			else
			{
				player.TurnOff();
			}

			player.OnlineNumber = OnlineNumber;
			player.SetName(Nickname);
			player.MyColor = color;
			player.MyColorDisplay.color = player.GetColor();
			player.SetControlType((Tank.ControlType)controlType);
		}
	}

	public override void OnRoomPropertiesUpdate(Hashtable hash)
	{
		RefreshUIClient(hash);
	}

	public void PlayerJoinedroom(Player newPlayer)
	{
		for (int i = 0; i < PlayerDataList.Count; i++)
		{
			PlayerSetup player = PlayerDataList[i];
			if (!player.Active.activeInHierarchy && player.OnlineNumber == -1)
			{
				player.OnlineNumber = newPlayer.ActorNumber;
				player.SetName(newPlayer.NickName);
				player.AddPlayer();
				return;
			}
		}
	}

	public void ResetPlayerData()
	{
		for (int i = 0; i < PlayerDataList.Count; i++)
		{
			PlayerSetup player = PlayerDataList[i];
			player.OnlineNumber = -1;
			player.TurnOff();
		}
	}
}
