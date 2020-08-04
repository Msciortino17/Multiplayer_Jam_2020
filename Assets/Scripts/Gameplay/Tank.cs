using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tank : MonoBehaviour
{
	public enum ControlType
	{
		LocalPlayer,
		EasyAI,
		NormalAI,
		HardAI,
		NetworkPlayer
	}
	private ControlType MyControlType;

	private Terrain terrain;
	private GameManager manager;

	public float MinMoveSpeed;
	public float MaxMoveSpeed;

	// Turret and bullet stuff
	public float TurretSpeed;
	public float MaxTurretAngle;
	public float MinTurretAngle;
	public float BulletPower;
	public GameObject BulletPrefab;
	public Transform TurretPivot;
	public Transform Lump;

	// Game loop stuff
	public int IDNumber;
	public string TankName;
	public bool MyTurn;
	public float Health;
	public float Fuel;
	public float FuelConsumption;

	// Color stuff
	public SpriteRenderer ChasisSprite;
	public SpriteRenderer TurretSprite;
	public SpriteRenderer LumpSprite;

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

	// Start is called before the first frame update
	void Start()
	{
		TrajectoryLine.positionCount = NumTrajectoryPoints;
	}

	// Update is called once per frame
	void Update()
	{
		if (MyTurn)
		{
			if (MyControlType == ControlType.LocalPlayer)
			{
				HorizontalInput();
				TurretInput();
			}
			else if (MyControlType == ControlType.NetworkPlayer)
			{
				// todo
			}
			else
			{
				ComputerInput();
			}

			WrapWorld();
			AdjustAngle();

			DebugText.SetText(TankName + ", Health: " + Health.ToString("0.00") + ", Fuel: " + Fuel.ToString("0.00"));
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

		TankName = name;
		IDNumber = id;
		MyControlType = controlType;

		ChasisSprite.color = color;
		TurretSprite.color = color;
		LumpSprite.color = color;

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
	}

	private void HorizontalInput()
	{
		if (Input.GetKey(KeyCode.LeftArrow) && Fuel > 0f)
		{
			MoveLeft();
		}

		if (Input.GetKey(KeyCode.RightArrow) && Fuel > 0f)
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
			TrajectoryLine.gameObject.SetActive(true);
		}

		if (Input.GetKey(KeyCode.Space))
		{
			UpdateTrajectory();
		}

		if (Input.GetKeyUp(KeyCode.Space))
		{
			TrajectoryLine.gameObject.SetActive(false);
			FireBullet();
		}
	}

	private void ComputerInput()
	{
		if (AI_moving)
		{
			float toGoal = AI_xPosGoal - transform.position.x;
			if (Mathf.Abs(toGoal) < 0.1f)
			{
				AI_moving = false;
				TrajectoryLine.gameObject.SetActive(true);
			}
			else if (toGoal < 0f || toGoal > terrain.MapWidth * 0.5f)
			{
				MoveLeft();
			}
			else if (toGoal > 0f || toGoal < -terrain.MapWidth * 0.5f)
			{
				MoveRight();
			}
			if (Fuel <= 0f)
			{
				AI_moving = false;
				TrajectoryLine.gameObject.SetActive(true);
			}
		}
		else
		{
			if (AI_aimTimer > 0f)
			{
				UpdateTrajectory();
				AI_aimTimer -= Time.deltaTime;

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

	private void FireBullet()
	{
		// Setup the bullet
		GameObject bullet = Instantiate(BulletPrefab);
		Vector3 bulletVector = Lump.position - TurretPivot.position;
		bulletVector.Normalize();
		bulletVector *= BulletPower;
		bullet.GetComponent<Bullet>().velocity = bulletVector;
		bullet.transform.position = Lump.position;

		// End turn
		manager.NextPlayer();
	}

	private bool ShouldHitSomething(float radius, float tempo, int checks)
	{
		Vector3 bulletVector = Lump.position - TurretPivot.position;
		bulletVector.Normalize();
		bulletVector *= BulletPower;

		Vector3 currentPoint = Lump.transform.position;
		for (int i = 0; i < NumTrajectoryPoints; i++)
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
				if (other != null && other != this && other.Health > 0f)
				{
					return true;
				}
			}

			currentPoint += bulletVector * TrajectoryTempo;
			if (currentPoint.x > terrain.MapWidth)
			{
				currentPoint.x = 0.001f;
			}
			else if (currentPoint.x < 0f)
			{
				currentPoint.x = terrain.MapWidth - 0.001f;
			}

			bulletVector += Physics.gravity * TrajectoryTempo;
			bulletVector += manager.GetWind() * TrajectoryTempo;

		}

		return false;
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


}
