using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Beacon : MonoBehaviour
{
	private SpriteRenderer MyRenderer;
	public float Speed;
	public float OffSet;

	// Start is called before the first frame update
	void Start()
	{
		MyRenderer = GetComponent<SpriteRenderer>();
	}

	// Update is called once per frame
	void Update()
	{
		Color color = MyRenderer.color;
		color.a = Mathf.PingPong(Time.time * Speed + OffSet, 1f);
		MyRenderer.color = color;
	}
}
