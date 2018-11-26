using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{

    public static GameManager GM;
    public Text Result;
    public GameObject[] Players;

    private void Awake ()
    {
        GM = this;
    }

    public void GameOver (int winner)
    {
        Result.text = "TEAM " + (winner == 1 ? "ONE" : "TWO") + " VICTORY";
        Result.transform.parent.gameObject.SetActive (true);
    }
}
