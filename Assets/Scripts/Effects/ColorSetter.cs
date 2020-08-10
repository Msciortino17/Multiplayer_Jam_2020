using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorSetter : MonoBehaviour
{
	public void SetColor(Color color)
	{
		SpriteRenderer sprite = GetComponent<SpriteRenderer>();
		if (sprite != null)
		{
			sprite.color = color;
		}
		Light light = GetComponent<Light>();
		if (light != null)
		{
			light.color = color;
		}
		Destroy(this);
	}
}
