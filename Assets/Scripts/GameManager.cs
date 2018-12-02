using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{

    public static GameManager GM;
    public Text Result;
    public GameObject[] Players;
    public GameObject RunSandVFX;
    public LayerMask AllPlayers;
    public GameObject[] Team1RespawnPts;
    public GameObject[] Team2RespawnPts;
    public float RespawnTime = 5f;

    private int Team1RespawnIndex = 0;
    private int Team2RespawnIndex = 0;

    private void Awake ()
    {
        GM = this;
    }

    public void SetToRespawn (GameObject player)
    {
        if (player.CompareTag ("Team1"))
        {
            Team1RespawnIndex = (Team1RespawnIndex + 1) % Team1RespawnPts.Length;
            player.transform.position = Team1RespawnPts[Team1RespawnIndex].transform.position;
        }
        else
        {
            Team2RespawnIndex = (Team2RespawnIndex + 1) % Team2RespawnPts.Length;
            player.transform.position = Team2RespawnPts[Team2RespawnIndex].transform.position;
        }
    }

    public void GameOver (int winner)
    {
        Result.text = "TEAM " + (winner == 1 ? "ONE" : "TWO") + " VICTORY";
        Result.transform.parent.gameObject.SetActive (true);
    }

    private void Update ()
    {
        CheckRestart ();
    }

    void CheckRestart ()
    {

#if UNITY_EDITOR_OSX
        if (Input.GetKeyDown(KeyCode.Joystick1Button10))
        {
            SceneManager.LoadScene("MasterScene");
        }

#endif
#if UNITY_EDITOR_WIN
        if (Input.GetKeyDown (KeyCode.Joystick1Button6))
        {
            SceneManager.LoadScene ("MasterScene");
        }
#endif

    }
}
