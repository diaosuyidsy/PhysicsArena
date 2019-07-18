using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceCollector : MonoBehaviour
{
	public TeamNum Team;
	private GameObject[] Team1ResourceSpawnPt;
	private GameObject[] Team2ResourceSpawnPt;

	//private int TeamTracker;
	private int TeamResourceSpawnIndex;

	private void Start()
	{
		Team1ResourceSpawnPt = GameManager.GM.Team1ResourceRespawnPoints;
		Team2ResourceSpawnPt = GameManager.GM.Team2ResrouceRespawnPoints;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (Team == TeamNum.Team1)
		{
			if (other.CompareTag("Team1Resource"))
			{
				//TeamTracker++;
				other.tag = "Untagged";
				other.gameObject.layer = LayerMask.NameToLayer("Defualt");
				//print("Team 1 Score = " + TeamTracker);
				// Statistics: Add the dropper to the stats record
				//int lastholder = other.GetComponent<rtBirdFood>().LastHolder;
				//if (GameManager.GM.FoodScoreTimes.Count > lastholder)
				//{
				//	GameManager.GM.FoodScoreTimes[lastholder]++;
				//}
				//else
				//{
				//	Debug.LogError("There is something wrong with the food collector statistics");
				//}
				// Statistics End
				EventManager.Instance.TriggerEvent(new FoodDelivered(other.gameObject, "Team1Resource", other.GetComponent<rtBirdFood>().LastHolder));
				//if (TeamTracker == 2)
				//{
				//	//GameManager.GM.GameOver(1, gameObject);
				//	//Camera.main.GetComponent<CameraController>().OnWinCameraZoom(transform);
				//	//ArenaScoreboard.instance.EndGame(gameObject);
				//}
			}
			else if (other.CompareTag("Team2Resource"))
			{
				EventManager.Instance.TriggerEvent(new ObjectDespawned(other.gameObject));
				GameManager.GM.Team2ResourceSpawnIndex = (GameManager.GM.Team2ResourceSpawnIndex + 1) % Team2ResourceSpawnPt.Length;
				other.transform.position = Team2ResourceSpawnPt[GameManager.GM.Team2ResourceSpawnIndex].transform.position;
			}
		}
		else
		{
			if (other.CompareTag("Team2Resource"))
			{
				//TeamTracker++;
				other.tag = "Untagged";
				other.gameObject.layer = LayerMask.NameToLayer("Defualt");
				//print("Team 2 Score = " + TeamTracker);
				// Statistics: Add the dropper to the stats record
				//int lastholder = other.GetComponent<rtBirdFood>().LastHolder;
				//if (GameManager.GM.FoodScoreTimes.Count > lastholder)
				//{
				//	GameManager.GM.FoodScoreTimes[lastholder]++;
				//}
				//else
				//{
				//	Debug.LogError("There is something wrong with the food collector statistics");
				//}
				// Statistics End
				EventManager.Instance.TriggerEvent(new FoodDelivered(other.gameObject, "Team2Resource", other.GetComponent<rtBirdFood>().LastHolder));

				//if (TeamTracker == 2)
				//{
				//	//GameManager.GM.GameOver(2, gameObject);
				//	//Camera.main.GetComponent<CameraController>().OnWinCameraZoom(transform);
				//	ArenaScoreboard.instance.EndGame(gameObject);
				//}

			}
			else if (other.CompareTag("Team1Resource"))
			{
				EventManager.Instance.TriggerEvent(new ObjectDespawned(other.gameObject));
				GameManager.GM.Team1ResourceSpawnIndex = (GameManager.GM.Team1ResourceSpawnIndex + 1) % Team1ResourceSpawnPt.Length;
				other.transform.position = Team1ResourceSpawnPt[GameManager.GM.Team1ResourceSpawnIndex].transform.position;
			}
		}


	}
}
