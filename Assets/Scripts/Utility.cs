using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using CharTween;
using DG.Tweening;

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

	// Sequence example, bubbly fade-in + bounce
	public static Sequence BubbleFadeIn(CharTweener _tweener, int start, int end, float amplitude = 100f, float duration = 0.5f)
	{
		var sequence = DOTween.Sequence();

		for (var i = start; i <= end; ++i)
		{
			var timeOffset = Mathf.Lerp(0, 1, (i - start) / (float)(end - start + 1));
			var charSequence = DOTween.Sequence();
			charSequence.Append(_tweener.DOLocalMoveY(i, amplitude, duration).SetEase(Ease.InOutCubic))
				.Join(_tweener.DOFade(i, 0, duration).From())
				.Join(_tweener.DOScale(i, 0, duration).From().SetEase(Ease.OutBack, 5))
				.Append(_tweener.DOLocalMoveY(i, 0, duration).SetEase(Ease.OutBounce));
			sequence.Insert(timeOffset, charSequence);
		}

		return sequence;
	}

	// Sequence example, bubbly fade-out + bounce
	public static Sequence BubbleFadeOut(CharTweener _tweener, int start, int end, float amplitude = 100f, float duration = 0.5f)
	{
		var sequence = DOTween.Sequence();

		for (var i = start; i <= end; ++i)
		{
			var timeOffset = Mathf.Lerp(0, 1, (i - start) / (float)(end - start + 1));
			var charSequence = DOTween.Sequence();
			charSequence.Append(_tweener.DOLocalMoveY(i, amplitude, duration).SetEase(Ease.InBounce))
				.Join(_tweener.DOScale(i, 1, duration).From().SetEase(Ease.InBack, 2))
				.Append(_tweener.DOFade(i, 1, duration).From())
			.Append(_tweener.DOLocalMoveY(i, 0, duration).SetEase(Ease.InOutCubic));
			sequence.Insert(timeOffset, charSequence);
		}

		return sequence;
	}
}

public class StatisticsRecord
{
	public int RewiredID;
	public int MaxTime;
	public float MaxTime_Float;

	public StatisticsRecord(int rewiredID, int maxTime)
	{
		RewiredID = rewiredID;
		MaxTime = maxTime;
	}

	public StatisticsRecord(int rewiredID, float maxTime)
	{
		RewiredID = rewiredID;
		MaxTime_Float = maxTime;
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
