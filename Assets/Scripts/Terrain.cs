using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Terrain : MonoBehaviour
{
	static Terrain reference;

	// Main data model
	private float[] heightMap;
	public float MapWidth;
	public int NumPoints;
	private float DuneWidth;

	// Generation params
	public float GenFrequency;
	public float GenAmplitude;
	public float GenBottom;

	// Prefabs and references
	public GameObject DunePrefab;

	public static Terrain GetReference()
	{
		if (reference == null)
		{
			reference = GameObject.Find("Terrain").GetComponent<Terrain>();
		}
		return reference;
	}

	// Start is called before the first frame update
	void Awake()
	{
		DuneWidth = MapWidth / NumPoints;
		heightMap = new float[NumPoints];

		GenerateTerrain();
		SetupDunes();
	}

	// Update is called once per frame
	void Update()
	{
		if (Input.GetKeyDown(KeyCode.R))
		{
			GenerateTerrain();
			SetupDunes();
		}
	}

	private void GenerateTerrain()
	{
		// Simple line
		//float height = 1f;
		//for (int i = 0; i < NumPoints; i++)
		//{
		//	heightMap[i] = height;
		//	height += 0.5f;
		//}

		// Sin wave
		float t = 0f;
		for (int i = 0; i < NumPoints; i++)
		{
			float height = GenAmplitude * Mathf.Sin(GenFrequency * t) + GenBottom;
			if (height < 1f)
			{
				height = 1f;
			}
			heightMap[i] = height;
			t += 0.1f;
		}
	}

	private void SetupDunes()
	{
		// Clear out old ones
		foreach (Transform remove in transform)
		{
			Destroy(remove.gameObject);
		}

		// Setup new ones
		for (int i = 0; i < NumPoints; i++)
		{
			GameObject dune = Instantiate(DunePrefab, transform);
			float height = heightMap[i];
			dune.transform.position = new Vector3(i * DuneWidth, height * 0.5f, 0f);
			dune.transform.localScale = new Vector3(DuneWidth, height, 1f);
		}
	}

	public float GetHeightAtX(float x)
	{
		float adjsutedX = x / DuneWidth;
		int p1 = (int)adjsutedX;

		if (p1 < 0 || p1 >= heightMap.Length)
		{
			return 0f;
		}

		return heightMap[p1];
	}

	public float GetSlopeAtX(float x)
	{
		float adjsutedX = x / DuneWidth;
		int p1 = (int)adjsutedX;
		int p2 = p1 + 1;
		if (p2 == NumPoints)
		{
			p2 = 0;
		}

		float width1 = p1 * DuneWidth;
		float width2 = p2 * DuneWidth;

		float height1 = heightMap[p1];
		float height2 = heightMap[p2];

		float dy = height2 - height1;
		float dx = width2 - width1;

		return dy / dx;
	}

	public float SlopeToDegrees(float slope)
	{
		return Mathf.Rad2Deg * Mathf.Atan(slope);
	}
}
