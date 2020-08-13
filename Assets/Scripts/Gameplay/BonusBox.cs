using ExitGames.Client.Photon;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class BonusBox : MonoBehaviourPunCallbacks
{
	GameManager manager;
	public Transform Background;
	public Text MyText;
	public ParticleSystem MyParticles;
	public SphereCollider MyCollider;
	public bool Fading;
	public float FadeTimer;
	public int NextBullet;

	// Start is called before the first frame update
	void Start()
	{
		manager = GameManager.GetReference();
	}

	// Update is called once per frame
	void Update()
	{
		Background.transform.Rotate(0f, 0f, 180f * Time.deltaTime);

		if (Fading)
		{
			FadeTimer -= Time.deltaTime;
			if (FadeTimer <= 0f)
			{
				Fading = false;
			}

			Color textColor = MyText.color;
			textColor.a = FadeTimer;
			MyText.color = textColor;
		}
	}

	public void Spawn(float minHeight)
	{
		float x = Random.Range(1f, Terrain.GetReference().MapWidth - 1f);
		float y = Random.Range(minHeight, 28f);

		int bulletType = Random.Range(1, manager.BulletPrefabs.Count);
		if (manager.OnlineGame)
		{
			if (PhotonNetwork.IsMasterClient)
			{
				transform.position = new Vector3(x, y, 0f);
				Hashtable hash = PhotonNetwork.CurrentRoom.CustomProperties;
				hash["BonusBox_X"] = x;
				hash["BonusBox_Y"] = y;
				hash["BonusBox_NextBullet"] = bulletType;
				PhotonNetwork.CurrentRoom.SetCustomProperties(hash);
			}
		}
		else
		{
			transform.position = new Vector3(x, y, 0f);
			NextBullet = bulletType;
		}

		Color textColor = MyText.color;
		textColor.a = 1f;
		MyText.color = textColor;
		MyText.text = "?";
		Background.gameObject.SetActive(true);
		MyText.gameObject.SetActive(true);
		MyCollider.enabled = true;
	}

	private void OnTriggerEnter(Collider other)
	{
		Bullet bullet = other.GetComponent<Bullet>();
		if (bullet != null)
		{
			Tank currentTank = manager.GetCurrentPlayer();
			currentTank.BulletResetCounter = 1;
			if (manager.OnlineGame)
			{
				if (PhotonNetwork.IsMasterClient)
				{
					currentTank.BulletType = NextBullet;
					Hashtable hash = currentTank.MyPlayer.CustomProperties;
					hash["NextBullet"] = currentTank.BulletType;
					currentTank.MyPlayer.SetCustomProperties(hash);
				}
			}
			else
			{
				currentTank.BulletType = NextBullet;
			}

			string bulletName = manager.BulletPrefabs[NextBullet].name;
			MyText.text = bulletName;

			MyParticles.Play();
			Background.gameObject.SetActive(false);
			//MyText.gameObject.SetActive(false);
			MyCollider.enabled = false;
			Fading = true;
			FadeTimer = 1f;
		}
	}

	public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
	{
		if (propertiesThatChanged.ContainsKey("BonusBox_X"))
		{
			Vector3 position = transform.position;
			position.x = (float)propertiesThatChanged["BonusBox_X"];
			transform.position = position;

			Color textColor = MyText.color;
			textColor.a = 1f;
			MyText.color = textColor;
			MyText.text = "?";
			Background.gameObject.SetActive(true);
			MyText.gameObject.SetActive(true);
			MyCollider.enabled = true;
		}

		if (propertiesThatChanged.ContainsKey("BonusBox_Y"))
		{
			Vector3 position = transform.position;
			position.y = (float)propertiesThatChanged["BonusBox_Y"];
			transform.position = position;

			Color textColor = MyText.color;
			textColor.a = 1f;
			MyText.color = textColor;
			MyText.text = "?";
			Background.gameObject.SetActive(true);
			MyText.gameObject.SetActive(true);
			MyCollider.enabled = true;
		}

		if (propertiesThatChanged.ContainsKey("BonusBox_NextBullet"))
		{
			NextBullet = (int)propertiesThatChanged["BonusBox_NextBullet"];
		}
	}
}
