using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DarkOverlay : MonoBehaviour
{
	private static DarkOverlay reference;

	private SpriteRenderer MyRenderer;
	public float TransitionSpeed;
	public float TargetAlpha;

	public static DarkOverlay GetReference()
	{
		if (reference == null)
		{
			reference = GameObject.Find("DarkOverlay").GetComponent<DarkOverlay>();
		}
		return reference;
	}

	// Start is called before the first frame update
	void Start()
	{
		MyRenderer = GetComponent<SpriteRenderer>();
	}

	// Update is called once per frame
	void Update()
	{
		Color color = MyRenderer.color;
		if (color.a > TargetAlpha)
		{
			color.a -= TransitionSpeed * Time.deltaTime;
		}
		if (color.a < TargetAlpha)
		{
			color.a += TransitionSpeed * Time.deltaTime;
		}
		MyRenderer.color = color;
	}

	public void SetDarkness(float value)
	{
		TargetAlpha = value;
	}
}
