using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class SetupOnlineMenu : MonoBehaviourPunCallbacks
{
	private bool joinedRoom;
	private float joinedTimer;
	private bool connectedToMaster;
	private const string onlineNameKey = "OnlineName";
	private const string onlineRoomKey = "OnlineRoom";

	// References
	public MainMenu MainMenuRef;
	public SetupGameMenu SetupGameRef;
	NetworkManager manager;
	public Text StatusText;
	public InputField RoomName;
	public InputField PlayerName;
	public Button JoinGameButton;
	public Button HostGameButton;

	// Start is called before the first frame update
	void Awake()
	{
	}

	// Update is called once per frame
	void Update()
	{
		if (joinedRoom)
		{
			joinedTimer -= Time.deltaTime;
			if (joinedTimer < 0f)
			{
				SetupGameRef.FromOnlineSetup = true;
				SetupGameRef.gameObject.SetActive(true);
				SetupGameRef.InitNetwork();
				gameObject.SetActive(false);
			}
		}
		else
		{
			if (connectedToMaster && !string.IsNullOrEmpty(PlayerName.text))
			{
				JoinGameButton.interactable = true;
				HostGameButton.interactable = true;
			}
		}
	}

	public void Init()
	{
		manager = NetworkManager.GetReference();
		manager.Connect();

		joinedRoom = false;
		joinedTimer = 0f;
		StatusText.text = "Connecting to master server...";

		if (PlayerPrefs.HasKey(onlineNameKey))
		{
			string name = PlayerPrefs.GetString(onlineNameKey);
			PlayerName.text = name;
			PhotonNetwork.NickName = name;
		}

		if (PlayerPrefs.HasKey(onlineRoomKey))
		{
			RoomName.text = PlayerPrefs.GetString(onlineRoomKey);
		}

		connectedToMaster = false;
		JoinGameButton.interactable = false;
		HostGameButton.interactable = false;
	}

	public void JoinRoom()
	{
		JoinGameButton.interactable = false;
		HostGameButton.interactable = false;

		string roomName = RoomName.text;
		if (string.IsNullOrEmpty(roomName))
		{
			manager.JoinRandomRoom();
			StatusText.text = "Attempting to join a random game...";
		}
		else
		{
			manager.JoinNamedRoom(roomName);
			StatusText.text = "Attempting to join " + roomName + "...";
		}
	}

	public void HostRoom()
	{
		JoinGameButton.interactable = false;
		HostGameButton.interactable = false;

		RoomOptions roomOptions = new RoomOptions();
		roomOptions.MaxPlayers = 4;

		string roomName = RoomName.text;
		if (string.IsNullOrEmpty(roomName))
		{
			roomOptions.IsVisible = true;
			roomName = System.Guid.NewGuid().ToString();
		}
		else
		{
			roomOptions.IsVisible = false;
		}

		manager.CreateRoom(roomName, roomOptions);
		StatusText.text = "Attempting to create a room...";
	}

	public void Exit()
	{
		manager.Disconnect();
		MainMenuRef.gameObject.SetActive(true);
		gameObject.SetActive(false);
	}

	public void SaveName()
	{
		PlayerPrefs.SetString(onlineNameKey, PlayerName.text);
		PhotonNetwork.NickName = PlayerName.text;
	}

	public void SaveRoomName()
	{
		PlayerPrefs.SetString(onlineRoomKey, RoomName.text);
	}

	public override void OnConnectedToMaster()
	{
		connectedToMaster = true;
		StatusText.text = "Successfully connected to master server, ready to join or host a game.";
	}

	public override void OnJoinedRoom()
	{
		joinedRoom = true;
		joinedTimer = 1f;
		StatusText.text = "Joining room now...";
		if (PhotonNetwork.IsMasterClient)
		{
			SetupGameRef.PlayerJoinedroom(PhotonNetwork.LocalPlayer);
		}
	}

	public override void OnCreatedRoom()
	{
		joinedRoom = true;
		joinedTimer = 1f;
		manager.IsHost = true;
		StatusText.text = "Success, joining room now...";
	}

	public override void OnCreateRoomFailed(short returnCode, string message)
	{
		JoinGameButton.interactable = true;
		HostGameButton.interactable = true;
		StatusText.text = "Could not create a room.\n" + message;
	}

	public override void OnJoinRoomFailed(short returnCode, string message)
	{
		JoinGameButton.interactable = true;
		HostGameButton.interactable = true;
		StatusText.text = "Could not join the given room.\n" + message;
	}

	public override void OnJoinRandomFailed(short returnCode, string message)
	{
		JoinGameButton.interactable = true;
		HostGameButton.interactable = true;
		StatusText.text = "Could not join a random room.\n" + message;
	}
}
