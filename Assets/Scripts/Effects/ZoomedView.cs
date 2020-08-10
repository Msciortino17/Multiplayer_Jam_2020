using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class ZoomedView : MonoBehaviour
{
	private static ZoomedView reference;
	public GameObject StandardView;
	public CinemachineVirtualCamera cinemaCamera;

	public static ZoomedView GetReference()
	{
		if (reference == null)
		{
			reference = GameObject.Find("Cameras").transform.Find("ZoomedView").GetComponent<ZoomedView>();
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

	public void ReturnToStandard()
	{
		StandardView.SetActive(true);
		gameObject.SetActive(false);
	}

	public void StartFollowing(Transform target)
	{
		cinemaCamera.Follow = target;
		StandardView.SetActive(false);
		gameObject.SetActive(true);
	}
}
