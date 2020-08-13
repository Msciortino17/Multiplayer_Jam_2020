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

	// Background data
	private float[] bg1_heightMap;
	private float bg1_DuneWidth;

	private float[] bg2_heightMap;
	private float bg2_DuneWidth;

	// Generation params
	public float GenFrequency;
	public float GenAmplitude;
	public float GenBottom;
	public float Seed;

	public int bg1_NumPoints;
	public float bg1_GenFrequency;
	public float bg1_GenAmplitude;
	public float bg1_GenBottom;
	public float bg1_Seed;

	public int bg2_NumPoints;
	public float bg2_GenFrequency;
	public float bg2_GenAmplitude;
	public float bg2_GenBottom;
	public float bg2_Seed;

	// Prefabs and references
	public GameObject DunePrefab;
	public GameObject DuneBackgroundPrefab;
	public GameObject DuneBackground2Prefab;

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

		bg1_DuneWidth = MapWidth / bg1_NumPoints;
		bg1_heightMap = new float[bg1_NumPoints];

		bg2_DuneWidth = MapWidth / bg2_NumPoints;
		bg2_heightMap = new float[bg2_NumPoints];

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

		bg1_Seed = Random.Range(0, 360f);
		bg2_Seed = Random.Range(0, 360f);
		GenerateBackground();

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

	private void GenerateBackground()
	{
		// Sin wave
		float t = bg1_Seed;
		for (int i = 0; i < bg1_NumPoints; i++)
		{
			float height = bg1_GenAmplitude * Mathf.Sin(bg1_GenFrequency * t) + bg1_GenBottom;
			if (height < 1f)
			{
				height = 1f;
			}
			bg1_heightMap[i] = height;
			t += 0.1f;
		}

		float t2 = bg2_Seed;
		for (int i = 0; i < bg2_NumPoints; i++)
		{
			float height = bg2_GenAmplitude * Mathf.Sin(bg2_GenFrequency * t) + bg2_GenBottom;
			if (height < 1f)
			{
				height = 1f;
			}
			bg2_heightMap[i] = height;
			t += 0.1f;
		}
	}

	private void GenerateSmoothTerrain()
	{
		float t = Seed;
		for (int i = 0; i < NumPoints; i++)
		{
			float height = Mathf.Sin(t * .5f) + 5;
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
		float t = Seed;
		float t2 = Seed;
		float t3 = Seed;
		float frequency = 1f / MapWidth;
		for (int i = 0; i < NumPoints; i++)
		{
			float height = 3f * Mathf.Sin(t * .25f) + 10;
			t += 0.2f;

			height += Mathf.Sin(t2 * .3f);
			t2 += 0.2f;

			height -= 2f * Mathf.Cos(t3 * .5f);
			t3 += 0.2f;

			if (height < 1f)
			{
				height = 1f;
			}
			heightMap[i] = height;
		}
	}

	private void GenerateRoughTerrain()
	{
		float t = Seed;
		float t2 = Seed;
		float t3 = Seed;
		float frequency = 1f / MapWidth;
		for (int i = 0; i < NumPoints; i++)
		{
			float height = 3f * Mathf.Sin(t * .45f) + 10;
			t += 0.2f;

			height += 3f * Mathf.Sin(t2 * .5f);
			t2 += 0.2f;

			height += 2f * Mathf.Cos(t3 * .35f);
			t3 += 0.2f;

			if (height < 1f)
			{
				height = 1f;
			}
			heightMap[i] = height;
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
			dune.transform.localScale = new Vector3(DuneWidth, height + .2f, 1f);
		}

		// Background
		for (int i = 0; i < bg1_NumPoints; i++)
		{
			GameObject dune = Instantiate(DuneBackgroundPrefab, transform);
			float height = bg1_heightMap[i];
			dune.transform.position = new Vector3(i * bg1_DuneWidth, height * 0.5f, 0f);
			dune.transform.localScale = new Vector3(bg1_DuneWidth, height, 1f);
		}

		for (int i = 0; i < bg2_NumPoints; i++)
		{
			GameObject dune = Instantiate(DuneBackground2Prefab, transform);
			float height = bg2_heightMap[i];
			dune.transform.position = new Vector3(i * bg2_DuneWidth, height * 0.5f, 0f);
			dune.transform.localScale = new Vector3(bg2_DuneWidth, height, 1f);
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
