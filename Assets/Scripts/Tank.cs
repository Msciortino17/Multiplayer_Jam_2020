using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tank : MonoBehaviour
{
	private Terrain terrain;

	public float MoveSpeed;
	public float TurretSpeed;

	public Transform TurretPivot;
	public Transform Lump;
	public float MaxTurretAngle;
	public float MinTurretAngle;

	public float BulletPower;
	public GameObject BulletPrefab;

	// Start is called before the first frame update
	void Start()
	{
		terrain = Terrain.GetReference();
		AdjustHeight();
	}

	// Update is called once per frame
	void Update()
	{
		MoveHorizontally();
		AimTurret();
		WrapWorld();
		AdjustAngle();
	}

	private void MoveHorizontally()
	{
		if (Input.GetKey(KeyCode.LeftArrow))
		{
			transform.Translate(transform.right * -MoveSpeed * Time.deltaTime, Space.World);
		}

		if (Input.GetKey(KeyCode.RightArrow))
		{
			transform.Translate(transform.right * MoveSpeed * Time.deltaTime, Space.World);
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
			GameObject bullet = Instantiate(BulletPrefab);
			Vector3 bulletVector = Lump.position - TurretPivot.position;
			bulletVector.Normalize();
			bulletVector *= BulletPower;
			bullet.GetComponent<Bullet>().velocity = bulletVector;
			bullet.transform.position = Lump.position;
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
