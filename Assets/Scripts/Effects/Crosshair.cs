using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crosshair : MonoBehaviour
{
	public int Index;

	// Start is called before the first frame update
	void Start()
	{

	}

	// Update is called once per frame
	void Update()
	{

	}

	public void WrapWorld(Terrain terrain)
	{
		if (transform.position.x > terrain.MapWidth)
		{
			Vector3 position = transform.position;
			position.x -= terrain.MapWidth;
			transform.position = position;
		}

		if (transform.position.x < 0f)
		{
			Vector3 position = transform.position;
			position.x += terrain.MapWidth;
			transform.position = position;
		}
	}
}
