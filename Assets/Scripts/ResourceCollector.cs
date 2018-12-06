using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceCollector : MonoBehaviour
{
	public TeamNum Team;
	public GameObject[] Team1ResourceSpawnPt;
	public GameObject[] Team2ResourceSpawnPt;
	
	private int TeamTracker;
	private int TeamResourceSpawnIndex;
	
	private void OnTriggerEnter(Collider other)
	{
		if (Team == TeamNum.Team1)
		{
			if (other.CompareTag("Team1Resource"))
			{
				TeamTracker++;
				print("Team 1 Score = " + TeamTracker);
			}		
			else if (other.CompareTag("Team2Resource"))
			{
				TeamResourceSpawnIndex = (TeamResourceSpawnIndex + 1) % Team2ResourceSpawnPt.Length;
				other.transform.position = Team2ResourceSpawnPt[TeamResourceSpawnIndex].transform.position;
			}
		}
		else
		{
			if (other.CompareTag("Team2Resource"))
			{
				TeamTracker++;
				print("Team 2 Score = " + TeamTracker);
			}		
			else if (other.CompareTag("Team1Resource"))
			{
				TeamResourceSpawnIndex = (TeamResourceSpawnIndex + 1) % Team1ResourceSpawnPt.Length;
				other.transform.position = Team1ResourceSpawnPt[TeamResourceSpawnIndex].transform.position;
			}
		}


	}
}
