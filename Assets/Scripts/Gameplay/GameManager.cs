using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class GameManager : MonoBehaviourPunCallbacks
{
	static GameManager reference;
	public SetupGameMenu SetupGameRef;
	public PauseMenu PauseMenuRef;

	public bool OnlineGame;
	public bool Started;
	public bool Paused;
	public int Winner;

	public Sandstorm SandstormRef;
	public GameOverMenu GameOverRef;
	public Text GameOverText;

	public List<Tank> PlayerTanks;
	public int CurrentPlayer;

	public int WindTurns;
	public int WindDirection;
	public float WindPower;

	// Transitions
	private bool WaitingToNextPlayer;
	private bool WaitingToEndGame;
	private float NextPlayerTimer;
	private float EndGameTimer;

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

		if (Input.GetKeyDown(KeyCode.Escape))
		{
			if (Paused)
			{
				Paused = false;
				PauseMenuRef.Unpause();
			}
			else
			{
				Paused = true;
				PauseMenuRef.gameObject.SetActive(true);
			}
		}

		if (WaitingToNextPlayer)
		{
			NextPlayerTimer -= Time.deltaTime;
			if (NextPlayerTimer <= 0f)
			{
				WaitingToNextPlayer = false;

				CurrentPlayer++;
				if (CurrentPlayer >= PlayerTanks.Count)
				{
					CurrentPlayer = 0;
					NextTurn();
				}

				Winner = CheckWinner();
				if (Winner == -1)
				{
					GetCurrentPlayer().StartTurn();
				}
				else
				{
					WaitingToEndGame = true;
					EndGameTimer = 3f;
					DarkOverlay.GetReference().SetDarkness(1);
					DebugText.SetText("Game Over!");
				}
			}
		}

		if (WaitingToEndGame)
		{
			EndGameTimer -= Time.deltaTime;
			if (EndGameTimer <= 0f)
			{
				WaitingToEndGame = false;

				GameOverRef.gameObject.SetActive(true);
				GameOverText.text = "Game Over\n\nThe winner is " + PlayerTanks[Winner].TankName;
				EndGame();
			}
		}
	}

	public void Init(bool online)
	{
		Started = true;
		Paused = false;
		WaitingToEndGame = false;
		WaitingToNextPlayer = false;
		OnlineGame = online;
		CurrentPlayer = 0;
		GetCurrentPlayer().StartTurn();
		Winner = -1;
		MusicManager.GetReference().PlayBattleMusic();

		if (!OnlineGame || PhotonNetwork.IsMasterClient)
		{
			int windDirection = Random.Range(-1, 2);
			int windTurns = Random.Range(1, 2);
			SetWind(windDirection, windTurns);
		}

		//string message = "Account nums:";
		//for (int i = 0; i < PlayerTanks.Count; i++)
		//{
		//	message += "\nindex: " + i + ", Account num: " + PlayerTanks[i].OnlineNumber;
		//}
		//Debug.Log(message);
	}

	private int CheckWinner()
	{
		// If there's only one player left, they win
		if (PlayerTanks.Count == 1)
		{
			return 0;
		}

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
		WaitingToNextPlayer = true;
		NextPlayerTimer = 0.5f;
	}

	public void EndGame()
	{
		SetWind(0, 0);
		SandstormRef.Stop();

		ClearPlayerObjects();

		Started = false;
		SetupGameRef.Clear();

		WaitingToEndGame = false;
		WaitingToNextPlayer = false;
		Terrain.GetReference().initialized = false;
		MusicManager.GetReference().PlayMenuMusic();
	}

	public void ClearPlayerObjects()
	{
		foreach (var player in PlayerTanks)
		{
			if (OnlineGame)
			{
				if (player.OnlineNumber == PhotonNetwork.LocalPlayer.ActorNumber)
				{
					PhotonNetwork.Destroy(player.gameObject);
				}
			}
			else
			{
				Destroy(player.gameObject);
			}
		}
		PlayerTanks.Clear();
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
			if (OnlineGame && !PhotonNetwork.IsMasterClient)
			{
				return;
			}

			int windDirection = Random.Range(-1, 2);
			int windTurns = Random.Range(1, 2);
			SetWind(windDirection, windTurns);
		}
	}

	public void SetWind(int direction, int turns)
	{
		if (!OnlineGame || PhotonNetwork.IsMasterClient)
		{
			WindDirection = direction;
			WindTurns = turns;
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

	public void UpdateWind()
	{
		if (Started)
		{
			Hashtable hash = PhotonNetwork.CurrentRoom.CustomProperties;
			WindDirection = (int)hash["WindDirection"];
			WindTurns = (int)hash["WindTurns"];
			SandstormRef.UpdateWind(WindDirection);
		}
	}
	
	public override void OnPlayerLeftRoom(Player otherPlayer)
	{
		if (Started)
		{
			// Remove the missing tank
			Tank currentTank = GetCurrentPlayer();
			Tank leavingTank = null;
			for (int i = 0; i < PlayerTanks.Count; i++)
			{
				if (otherPlayer.ActorNumber == PlayerTanks[i].OnlineNumber)
				{
					leavingTank = PlayerTanks[i];
					PlayerTanks.RemoveAt(i);
					break;
				}
			}

			if (leavingTank != null)
			{
				// If it's that tanks turn or there's only one player left, increment to next turn
				if (leavingTank == currentTank || PlayerTanks.Count == 1)
				{
					WaitingToNextPlayer = true;
					NextPlayerTimer = 0.5f;
				}

				// Spawn a dead tank on the leaving tank's body
				// to-do
			}
		}
	}
}
