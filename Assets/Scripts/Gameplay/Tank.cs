using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tank : MonoBehaviour
{
	private Terrain terrain;
	private GameManager manager;

	public float MoveSpeed;

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

	// Start is called before the first frame update
	void Start()
	{
	}

	// Update is called once per frame
	void Update()
	{
		if (MyTurn)
		{
			MoveHorizontally();
			AimTurret();
			WrapWorld();
			AdjustAngle();

			DebugText.SetText(TankName + ", Health: " + Health.ToString("0.00") + ", Fuel: " + Fuel.ToString("0.00"));
		}
	}

	public void Init(string name, int id, Color color)
	{
		terrain = Terrain.GetReference();
		manager = GameManager.GetReference();
		AdjustHeight();
		AdjustAngle();

		TankName = name;
		IDNumber = id;

		ChasisSprite.color = color;
		TurretSprite.color = color;
		LumpSprite.color = color;
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
	}

	private void MoveHorizontally()
	{
		if (Input.GetKey(KeyCode.LeftArrow) && Fuel > 0f)
		{
			transform.Translate(transform.right * -MoveSpeed * Time.deltaTime, Space.World);
			Fuel -= FuelConsumption * Time.deltaTime;
		}

		if (Input.GetKey(KeyCode.RightArrow) && Fuel > 0f)
		{
			transform.Translate(transform.right * MoveSpeed * Time.deltaTime, Space.World);
			Fuel -= FuelConsumption * Time.deltaTime;
		}
	}

	private void AimTurret()
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
}
