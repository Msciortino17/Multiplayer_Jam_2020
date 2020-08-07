using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class PlayerSetup : MonoBehaviour
{
	public int PlayerNumber;
	public int OnlineNumber = -1;

	public SetupGameMenu SetupGameRef;
	public InputField NameField;
	public Image MyColorDisplay;
	public int MyColor;
	public Dropdown MyControlType;

	public GameObject Active;
	public GameObject Inactive;

	// Start is called before the first frame update
	void Start()
	{
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

	public void SetName(string name)
	{
		NameField.text = name;
	}

	public Color GetColor()
	{
		return SetupGameRef.ColorOptions[MyColor];
	}

	public Tank.ControlType GetControlType()
	{
		return (Tank.ControlType)MyControlType.value;
	}

	public void SetControlType(Tank.ControlType type)
	{
		MyControlType.value = (int)type;
	}

	public void NextColor()
	{
		MyColor++;
		if (MyColor == SetupGameRef.ColorOptions.Count)
		{
			MyColor = 0;
		}
		MyColorDisplay.color = GetColor();
	}

	public void PrevColor()
	{
		MyColor--;
		if (MyColor == -1)
		{
			MyColor = SetupGameRef.ColorOptions.Count - 1;
		}
		MyColorDisplay.color = GetColor();
	}

	public void AddPlayer()
	{
		if (SetupGameRef.FromOnlineSetup && !PhotonNetwork.IsMasterClient)
		{
			return;
		}

		TurnOn();
	}

	public void Remove()
	{
		if (SetupGameRef.FromOnlineSetup)
		{
			SetupGameRef.Exit();
			return;
		}

		TurnOff();
	}

	public void TurnOn()
	{
		Active.SetActive(true);
		Inactive.SetActive(false);
	}

	public void TurnOff()
	{
		Active.SetActive(false);
		Inactive.SetActive(true);
	}
}
