using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class Utility
{

}

[Serializable]
public sealed class PlayerInformation
{
	public int[] RewiredID;
	public int[] GamePlayerID;
	public int[] ColorIndex;

	/// <summary>
	/// 
	/// </summary>
	/// <param name="rewiredID"></param>
	/// <param name="gamePlayerID"></param>
	/// <param name="colorIndex">The Index of the Color of player, which is always the same set in Config</param>
	public PlayerInformation(int[] rewiredID, int[] gamePlayerID, int[] colorIndex)
	{
		RewiredID = rewiredID;
		GamePlayerID = gamePlayerID;
		ColorIndex = colorIndex;
	}
}

public enum GameMapMode
{
	FoodMode,
	CartMode,
	FoodCartMode,
	ObjectiveMode,
	BrawlMode,
}
