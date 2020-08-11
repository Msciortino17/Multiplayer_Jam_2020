using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class NetworkManager : MonoBehaviourPunCallbacks
{
	static NetworkManager reference;

	public bool IsHost;

	// Callbacks
	public delegate void NetworkCallback(string message);
	NetworkCallback OnConnectedToMasterCallback;

	public static NetworkManager GetReference()
	{
		if (reference == null)
		{
			reference = GameObject.Find("NetworkManager").GetComponent<NetworkManager>();
		}
		return reference;
	}

	void Awake()
	{
	}

	// Start is called before the first frame update
	void Start()
	{

	}

	// Update is called once per frame
	void Update()
	{

	}

	private void RoomSchema()
	{
		Hashtable schema = new Hashtable();

		schema.Add("WindDirection", 0);
		schema.Add("WindTurns", 0);
		schema.Add("TerrainSeed", 0f);

		for (int i = 0; i < 4; i++)
		{
			string prefix = "Player_" + i + "_";
			schema.Add(prefix + "Active", false);
			schema.Add(prefix + "OnlineNumber", -1);
			schema.Add(prefix + "Nickname", "Player " + (i + 1));
			schema.Add(prefix + "Color", 0);
			schema.Add(prefix + "ControlType", 4);
		}
		schema.Add("TerrainType", 0);
		schema.Add("GameStarted", false);

		PhotonNetwork.CurrentRoom.SetCustomProperties(schema);
	}

	public void PlayerSchema(Player player)
	{
		Hashtable schema = new Hashtable();

		schema.Add("Ready", false);
		schema.Add("Health", 100f);
		schema.Add("Fuel", 100f);

		player.SetCustomProperties(schema);
	}

	#region Main wrappers

	public void Connect()
	{
		PhotonNetwork.ConnectUsingSettings();
	}

	public void Disconnect()
	{
		PhotonNetwork.Disconnect();
	}

	public void JoinRandomRoom()
	{
		PhotonNetwork.JoinRandomRoom();
	}

	public void JoinNamedRoom(string name)
	{
		PhotonNetwork.JoinRoom(name);
	}

	public void CreateRoom(string name, RoomOptions options)
	{
		PhotonNetwork.JoinOrCreateRoom(name, options, TypedLobby.Default);
	}

	public bool IsMine()
	{
		return photonView.IsMine;
	}

	#endregion

	#region Photon overrides

	public override void OnConnectedToMaster()
	{
		Debug.Log("Connected to master...");
	}

	public override void OnDisconnected(DisconnectCause cause)
	{
		Debug.Log("Disconnected from server. Reason: " + cause.ToString());
	}

	public override void OnCreatedRoom()
	{
		RoomSchema();
		PlayerSchema(PhotonNetwork.MasterClient);
	}

	public override void OnPlayerEnteredRoom(Player newPlayer)
	{
		PlayerSchema(newPlayer);
	}

	public override void OnJoinedRoom()
	{
		Debug.Log("Successfully joined room...");
	}

	public override void OnJoinRoomFailed(short returnCode, string message)
	{
		Debug.Log("Failed to join the given room. Reason: " + message);
	}

	public override void OnJoinRandomFailed(short returnCode, string message)
	{
		Debug.Log("Failed to join a random room. Reason: " + message);
	}

	public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
	{
		GameManager manager = GameManager.GetReference();
		if (manager.Started)
		{
			manager.UpdateWind();
		}
	}

	#endregion
}
