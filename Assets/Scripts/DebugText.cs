using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugText : MonoBehaviour
{
	public Text MyText;
	public static DebugText reference;

	// Start is called before the first frame update
	void Start()
	{
		MyText = GetComponent<Text>();
		reference = GameObject.Find("Canvas").transform.Find("DebugText").GetComponent<DebugText>();
	}

	/// <summary>
	/// Can be called anywhere and will set the debug text to whatever we want.
	/// Will be used a lot throughout development, the final build might not have any calls.
	/// </summary>
	public static void SetText(string text)
	{
		reference.MyText.text = text;
	}
}
