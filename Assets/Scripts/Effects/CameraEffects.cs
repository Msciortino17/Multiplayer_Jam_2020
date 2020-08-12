using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraEffects : MonoBehaviour
{
	private static CameraEffects reference;
	public CinemachineVirtualCamera StandardView;
	public CinemachineVirtualCamera ZoomedCamera;
	public CinemachineVirtualCamera TankMovingView;
	public CinemachineVirtualCamera SandstormCamera;
	public CinemachineVirtualCamera QuickShakeCamera;
	public float ShakeTimer;

	public static CameraEffects GetReference()
	{
		if (reference == null)
		{
			reference = GameObject.Find("Cameras").GetComponent<CameraEffects>();
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
		if (ShakeTimer > 0f)
		{
			ShakeTimer -= Time.deltaTime;
			if (ShakeTimer <= 0f)
			{
				QuickShakeCamera.gameObject.SetActive(false);
			}
		}
	}

	public void ReturnToStandard()
	{
		StandardView.gameObject.SetActive(true);
	}

	public void SetTankMoving(bool on)
	{
		TankMovingView.gameObject.SetActive(on);
	}

	public void SetSandStorm(bool on)
	{
		SandstormCamera.gameObject.SetActive(on);
	}

	public void Shake(float duration)
	{
		ShakeTimer = duration;
		QuickShakeCamera.gameObject.SetActive(true);
	}

	public void StartFollowing(Transform target)
	{
		ZoomedCamera.Follow = target;
		StandardView.gameObject.SetActive(false);
	}
}
