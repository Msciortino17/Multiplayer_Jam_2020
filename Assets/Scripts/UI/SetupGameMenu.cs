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
	static SetupGameMenu reference;

	public bool FromOnlineSetup;
	public bool OnlineGameStarted;
	public bool Ready;

	public MainMenu MainMenuRef;
	public GameObject Players;
	public List<PlayerSetup> PlayerDataList;
	public List<Color> ColorOptions;
	public Dropdown TerrainType;
	public Button StartButton;
	public Text StatusText;

	private Terrain terrain;
	private GameManager manager;
	NetworkManager network;
	public Transform TankParent;
	public GameObject TankPrefab;

	public bool WaitingForOthersToStart;
	public int NumLoadedTanks;
	public float ReadyTimer;
	public bool CountingDown;
	public int ReadyCountdown = 3;

	public static SetupGameMenu GetReference()
	{
		if (reference == null)
		{
			reference = GameObject.Find("Canvas").transform.Find("Setup Game").GetComponent<SetupGameMenu>();
		}
		return reference;
	}

	void Awake()
	{
	}

	// Start is called before the first frame update
	void Start()
	{
		terrain = Terrain.GetReference();
		manager = GameManager.GetReference();
	}

	/// <summary>
	/// Resets all settings, values, etc to how they should be on a fresh game startup
	/// </summary>
	public void Clear()
	{
		if (FromOnlineSetup)
		{
			NumLoadedTanks = 0;
			WaitingForOthersToStart = false;
			PhotonNetwork.CurrentRoom.IsOpen = true;
			OnlineGameStarted = false;
			CountingDown = false;
			ReadyCountdown = 3;
			StartButton.interactable = true;
			StartButton.transform.Find("Text").GetComponent<Text>().text = "Start";
			StatusText.text = "";
			ReadyTimer = 1f;

			Hashtable hash = PhotonNetwork.CurrentRoom.CustomProperties;
			hash["GameStarted"] = OnlineGameStarted;
			PhotonNetwork.CurrentRoom.SetCustomProperties(hash);

			Hashtable hashPlayer = PhotonNetwork.LocalPlayer.CustomProperties;
			Ready = false;
			hashPlayer["Ready"] = Ready;
			PhotonNetwork.LocalPlayer.SetCustomProperties(hashPlayer);
		}
	}

	// Update is called once per frame
	void Update()
	{
		if (FromOnlineSetup)
		{
			TerrainType.interactable = PhotonNetwork.IsMasterClient;

			if (WaitingForOthersToStart && NumLoadedTanks == GetPlayerCount())
			{
				manager.Init(FromOnlineSetup);
				manager.UpdateWind();
				gameObject.SetActive(false);
			}

			ReadyTimer -= Time.deltaTime;
			if (ReadyTimer <= 0)
			{
				ReadyTimer = 1f;

				// Verify everyone's in the lobby
				bool allReady = true;
				foreach (Player player in PhotonNetwork.PlayerList)
				{
					if (!player.CustomProperties.ContainsKey("Ready") || !(bool)player.CustomProperties["Ready"])
					{
						allReady = false;
					}
				}

				if (allReady)
				{
					CountingDown = true;
					StartButton.interactable = false;
				}

				if (CountingDown)
				{
					StatusText.text = "Starting in " + ReadyCountdown + "...";
					if (ReadyCountdown == 0)
					{
						StartOnlineGame();
						RefreshUIWrite();
					}
					ReadyCountdown--;
				}
			}
		}
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
			RefreshUIRead();
		}
	}

	public void StartGameButton()
	{
		if (FromOnlineSetup)
		{
			int playerCount = GetPlayerCount();
			if (playerCount < 2)
			{
				StatusText.text = "Need at least 2 players in the lobby to begin.";
				return;
			}

			if (Ready)
			{
				StatusText.text = "";
				Hashtable hashPlayer = PhotonNetwork.LocalPlayer.CustomProperties;
				Ready = false;
				hashPlayer["Ready"] = Ready;
				PhotonNetwork.LocalPlayer.SetCustomProperties(hashPlayer);
				StartButton.transform.Find("Text").GetComponent<Text>().text = "Start";
			}
			else
			{
				Hashtable hashPlayer = PhotonNetwork.LocalPlayer.CustomProperties;
				Ready = true;
				hashPlayer["Ready"] = Ready;
				PhotonNetwork.LocalPlayer.SetCustomProperties(hashPlayer);
				StartButton.transform.Find("Text").GetComponent<Text>().text = "Cancel";

				for (int i = 0; i < PlayerDataList.Count; i++)
				{
					PlayerSetup player = PlayerDataList[i];
					if (player.MyPlayer == PhotonNetwork.LocalPlayer)
					{
						player.StatusText.text = ((bool)player.MyPlayer.CustomProperties["Ready"]) ? "Ready" : "Not Ready";
					}
				}
			}
		}
		else
		{
			StartLocalGame();
		}
	}

	public void StartOnlineGame()
	{
		OnlineGameStarted = true;
		PhotonNetwork.CurrentRoom.IsOpen = false;

		terrain.Init(TerrainType.value);

		int playerCount = GetPlayerCount();
		float offset = terrain.MapWidth / (playerCount + 1);
		int counter = 0;
		for (int i = 0; i < PlayerDataList.Count; i++)
		{
			PlayerSetup playerData = PlayerDataList[i];
			if (playerData.Active.activeInHierarchy)
			{
				if (playerData.OnlineNumber == PhotonNetwork.LocalPlayer.ActorNumber)
				{
					Vector3 position = new Vector3((counter + 1) * offset, 0f, 0f);
					Tank tank = PhotonNetwork.Instantiate(TankPrefab.name, position, Quaternion.identity).GetComponent<Tank>();
					tank.transform.parent = TankParent;
					tank.Init(playerData.GetName(), counter, playerData.GetColor(), Tank.ControlType.LocalPlayer);
					manager.PlayerTanks.Add(tank);
				}
				counter++;
			}
		}

		WaitingForOthersToStart = true;
		NumLoadedTanks++;
	}

	public void StartLocalGame()
	{
		int playerCount = GetPlayerCount();
		if (playerCount < 2)
		{
			return;
		}

		terrain.Init(TerrainType.value);
		manager.PlayerTanks = new List<Tank>();

		float offset = terrain.MapWidth / (playerCount + 1);
		int counter = 0;
		for (int i = 0; i < PlayerDataList.Count; i++)
		{
			PlayerSetup playerData = PlayerDataList[i];
			if (playerData.Active.activeInHierarchy)
			{
				Tank tank = Instantiate(TankPrefab, TankParent).GetComponent<Tank>(); // todo - init with photon
				tank.transform.position = new Vector3((counter + 1) * offset, 0f, 0f);
				Tank.ControlType controlType = playerData.GetControlType();
				tank.Init(playerData.GetName(), counter, playerData.GetColor(), controlType);
				manager.PlayerTanks.Add(tank);
				counter++;
			}
		}

		manager.Init(FromOnlineSetup);

		gameObject.SetActive(false);
	}

	public void Exit()
	{
		if (FromOnlineSetup)
		{
			Clear();
			PhotonNetwork.Disconnect();
			MainMenuRef.gameObject.SetActive(true);
		}
		MainMenuRef.gameObject.SetActive(true);
		manager.EndGame();
		ResetPlayerData();
		gameObject.SetActive(false);
	}

	public int GetPlayerCount()
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

	// todo - convert these to use player data
	public void RefreshUIWrite()
	{
		if (!FromOnlineSetup)
		{
			return;
		}

		Hashtable hash = PhotonNetwork.CurrentRoom.CustomProperties;
		for (int i = 0; i < PlayerDataList.Count; i++)
		{
			string prefix = "Player_" + i + "_";
			PlayerSetup player = PlayerDataList[i];
			hash[prefix + "Active"] = player.Active.activeInHierarchy;
			hash[prefix + "OnlineNumber"] = player.OnlineNumber;
			hash[prefix + "Nickname"] = player.GetName();
			hash[prefix + "Color"] = player.MyColor;
			hash[prefix + "ControlType"] = (int)player.GetControlType();
		}

		hash["TerrainType"] = TerrainType.value;
		hash["GameStarted"] = OnlineGameStarted;
		PhotonNetwork.CurrentRoom.SetCustomProperties(hash);
	}

	public void RefreshUIRead()
	{
		Hashtable hash = PhotonNetwork.CurrentRoom.CustomProperties;
		TerrainType.value = (int)hash["TerrainType"];

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
			if (player.MyPlayer == null)
			{
				foreach (Player p in PhotonNetwork.PlayerList)
				{
					if (p.ActorNumber == OnlineNumber)
					{
						player.MyPlayer = p;
					}
				}
			}
			player.SetName(Nickname);
			player.MyColor = color;
			player.MyColorDisplay.color = player.GetColor();
			player.SetControlType((Tank.ControlType)controlType);
			if (player.MyPlayer != null && player.MyPlayer.CustomProperties.ContainsKey("Ready"))
			{
				player.StatusText.text = ((bool)player.MyPlayer.CustomProperties["Ready"]) ? "Ready" : "Not Ready";
			}
		}
	}

	private void StartOnlineGameCheck(Hashtable hash)
	{
		if (!hash.ContainsKey("GameStarted"))
		{
			return;
		}

		if ((bool)hash["GameStarted"] && !OnlineGameStarted)
		{
			StartOnlineGame();
		}
	}

	public override void OnRoomPropertiesUpdate(Hashtable hash)
	{
		RefreshUIRead();
		StartOnlineGameCheck(hash);
	}

	public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
	{
		RefreshUIRead();
	}

	public override void OnPlayerEnteredRoom(Player newPlayer)
	{
		if (PhotonNetwork.IsMasterClient)
		{
			PlayerJoinedroom(newPlayer);
			RefreshUIWrite();
		}
	}

	public override void OnJoinedRoom()
	{
		PlayerJoinedroom(PhotonNetwork.LocalPlayer);
	}

	public override void OnPlayerLeftRoom(Player otherPlayer)
	{
		if (PhotonNetwork.IsMasterClient)
		{
			PlayerLeftRoom(otherPlayer);
			RefreshUIWrite();
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
				player.MyPlayer = newPlayer;
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
