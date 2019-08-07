using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceCollector : MonoBehaviour
{
    public TeamNum Team;
    private Transform[] SpawnPoints;

    //private int TeamTracker;
    private int TeamResourceSpawnIndex;

    private void Awake()
    {
        Transform temp = transform.parent.Find("RespawnPoints");
        SpawnPoints = new Transform[temp.childCount];
        for (int i = 0; i < temp.childCount; i++)
        {
            SpawnPoints[i] = temp.GetChild(i);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (Team == TeamNum.Team1)
        {
            if (other.CompareTag("Team1Resource"))
            {
                //TeamTracker++;
                other.tag = "Untagged";
                other.gameObject.layer = LayerMask.NameToLayer("Default");
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
                EventManager.Instance.TriggerEvent(new ObjectDespawned(other.gameObject, gameObject));
                TeamResourceSpawnIndex = (TeamResourceSpawnIndex + 1) % SpawnPoints.Length;
                other.transform.position = SpawnPoints[TeamResourceSpawnIndex].transform.position;
            }
        }
        else
        {
            if (other.CompareTag("Team2Resource"))
            {
                //TeamTracker++;
                other.tag = "Untagged";
                other.gameObject.layer = LayerMask.NameToLayer("Default");
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
                EventManager.Instance.TriggerEvent(new ObjectDespawned(other.gameObject, gameObject));
                TeamResourceSpawnIndex = (TeamResourceSpawnIndex + 1) % SpawnPoints.Length;
                other.transform.position = SpawnPoints[TeamResourceSpawnIndex].transform.position;
            }
        }


    }
}
