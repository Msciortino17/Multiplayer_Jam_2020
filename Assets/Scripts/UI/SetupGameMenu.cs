using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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
	public Dropdown TerrainType;

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
		TerrainType.interactable = PhotonNetwork.IsMasterClient;
	}

	public void InitNetwork()
	{
		network = NetworkManager.GetReference();

		if (PhotonNetwork.IsMasterClient)
		{
			RefreshUIWrite();
		}
		else
		{
			RefreshUIRead(PhotonNetwork.CurrentRoom.CustomProperties);
		}
	}

	public void StartGame()
	{
		if (!PhotonNetwork.IsMasterClient)
		{
			return;
		}

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
		if (PhotonNetwork.IsMasterClient)
		{
			PlayerJoinedroom(newPlayer);
			RefreshUIWrite();
		}
	}

	public void RefreshUIWrite()
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
		hash.Add("TerrainType", TerrainType.value);
		PhotonNetwork.CurrentRoom.SetCustomProperties(hash);
	}

	public void RefreshUIRead(Hashtable hash)
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
		TerrainType.value = (int)hash["TerrainType"];
	}

	public override void OnRoomPropertiesUpdate(Hashtable hash)
	{
		RefreshUIRead(hash);
	}

	public override void OnPlayerLeftRoom(Player otherPlayer)
	{
		if (PhotonNetwork.IsMasterClient)
		{
			PlayerLeftRoom(otherPlayer);
		}
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

	public void PlayerLeftRoom(Player leavingPlayer)
	{
		for (int i = 0; i < PlayerDataList.Count; i++)
		{
			PlayerSetup player = PlayerDataList[i];
			if (player.Active.activeInHierarchy && player.OnlineNumber == leavingPlayer.ActorNumber)
			{
				player.TurnOff();
				player.OnlineNumber = -1;
				RefreshUIWrite();
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
