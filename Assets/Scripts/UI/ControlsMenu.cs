using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlsMenu : MonoBehaviour
{
	public OptionsMenu OptionsMenuRef;

	// Start is called before the first frame update
	void Start()
	{

	}

	// Update is called once per frame
	void Update()
	{

	}

	public void Exit()
	{
		OptionsMenuRef.gameObject.SetActive(true);
		gameObject.SetActive(false);
	}
}
