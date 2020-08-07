﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

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

	#endregion
}