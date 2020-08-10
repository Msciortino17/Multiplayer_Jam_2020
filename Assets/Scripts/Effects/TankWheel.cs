using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankWheel : MonoBehaviour
{
	public float prevX;
	public float MaxMoveSpeed;
	public float MinMoveSpeed;
	public float MaxSpinSpeed;
	public float MinSpeedSpeed;

	// Start is called before the first frame update
	void Start()
	{

	}

	//void FixedUpdate()
	//{
	//	float currentX = transform.position.x;
	//	float deltaX = currentX - prevX;
	//	float xPerSecond = deltaX * Time.fixedDeltaTime;

	//	float a = Mathf.Lerp(MinMoveSpeed, MaxMoveSpeed, (xPerSecond - MinMoveSpeed) / MaxMoveSpeed);
	//	float rotateSpeed = Mathf.Lerp(MinSpeedSpeed, MaxSpinSpeed, a);

	//	transform.Rotate(0f, 0f, rotateSpeed * Time.deltaTime);

	//	prevX = currentX;
	//}

	public void Spin(float speed)
	{
		transform.Rotate(0f, 0f, speed * Time.deltaTime);
	}
}
