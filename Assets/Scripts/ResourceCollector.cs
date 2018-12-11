using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceCollector : MonoBehaviour
{
    public TeamNum Team;
    private GameObject[] Team1ResourceSpawnPt;
    private GameObject[] Team2ResourceSpawnPt;

    private int TeamTracker;
    private int TeamResourceSpawnIndex;

    private void Start ()
    {
        Team1ResourceSpawnPt = GameManager.GM.Team1ResourceRespawnPoints;
        Team2ResourceSpawnPt = GameManager.GM.Team2ResrouceRespawnPoints;
    }

    private void OnTriggerEnter (Collider other)
    {
        if (Team == TeamNum.Team1)
        {
            if (other.CompareTag ("Team1Resource"))
            {
                TeamTracker++;
                other.tag = "Untagged";
                print ("Team 1 Score = " + TeamTracker);
                if (TeamTracker == 3)
                    GameManager.GM.GameOver (1);
            }
            else if (other.CompareTag ("Team2Resource"))
            {
                GameManager.GM.Team2ResourceSpawnIndex = (GameManager.GM.Team2ResourceSpawnIndex + 1) % Team2ResourceSpawnPt.Length;
                other.transform.position = Team2ResourceSpawnPt[GameManager.GM.Team2ResourceSpawnIndex].transform.position;
            }
        }
        else
        {
            if (other.CompareTag ("Team2Resource"))
            {
                TeamTracker++;
                other.tag = "Untagged";
                print ("Team 2 Score = " + TeamTracker);
                if (TeamTracker == 3)
                    GameManager.GM.GameOver (2);
            }
            else if (other.CompareTag ("Team1Resource"))
            {
                GameManager.GM.Team1ResourceSpawnIndex = (GameManager.GM.Team1ResourceSpawnIndex + 1) % Team1ResourceSpawnPt.Length;
                other.transform.position = Team1ResourceSpawnPt[GameManager.GM.Team1ResourceSpawnIndex].transform.position;
            }
        }


    }
}
