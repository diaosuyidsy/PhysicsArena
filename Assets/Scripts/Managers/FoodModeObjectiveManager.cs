using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodModeObjectiveManager : ObjectiveManager
{
	private int Team1Score;
	private int Team2Score;

	public FoodModeObjectiveManager()
	{
		EventManager.Instance.AddHandler<FoodDelivered>(_onFoodDelivered);
	}

	public override void Destroy()
	{
		EventManager.Instance.RemoveHandler<FoodDelivered>(_onFoodDelivered);
	}

	private void _onFoodDelivered(FoodDelivered fd)
	{
		if (fd.FoodTag == "Team1Resource")
		{
			Team1Score++;
			if (Team1Score >= 2)
			{
				EventManager.Instance.TriggerEvent(new GameEnd(1, fd.Food.transform, GameWinType.FoodWin));
			}
		}
		else
		{
			Team2Score++;
			if (Team2Score >= 2)
			{
				EventManager.Instance.TriggerEvent(new GameEnd(2, fd.Food.transform, GameWinType.FoodWin));
			}
		}
	}
}
