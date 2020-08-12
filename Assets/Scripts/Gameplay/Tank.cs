using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Random = UnityEngine.Random;

public class Tank : MonoBehaviourPunCallbacks, IPunInstantiateMagicCallback
{
	public enum ControlType
	{
		LocalPlayer,
		EasyAI,
		NormalAI,
		HardAI,
		NetworkPlayer
	}
	public ControlType MyControlType;

	public int OnlineNumber = -1;
	public Player MyPlayer;
	public float NetworkTimer;
	public Text MyHoverText;

	private Terrain terrain;
	private GameManager manager;

	public float MinMoveSpeed;
	public float MaxMoveSpeed;

	// Turret and bullet stuff
	public float TurretSpeed;
	public float MaxTurretAngle;
	public float MinTurretAngle;
	public float BulletPower;
	public GameObject[] BulletPrefabs;
	public int BulletType;
	public Transform TurretPivot;
	public Transform Lump;
	private float FireDelayTimer;
	private bool WaitingToFire;

	// Game loop stuff
	public int IDNumber;
	public string TankName;
	public bool MyTurn;
	public float Health;
	public float Fuel;
	public float FuelConsumption;

	// Visual effect stuff
	public List<TankWheel> MyWheels;
	public float MinWheelSpin;
	public float MaxWheelSpin;
	public float PrevX;
	private List<Crosshair> CrossHairs;
	public GameObject CrossHairParent;
	public ParticleSystem DustParticles;
	public TankGun GunSprite;
	public ParticleSystem GunBlast;
	public ParticleSystem GunSmoke;

	// Trajectory preview
	public LineRenderer TrajectoryLine;
	public int NumTrajectoryPoints = 10;
	public float TrajectoryTempo = 0.1f;

	// AI control stuff
	private float AI_moveDistance;
	private bool AI_moving;
	private int AI_rotateDirection;
	private float AI_aimTime;
	private float AI_aimTimer;
	private float AI_fireTimer;
	private float AI_xPosGoal;
	private float AI_accuracy;

	// Bumpers for doing movement checks
	public Transform BumperUR;
	public Transform BumperLR;
	public Transform BumperUL;
	public Transform BumperLL;

	// Start is called before the first frame update
	void Start()
	{
		TrajectoryLine.positionCount = NumTrajectoryPoints;
		MyHoverText.text = TankName + " : " + OnlineNumber + "\nHealth: " + Health.ToString("0.00");

		MyWheels = new List<TankWheel>();
		MyWheels.AddRange(GetComponentsInChildren<TankWheel>());

		CrossHairs = new List<Crosshair>();
		CrossHairs.AddRange(CrossHairParent.GetComponentsInChildren<Crosshair>());

		PrevX = transform.position.x;
	}

	// Update is called once per frame
	void Update()
	{
		MyHoverText.text = TankName + "\nHealth: " + Health.ToString("0.00");
		if (MyTurn)
		{
			if (MyControlType == ControlType.LocalPlayer && !WaitingToFire)
			{
				HorizontalInput();
				TurretInput();
				MyHoverText.text += "\nFuel: " + Fuel.ToString("0.00");
			}
			else if (MyControlType == ControlType.NetworkPlayer)
			{
			}
			else
			{
				ComputerInput();
			}

			WrapWorld();
			AdjustAngle();
		}

		if (Health <= 0f)
		{
			MyHoverText.text = TankName + " RIP";
		}

		if (WaitingToFire)
		{
			FireDelayTimer -= Time.deltaTime;
			if (FireDelayTimer <= 0f)
			{
				WaitingToFire = false;

				// Fire Gun
				GunSprite.FireGun();
				GunBlast.Play();
				GunSmoke.Play();

				// Setup the bullet
				GameObject bullet = Instantiate(BulletPrefabs[BulletType]);
				Vector3 bulletVector = Lump.position - TurretPivot.position;
				bulletVector.Normalize();
				bulletVector *= BulletPower;
				bullet.GetComponent<Bullet>().velocity = bulletVector;
				bullet.transform.position = Lump.position;

				// End turn
				manager.NextPlayer();
			}
		}

		MyHoverText.gameObject.SetActive(!manager.Paused);
		SpinWheels();
		PrevX = transform.position.x;
	}

	private void SpinWheels()
	{
		float delta = (transform.position.x - PrevX);
		if (Mathf.Abs(delta) > 0f)
		{
			float slope = terrain.GetSlopeAtX(transform.position.x);
			slope = (slope + 1) * 0.5f;
			float spinSpeed = Mathf.Lerp(MinWheelSpin, MaxWheelSpin, slope);

			foreach (TankWheel wheel in MyWheels)
			{
				wheel.Spin(spinSpeed);
			}
		}
	}

	void OnCollisionEnter(Collision collision)
	{
		Tank otherTank = collision.gameObject.GetComponent<Tank>();
		if (otherTank != null)
		{
			Fuel = 0f;
		}
	}

	public void Init(string name, int id, Color color, ControlType controlType)
	{
		terrain = Terrain.GetReference();
		manager = GameManager.GetReference();
		AdjustHeight();
		AdjustAngle();

		BulletType = 0;

		TankName = name;
		IDNumber = id;
		MyControlType = controlType;

		//ChasisSprite.color = color;
		//TurretSprite.color = color;
		//LumpSprite.color = color;
		ColorSetter[] colorSetters = GetComponentsInChildren<ColorSetter>();
		for (int i = 0; i < colorSetters.Length; i++)
		{
			colorSetters[i].SetColor(color);
		}

		// AI setup
		switch (controlType)
		{
			case ControlType.EasyAI:
				AI_moveDistance = 0.05f;
				AI_aimTime = 1f;
				AI_accuracy = 0.1f;
				break;
			case ControlType.NormalAI:
				AI_moveDistance = 0.1f;
				AI_aimTime = 2f;
				AI_accuracy = 0.5f;
				break;
			case ControlType.HardAI:
				AI_moveDistance = 0.15f;
				AI_aimTime = 3f;
				AI_accuracy = 0.95f;
				break;
		}
	}

	public void StartTurn()
	{
		MyTurn = true;
		Fuel = 100f;

		// Skip turn if dead
		if (Health <= 0f)
		{
			manager.NextPlayer();
			manager.BulletLanded();
		}

		if (MyControlType != ControlType.LocalPlayer && MyControlType != ControlType.NetworkPlayer)
		{
			float mapTenth = terrain.MapWidth * AI_moveDistance;
			AI_xPosGoal = transform.position.x;
			AI_xPosGoal += Random.Range(-mapTenth, mapTenth);
			if (AI_xPosGoal < 0f)
			{
				AI_xPosGoal += terrain.MapWidth;
			}
			else if (AI_xPosGoal > terrain.MapWidth)
			{
				AI_xPosGoal -= terrain.MapWidth;
			}
			AI_moving = true;
			AI_fireTimer = 0.3f;
			AI_aimTimer = AI_aimTime;
			AI_rotateDirection = Random.Range(0, 2) == 0 ? -1 : 1;
		}

		//ZoomedView.GetReference().StartFollowing(transform);
	}

	private void HorizontalInput()
	{
		if (Input.GetKey(KeyCode.LeftArrow) && Fuel > 0f && CanMoveLeft())
		{
			MoveLeft();
		}

		if (Input.GetKey(KeyCode.RightArrow) && Fuel > 0f && CanMoveRight())
		{
			MoveRight();
		}
	}

	private void TurretInput()
	{
		float turretAngle = TurretPivot.localRotation.eulerAngles.z;
		if (Input.GetKey(KeyCode.UpArrow) && turretAngle < MaxTurretAngle)
		{
			TurretPivot.Rotate(0f, 0f, TurretSpeed * Time.deltaTime);
		}

		if (Input.GetKey(KeyCode.DownArrow) && turretAngle > MinTurretAngle)
		{
			TurretPivot.Rotate(0f, 0f, -TurretSpeed * Time.deltaTime);
		}

		if (Input.GetKeyDown(KeyCode.Space))
		{
			//TrajectoryLine.gameObject.SetActive(true);
			CrossHairParent.gameObject.SetActive(true);
		}

		if (Input.GetKey(KeyCode.Space))
		{
			//UpdateTrajectory();
			UpdateCrossHairs();
			//ZoomedView.GetReference().ReturnToStandard();
		}

		if (Input.GetKeyUp(KeyCode.Space))
		{
			//TrajectoryLine.gameObject.SetActive(false);
			CrossHairParent.gameObject.SetActive(false);
			if (OnlineNumber == -1)
			{
				FireBullet();
				//ZoomedView.GetReference().StartFollowing(bullet.transform);
			}
			else
			{
				photonView.RPC("FireBullet", RpcTarget.All);
			}
		}
	}

	private void ComputerInput()
	{
		if (AI_moving)
		{
			FlailTurret();
			float toGoal = AI_xPosGoal - transform.position.x;
			if (Mathf.Abs(toGoal) < 0.1f)
			{
				AI_moving = false;
				//TrajectoryLine.gameObject.SetActive(true);
			}
			else if (toGoal < 0f || toGoal > terrain.MapWidth * 0.5f)
			{
				if (CanMoveLeft())
				{
					MoveLeft();
				}
				else
				{
					AI_moving = false;
					//TrajectoryLine.gameObject.SetActive(true);
				}
			}
			else if (toGoal > 0f || toGoal < -terrain.MapWidth * 0.5f)
			{
				if (CanMoveRight())
				{
					MoveRight();
				}
				else
				{
					AI_moving = false;
					//TrajectoryLine.gameObject.SetActive(true);
				}
			}
			if (Fuel <= 0f)
			{
				AI_moving = false;
				//TrajectoryLine.gameObject.SetActive(true);
			}
		}
		else
		{
			if (AI_aimTimer > 0f)
			{
				//UpdateTrajectory();
				AI_aimTimer -= Time.deltaTime;

				FlailTurret();

				float radius = Mathf.Lerp(1f, 0.1f, AI_accuracy);
				if (ShouldHitSomething(radius, 0.1f, 50))
				{
					AI_aimTimer = 0f;
				}
			}
			else if (AI_fireTimer > 0f)
			{
				TrajectoryLine.gameObject.SetActive(false);
				AI_fireTimer -= Time.deltaTime;
				if (AI_fireTimer <= 0f)
				{
					FireBullet();
				}
			}
		}
	}

	private void FlailTurret()
	{
		float turretAngle = TurretPivot.localRotation.eulerAngles.z;

		if (AI_rotateDirection == -1 && turretAngle < MinTurretAngle)
		{
			AI_rotateDirection = 1;
		}

		if (AI_rotateDirection == 1 && turretAngle > MaxTurretAngle)
		{
			AI_rotateDirection = -1;
		}

		TurretPivot.Rotate(0f, 0f, AI_rotateDirection * TurretSpeed * Time.deltaTime);
	}

	private void MoveLeft()
	{
		float slope = terrain.GetSlopeAtX(transform.position.x);
		slope = (slope + 1) * 0.5f;
		float moveSpeed = Mathf.Lerp(MinMoveSpeed, MaxMoveSpeed, slope);
		transform.Translate(transform.right * -moveSpeed * Time.deltaTime, Space.World);
		Fuel -= FuelConsumption * Time.deltaTime;
	}

	private void MoveRight()
	{
		float slope = terrain.GetSlopeAtX(transform.position.x);
		slope = (slope + 1) * 0.5f;
		float moveSpeed = Mathf.Lerp(MaxMoveSpeed, MinMoveSpeed, slope);
		transform.Translate(transform.right * moveSpeed * Time.deltaTime, Space.World);
		Fuel -= FuelConsumption * Time.deltaTime;
	}

	[PunRPC]
	private void FireBullet()
	{
		WaitingToFire = true;
		FireDelayTimer = 0.5f;
	}

	private bool ShouldHitSomething(float radius, float tempo, int checks)
	{
		Vector3 bulletVector = Lump.position - TurretPivot.position;
		bulletVector.Normalize();
		bulletVector *= BulletPower;

		Vector3 currentPoint = Lump.transform.position;
		for (int i = 0; i < checks; i++)
		{
			float height = terrain.GetHeightAtX(currentPoint.x);
			if (height > currentPoint.y)
			{
				return false;
			}

			Collider[] colliders = Physics.OverlapSphere(currentPoint, radius);
			foreach (Collider collider in colliders)
			{
				Tank other = collider.GetComponent<Tank>();
				if (other != null && other.IDNumber != IDNumber && other.Health > 0f)
				{
					return true;
				}
			}

			currentPoint += bulletVector * tempo;
			if (currentPoint.x > terrain.MapWidth)
			{
				currentPoint.x = 0.001f;
			}
			else if (currentPoint.x < 0f)
			{
				currentPoint.x = terrain.MapWidth - 0.001f;
			}

			bulletVector += Physics.gravity * tempo;
			bulletVector += manager.GetWind() * tempo;

		}

		return false;
	}

	private bool CanMoveRight()
	{
		bool upperClear = !Physics.Raycast(BumperUR.position, BumperUR.right, 0.5f);
		bool lowerClear = !Physics.Raycast(BumperLR.position, BumperUR.right, 0.5f);

		return upperClear && lowerClear;
	}

	private bool CanMoveLeft()
	{
		bool upperClear = !Physics.Raycast(BumperUL.position, BumperUL.right, 0.5f);
		bool lowerClear = !Physics.Raycast(BumperLL.position, BumperUL.right, 0.5f);

		return upperClear && lowerClear;
	}

	private void WrapWorld()
	{
		if (transform.position.x > terrain.MapWidth)
		{
			Vector3 position = transform.position;
			position.x = 0.001f;
			transform.position = position;
			AdjustHeight();
		}

		if (transform.position.x < 0f)
		{
			Vector3 position = transform.position;
			position.x = terrain.MapWidth - 0.001f;
			transform.position = position;
			AdjustHeight();
		}

		bool useDust = (transform.position.x < terrain.MapWidth * 0.99f && transform.position.x > 0.1f);
		DustParticles.gameObject.SetActive(useDust);
	}

	private void AdjustHeight()
	{
		Vector3 position = transform.position;
		position.y = terrain.GetHeightAtX(position.x) + 0.5f;
		transform.position = position;
	}

	private void AdjustAngle()
	{
		float slope = terrain.GetSlopeAtX(transform.position.x);
		float angle = terrain.SlopeToDegrees(slope);
		transform.rotation = Quaternion.Euler(0f, 0f, angle);
	}

	private void UpdateTrajectory()
	{
		Vector3 bulletVector = Lump.position - TurretPivot.position;
		bulletVector.Normalize();
		bulletVector *= BulletPower;

		List<Vector3> trajectoryPoints = new List<Vector3>();
		Vector3 currentPoint = Lump.transform.position;
		for (int i = 0; i < NumTrajectoryPoints; i++)
		{
			trajectoryPoints.Add(currentPoint);

			currentPoint += bulletVector * TrajectoryTempo;
			bulletVector += Physics.gravity * TrajectoryTempo;
			bulletVector += manager.GetWind() * TrajectoryTempo;

		}

		TrajectoryLine.SetPositions(trajectoryPoints.ToArray());
	}

	private void UpdateCrossHairs()
	{
		Vector3 bulletVector = Lump.position - TurretPivot.position;
		bulletVector.Normalize();
		bulletVector *= BulletPower;

		List<Vector3> trajectoryPoints = new List<Vector3>();
		Vector3 currentPoint = Lump.transform.position;
		for (int i = 0; i < NumTrajectoryPoints; i++)
		{
			if (currentPoint.x > terrain.MapWidth)
			{
				currentPoint.x -= terrain.MapWidth;
			}

			if (currentPoint.x < 0f)
			{
				currentPoint.x += terrain.MapWidth;
			}

			trajectoryPoints.Add(currentPoint);

			currentPoint += bulletVector * TrajectoryTempo;
			bulletVector += Physics.gravity * TrajectoryTempo;
			bulletVector += manager.GetWind() * TrajectoryTempo;

			if (terrain.GetHeightAtX(currentPoint.x) > currentPoint.y)
			{
				break;
			}
		}

		int halfWay = (int)(trajectoryPoints.Count * 0.5f);
		int spaceBetween = (int)(trajectoryPoints.Count * 0.05f) + 1;
		for (int i = 0; i < CrossHairs.Count; i++)
		{
			Crosshair crosshair = CrossHairs[i];
			int index = (i * spaceBetween) + halfWay;
			if (index >= trajectoryPoints.Count)
			{
				index = trajectoryPoints.Count - 1;
			}
			crosshair.transform.position = trajectoryPoints[index];
			crosshair.transform.rotation = Quaternion.identity;
			crosshair.WrapWorld(terrain);
		}
	}

	public void OnPhotonInstantiate(PhotonMessageInfo info)
	{
		manager = GameManager.GetReference();
		terrain = Terrain.GetReference();

		int actorNum = info.Sender.ActorNumber;
		MyPlayer = info.Sender;
		OnlineNumber = actorNum;
		float seed = (float)PhotonNetwork.CurrentRoom.CustomProperties["TerrainSeed"];
		terrain.Init(SetupGameMenu.GetReference().TerrainType.value, seed);
		if (actorNum == PhotonNetwork.LocalPlayer.ActorNumber)
		{
			return;
		}

		SetupGameMenu setup = SetupGameMenu.GetReference();
		int playerCount = setup.GetPlayerCount();
		float offset = terrain.MapWidth / (playerCount + 1);
		int counter = 0;
		for (int i = 0; i < setup.PlayerDataList.Count; i++)
		{
			PlayerSetup playerData = setup.PlayerDataList[i];
			if (playerData.Active.activeInHierarchy)
			{
				if (playerData.OnlineNumber == OnlineNumber)
				{
					Vector3 position = new Vector3((counter + 1) * offset, 0f, 0f);
					transform.parent = setup.TankParent;
					Init(playerData.GetName(), counter, playerData.GetColor(), ControlType.NetworkPlayer);
					manager.PlayerTanks.Add(this);
				}
				counter++;
			}
		}

		setup.NumLoadedTanks++;
	}

	//public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
	//{
	//	ReadHealthSettings();
	//	ReadFuelSettings();
	//	MyHoverText.text = TankName + " : " + OnlineNumber + "\nHealth: " + Health.ToString("0.00") + "\nFuel: " + Fuel.ToString("0.00");
	//}

	//public void ReadHealthSettings()
	//{
	//	if (manager.OnlineGame)
	//	{
	//		Hashtable hash = MyPlayer.CustomProperties;
	//		Health = (float)hash["Health"];
	//	}
	//}

	//public void WriteHealthSettings()
	//{
	//	if (manager.OnlineGame)
	//	{
	//		Hashtable hash = MyPlayer.CustomProperties;
	//		hash["Health"] = Health;
	//		MyPlayer.SetCustomProperties(hash);
	//	}
	//}

	//public void ReadFuelSettings()
	//{
	//	if (manager.OnlineGame)
	//	{
	//		Hashtable hash = MyPlayer.CustomProperties;
	//		Fuel = (float)hash["Fuel"];
	//	}
	//}

	//public void WriteFuelSettings()
	//{
	//	if (manager.OnlineGame)
	//	{
	//		Hashtable hash = MyPlayer.CustomProperties;
	//		hash["Fuel"] = Fuel;
	//		MyPlayer.SetCustomProperties(hash);
	//	}
	//}
}
