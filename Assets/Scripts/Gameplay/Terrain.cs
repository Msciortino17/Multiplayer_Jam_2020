using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Terrain : MonoBehaviour
{
	static Terrain reference;

	public bool initialized;

	// Main data model
	private float[] heightMap;
	public float MapWidth;
	public int NumPoints;
	private float DuneWidth;

	// Generation params
	public float GenFrequency;
	public float GenAmplitude;
	public float GenBottom;
	public float Seed;

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

	public void Init(int roughness, float seed)
	{
		if (initialized)
		{
			return;
		}
		initialized = true;

		DuneWidth = MapWidth / NumPoints;
		heightMap = new float[NumPoints];

		// todo - set seed
		Seed = seed;
		switch (roughness)
		{
			case 0:
				GenerateSmoothTerrain();
				break;
			case 1:
				GenerateStandardTerrain();
				break;
			case 2:
				GenerateRoughTerrain();
				break;
		}

		SetupDunes();
	}

	private void GenerateTerrain()
	{
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

	private void GenerateSmoothTerrain()
	{
		// Sin wave
		float t = Seed;
		for (int i = 0; i < NumPoints; i++)
		{
			float height = Mathf.Sin(t) + 5;
			if (height < 1f)
			{
				height = 1f;
			}
			heightMap[i] = height;
			t += 0.1f;
		}
	}

	private void GenerateStandardTerrain()
	{
		// Sin wave
		float t = Seed;
		float t2 = Seed;
		float t3 = Seed;
		float frequency = 1f / MapWidth;
		for (int i = 0; i < NumPoints; i++)
		{
			float height = 3f * Mathf.Sin(t * .5f) + 10;
			height += Mathf.Sin(t2 * .75f);
			height -= 2f * Mathf.Cos(t3);
			if (height < 1f)
			{
				height = 1f;
			}
			heightMap[i] = height;
			t += 0.1f;
			t2 += 0.1f;
			t3 += 0.1f;
		}
	}

	private void GenerateRoughTerrain()
	{

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
