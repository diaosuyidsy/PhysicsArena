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
                // Add Delivery Food VFX
                Instantiate (VisualEffectManager.VEM.DeliverFoodVFX, other.transform.position, VisualEffectManager.VEM.DeliverFoodVFX.transform.rotation);
                // END Add
                if (TeamTracker == 1)
                {
                    GameManager.GM.GameOver (1, gameObject);
                    Camera.main.GetComponent<CameraController> ().OnWinCameraZoom (transform);
                }
            }
            else if (other.CompareTag ("Team2Resource"))
            {
                // Add Vanish VFX
                Instantiate (VisualEffectManager.VEM.VanishVFX, other.transform.position, VisualEffectManager.VEM.VanishVFX.transform.rotation);
                // END ADD
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
                // Add Delivery Food VFX
                Instantiate (VisualEffectManager.VEM.DeliverFoodVFX, other.transform.position, VisualEffectManager.VEM.DeliverFoodVFX.transform.rotation);
                // END Add
                if (TeamTracker == 1)
                {
                    GameManager.GM.GameOver (2, gameObject);
                    Camera.main.GetComponent<CameraController> ().OnWinCameraZoom (transform);
                }

            }
            else if (other.CompareTag ("Team1Resource"))
            {
                // Add Vanish VFX
                Instantiate (VisualEffectManager.VEM.VanishVFX, other.transform.position, VisualEffectManager.VEM.VanishVFX.transform.rotation);
                // END ADD
                GameManager.GM.Team1ResourceSpawnIndex = (GameManager.GM.Team1ResourceSpawnIndex + 1) % Team1ResourceSpawnPt.Length;
                other.transform.position = Team1ResourceSpawnPt[GameManager.GM.Team1ResourceSpawnIndex].transform.position;
            }
        }


    }
}
