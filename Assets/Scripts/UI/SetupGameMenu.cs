using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetupGameMenu : MonoBehaviour
{
	public MainMenu MainMenuRef;
	public GameObject Players;
	public List<PlayerSetup> PlayerDataList;
	public List<Color> ColorOptions;

	private Terrain terrain;
	private GameManager manager;
	public Transform TankParent;
	public GameObject TankPrefab;

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

	public void AddPlayer()
	{
		if (PlayerDataList.Count < 4)
		{
			PlayerSetup player = Instantiate(PlayerDataList[PlayerDataList.Count - 1].gameObject, Players.transform).GetComponent<PlayerSetup>();
			PlayerDataList.Add(player);
			player.NameField.text = "Player " + PlayerDataList.Count;
			player.NextColor();
		}
	}

	public void StartGame()
	{
		if (PlayerDataList.Count == 1)
		{
			return;
		}

		terrain.Init();
		manager.Players = new List<Tank>();

		float offset = terrain.MapWidth / (PlayerDataList.Count + 1);
		for (int i = 0; i < PlayerDataList.Count; i++)
		{
			PlayerSetup playerData = PlayerDataList[i];
			Tank tank = Instantiate(TankPrefab, TankParent).GetComponent<Tank>();
			tank.transform.position = new Vector3((i + 1) * offset, 0f, 0f);
			tank.Init(playerData.GetName(), i, playerData.GetColor());
			manager.Players.Add(tank);
		}

		manager.Init();

		gameObject.SetActive(false);
	}

	public void Exit()
	{
		MainMenuRef.gameObject.SetActive(true);
		gameObject.SetActive(false);
	}
}
