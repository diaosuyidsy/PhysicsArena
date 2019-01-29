using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManagerStart : MonoBehaviour
{
    public static GameManagerStart GMS;
    public GameObject WinningObj;
    public GameObject Payload;
    public GameObject Or;
    public GameObject Food;
    public GameObject PressAToSpawn;

    private void Awake()
    {
        GMS = this;
    }

    private IEnumerator PlayIntro()
    {
        // Inside this if Block is the all tutorial content
        // Could only run once
        if (PlayerPrefs.GetInt("TutorialDone") == 0)
        {
            PlayerPrefs.SetInt("TutorialDone", 1);
            yield return new WaitForSeconds(1f);

            WinningObj.SetActive(true);

            yield return new WaitForSeconds(5f);

            Payload.SetActive(true);

            yield return new WaitForSeconds(7f);

            Or.SetActive(true);

            yield return new WaitForSeconds(3f);

            Food.SetActive(true);

            yield return new WaitForSeconds(3f);
        }

        PressAToSpawn.SetActive(true);

        //SceneManager.LoadSceneAsync(1);
        GameManager.GM.FillPlayerInformation();
        GameManager.GM.GetComponent<InputController>().enabled = true;
    }

    public void StartIntro()
    {
        StartCoroutine(PlayIntro());
    }
}
