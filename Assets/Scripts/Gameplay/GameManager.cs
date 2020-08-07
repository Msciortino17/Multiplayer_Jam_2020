using ExitGames.Client.Photon;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class GameManager : MonoBehaviourPunCallbacks
{
	static GameManager reference;

	public bool OnlineGame;
	public bool Started;

	public Sandstorm SandstormRef;
	public GameOverMenu GameOverRef;
	public Text GameOverText;

	public List<Tank> PlayerTanks;
	public int CurrentPlayer;

	public int WindTurns;
	public int WindDirection;
	public float WindPower;

	public static GameManager GetReference()
	{
		if (reference == null)
		{
			reference = GameObject.Find("GameManager").GetComponent<GameManager>();
		}
		return reference;
	}

	// Start is called before the first frame update
	void Start()
	{
	}

	// Update is called once per frame
	void Update()
	{
		if (Started)
		{
			string message = "Current player: " + CurrentPlayer;
			message += "\n" + GetCurrentPlayer().TankName + " : " + GetCurrentPlayer().OnlineNumber;
			DebugText.SetText(message);
		}
	}

	public void Init(bool online)
	{
		Started = true;
		OnlineGame = online;
		CurrentPlayer = 0;
		GetCurrentPlayer().StartTurn();

		string message = "Account nums:";
		for (int i = 0; i < PlayerTanks.Count; i++)
		{
			message += "\nindex: " + i + ", Account num: " + PlayerTanks[i].OnlineNumber;
		}
		Debug.Log(message);
	}

	private int CheckWinner()
	{
		// Tally up the dead to see if there's one left
		int deathCheck = 0;
		for (int i = 0; i < PlayerTanks.Count; i++)
		{
			if (PlayerTanks[i].Health <= 0f)
			{
				deathCheck++;
			}
		}

		if (deathCheck == PlayerTanks.Count - 1)
		{
			// Sift through to see who has any health
			for (int i = 0; i < PlayerTanks.Count; i++)
			{
				if (PlayerTanks[i].Health > 0f)
				{
					return i;
				}
			}
		}

		return -1;

	}

	public Vector3 GetWind()
	{
		return new Vector3(WindDirection * WindPower, 0f, 0f);
	}

	public Tank GetCurrentPlayer()
	{
		if (OnlineGame)
		{
			for (int i = 0; i < PlayerTanks.Count; i++)
			{
				//Debug.Log("index: " + i + ", online num: " + PlayerTanks[i].OnlineNumber);
				if (PlayerTanks[i].OnlineNumber - 1 == CurrentPlayer)
				{
					return PlayerTanks[i];
				}
			}
		}
		//Debug.Log("hmm");
		return PlayerTanks[CurrentPlayer];
	}

	public void BulletLanded()
	{
		CurrentPlayer++;
		if (CurrentPlayer == PlayerTanks.Count)
		{
			CurrentPlayer = 0;
			NextTurn();
		}

		int winner = CheckWinner();
		if (winner == -1)
		{
			GetCurrentPlayer().StartTurn();
		}
		else
		{
			GameOverRef.gameObject.SetActive(true);
			GameOverText.text = "Game Over\n\nThe winner is " + PlayerTanks[winner].TankName;

			// Clear out players
			foreach (var player in PlayerTanks)
			{
				Destroy(player.gameObject);
			}
			PlayerTanks.Clear();

			Started = false;
		}
	}

	/// <summary>
	/// Passes control to the next player and checks for the end of the turn.
	/// </summary>
	public void NextPlayer()
	{
		GetCurrentPlayer().MyTurn = false;
	}

	/// <summary>
	/// Triggered when the last player finishes, completing a full game turn. 
	/// Anything that should happen at that point should be put here.
	/// </summary>
	private void NextTurn()
	{
		WindTurns--;
		if (WindTurns <= 0)
		{
			if (!OnlineGame || PhotonNetwork.IsMasterClient)
			{
				WindDirection = Random.Range(-1, 2);
				WindTurns = Random.Range(1, 2);
				SandstormRef.UpdateWind(WindDirection);
			}

			if (OnlineGame)
			{
				Hashtable hash = PhotonNetwork.CurrentRoom.CustomProperties;
				hash["WindDirection"] = WindDirection;
				hash["WindTurns"] = WindTurns;
				PhotonNetwork.CurrentRoom.SetCustomProperties(hash);
			}
		}
	}

	public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
	{
		Hashtable hash = PhotonNetwork.CurrentRoom.CustomProperties;
		WindDirection = (int)hash["WindDirection"];
		WindTurns = (int)hash["WindTurns"];
		SandstormRef.UpdateWind(WindDirection);
	}
}
