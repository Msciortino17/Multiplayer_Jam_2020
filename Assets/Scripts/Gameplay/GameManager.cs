using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
	static GameManager reference;

	public GameOverMenu GameOverRef;
	public Text GameOverText;

	public List<Tank> Players;
	public int CurrentPlayer;

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

	}

	public void Init()
	{
		CurrentPlayer = 0;
		GetCurrentPlayer().MyTurn = true;
	}

	private int CheckWinner()
	{
		// Tally up the dead to see if there's one left
		int deathCheck = 0;
		for (int i = 0; i < Players.Count; i++)
		{
			if (Players[i].Health <= 0f)
			{
				deathCheck++;
			}
		}

		if (deathCheck == Players.Count - 1)
		{
			// Sift through to see who has any health
			for (int i = 0; i < Players.Count; i++)
			{
				if (Players[i].Health > 0f)
				{
					return i;
				}
			}
		}

		return -1;

	}

	public Tank GetCurrentPlayer()
	{
		return Players[CurrentPlayer];
	}

	public void BulletLanded()
	{
		int winner = CheckWinner();
		if (winner == -1)
		{
			GetCurrentPlayer().StartTurn();
		}
		else
		{
			GameOverRef.gameObject.SetActive(true);
			GameOverText.text = "Game Over\n\nThe winner is " + Players[winner].TankName;

			// Clear out players
			foreach (var player in Players)
			{
				Destroy(player.gameObject);
			}
			Players.Clear();
		}
	}

	/// <summary>
	/// Passes control to the next player and checks for the end of the turn.
	/// </summary>
	public void NextPlayer()
	{
		GetCurrentPlayer().MyTurn = false;
		CurrentPlayer++;
		if (CurrentPlayer == Players.Count)
		{
			CurrentPlayer = 0;
			NextTurn();
		}
	}

	/// <summary>
	/// Triggered when the last player finishes, completing a full game turn. 
	/// Anything that should happen at that point should be put here.
	/// </summary>
	private void NextTurn()
	{

	}
}
