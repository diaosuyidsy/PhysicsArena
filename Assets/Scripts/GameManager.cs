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

    private void Awake()
    {
        GM = this;
    }

    public void GameOver(int winner)
    {
        Result.text = "TEAM " + (winner == 1 ? "ONE" : "TWO") + " VICTORY";
        Result.transform.parent.gameObject.SetActive(true);
    }

    private void Update()
    {
        CheckRestart();
    }

    void CheckRestart()
    {

#if UNITY_EDITOR_OSX
        if (Input.GetKeyDown(KeyCode.Joystick1Button10))
        {
            SceneManager.LoadScene("MasterScene");
        }

#endif
#if UNITY_EDITOR_WIN
        if (Input.GetKeyDown(KeyCode.Joystick1Button6))
        {
            SceneManager.LoadScene("MasterScene");
        }
#endif

    }
}
