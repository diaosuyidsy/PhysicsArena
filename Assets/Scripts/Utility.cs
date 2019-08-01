using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class Utility
{
	// This function takes the center position on sreen
	// and calculates the maximum length of its position to the four corners
	public static float GetMaxLengthToCorner(Vector2 centerposition)
	{
		Vector2[] screenVertices = new Vector2[]
		{
			new Vector2(0f, 0f),
			new Vector2(Screen.width, 0f),
			new Vector2(Screen.width, Screen.height),
			new Vector2(0f, Screen.height),
		};

		float maxLength = 0f;
		for (int i = 0; i < 4; i++)
		{
			float length = Vector2.Distance(centerposition, screenVertices[i]);
			if (length > maxLength)
			{
				maxLength = length;
			}
		}
		return maxLength;
	}
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
