using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarModeObjectiveManager : MonoBehaviour
{
	private int Team1Score;
	private int Team2Score;

	private void _onFoodDelivered(FoodDelivered fd)
	{
		if (fd.FoodTag == "Team1Resource")
		{
			Team1Score++;
			if (Team1Score >= 2)
			{
				//GameManager.GM.GameOver(1, fd.Food);
				EventManager.Instance.TriggerEvent(new GameEnd(1, fd.Food.transform));
				//Camera.main.GetComponent<CameraController>().OnWinCameraZoom(fd.Food.transform);
			}
		}
		else
		{
			Team2Score++;
			if (Team2Score >= 2)
			{
				//GameManager.GM.GameOver(2, fd.Food);
				EventManager.Instance.TriggerEvent(new GameEnd(2, fd.Food.transform));
				//Camera.main.GetComponent<CameraController>().OnWinCameraZoom(fd.Food.transform);
			}
		}
	}

	private void OnEnable()
	{
		EventManager.Instance.AddHandler<FoodDelivered>(_onFoodDelivered);
	}

	private void OnDisable()
	{
		EventManager.Instance.RemoveHandler<FoodDelivered>(_onFoodDelivered);
	}
}
