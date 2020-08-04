using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSetup : MonoBehaviour
{
	public SetupGameMenu SetupGameRef;
	public Image MyPortraitRef;
	public InputField NameField;
	public Image MyColorDisplay;
	public int MyColor;
	public Dropdown MyControlType;

	// Start is called before the first frame update
	void Start()
	{
		MyPortraitRef.color = GetColor();
		MyColorDisplay.color = GetColor();
	}

	// Update is called once per frame
	void Update()
	{

	}

	public string GetName()
	{
		return NameField.text;
	}

	public Color GetColor()
	{
		return SetupGameRef.ColorOptions[MyColor];
	}

	public Tank.ControlType GetControlType()
	{
		return (Tank.ControlType)MyControlType.value;
	}

	public void NextColor()
	{
		MyColor++;
		if (MyColor == SetupGameRef.ColorOptions.Count)
		{
			MyColor = 0;
		}
		MyPortraitRef.color = GetColor();
		MyColorDisplay.color = GetColor();
	}

	public void PrevColor()
	{
		MyColor--;
		if (MyColor == -1)
		{
			MyColor = SetupGameRef.ColorOptions.Count - 1;
		}
		MyPortraitRef.color = GetColor();
		MyColorDisplay.color = GetColor();
	}

	public void Remove()
	{
		if (SetupGameRef.PlayerDataList.Count > 1)
		{
			SetupGameRef.PlayerDataList.Remove(this);
			Destroy(gameObject);
		}
	}
}
