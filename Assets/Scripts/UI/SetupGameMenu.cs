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

	public MainMenu MainMenuRef;
	public GameObject Players;
	public List<PlayerSetup> PlayerDataList;
	public List<Color> ColorOptions;
	public Dropdown TerrainType;
	public Button StartButton;

	private Terrain terrain;
	private GameManager manager;
	NetworkManager network;
	public Transform TankParent;
	public GameObject TankPrefab;

	public bool WaitingForOthersToStart;
	public int NumLoadedTanks;

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
		StartButton.interactable = !(FromOnlineSetup && !PhotonNetwork.IsMasterClient);

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

			Hashtable hash = PhotonNetwork.CurrentRoom.CustomProperties;
			hash["GameStarted"] = OnlineGameStarted;
			PhotonNetwork.CurrentRoom.SetCustomProperties(hash);

			Hashtable hashPlayer = PhotonNetwork.LocalPlayer.CustomProperties;
			hashPlayer["InLobby"] = false;
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
				//for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
				//{
				//	Player player = PhotonNetwork.PlayerList[i];
				//	if (!(bool)player.CustomProperties["Ready"])
				//	{
				//		return;
				//	}
				//}

				manager.Init(FromOnlineSetup);
				gameObject.SetActive(false);
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
		if (FromOnlineSetup && !PhotonNetwork.IsMasterClient)
		{
			return;
		}

		if (FromOnlineSetup)
		{
			// Verify everyone's in the lobby
			bool ready = true;
			foreach (Player player in PhotonNetwork.PlayerList)
			{
				if (!(bool)player.CustomProperties["InLobby"])
				{
					ready = false;
				}
			}

			if (ready)
			{
				StartOnlineGame();
				RefreshUIWrite();
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

		int playerCount = GetPlayerCount();
		if (playerCount < 2)
		{
			return;
		}

		terrain.Init();

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
					Debug.Log("hello?");
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

		terrain.Init();
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
		else
		{
			MainMenuRef.gameObject.SetActive(true);
		}
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
			player.SetName(Nickname);
			player.MyColor = color;
			player.MyColorDisplay.color = player.GetColor();
			player.SetControlType((Tank.ControlType)controlType);
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

	public override void OnPlayerEnteredRoom(Player newPlayer)
	{
		if (PhotonNetwork.IsMasterClient)
		{
			PlayerJoinedroom(newPlayer);
			RefreshUIWrite();
		}
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
