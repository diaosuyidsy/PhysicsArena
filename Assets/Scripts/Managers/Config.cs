using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Config
{
	public ConfigData ConfigData;
	public GameMapData GameMapData;

	public Config(ConfigData _cd, GameMapData _gmp)
	{
		ConfigData = _cd;
		GameMapData = _gmp;
	}

	public void Destroy()
	{
	}
}
