using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Config
{
	public ConfigData ConfigData;
	public PlayerController[] Players;

	public Config(ConfigData _cd)
	{
		ConfigData = _cd;
		Players = GameObject.Find("Players").GetComponentsInChildren<PlayerController>(true);
	}

	public void Destroy()
	{
		Players = null;
	}
}
