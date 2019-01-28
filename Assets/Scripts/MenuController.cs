using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
using UnityEngine.SceneManagement;

// This Class is for the entire 2Dstartscene progress
public class MenuController : MonoBehaviour
{
    public static MenuController MC;
    public GameObject[] StartImagePhaseObjects;
    public GameObject[] CharacterSelectionPhaseObjects;
    public GameObject[] OnLoadingPhaseObjects;

    private enum State
    {
        OnStartImage,
        OnCharacterSelection,
        OnLoading,
    }

    private State MenuState;

    private void Awake()
    {
        MC = this;
    }
    // Use this for initialization
    void Start()
    {
        foreach (GameObject go in StartImagePhaseObjects)
        {
            go.SetActive(true);
        }
        foreach (GameObject go in CharacterSelectionPhaseObjects)
        {
            go.SetActive(false);
        }
        //foreach (GameObject go in OnLoadingPhaseObjects)
        //{
        //    go.SetActive(false);
        //}
        MenuState = State.OnStartImage;
    }

    // Update is called once per frame
    void Update()
    {
        _checkSelectionInput();
    }

    private void _checkSelectionInput()
    {
        if (MenuState != State.OnStartImage) return;
        for (int i = 0; i < 6; i++)
        {
            if (ReInput.players.GetPlayer(i).GetButtonDown("Jump"))
            {
                // Now we are on the phase of CharacterSelection
                // Initialize as such
                MenuState = State.OnCharacterSelection;
                foreach (GameObject go in CharacterSelectionPhaseObjects)
                {
                    go.SetActive(true);
                }
                foreach (GameObject go in StartImagePhaseObjects)
                {
                    go.SetActive(false);
                }
            }
        }
    }

    /// <summary>
    /// Once Called from outside, change to loading phase
    /// </summary>
    public void ChangeToLoadingState()
    {
        MenuState = State.OnLoading;
        // Deactivate other stuff, other than the first child, CanvasController
        // Which should always be there
        for (int i = 1; i < CharacterSelectionPhaseObjects.Length; i++)
        {
            CharacterSelectionPhaseObjects[i].SetActive(false);
        }
        foreach (GameObject go in OnLoadingPhaseObjects)
        {
            go.SetActive(true);
            go.GetComponentInChildren<CountDownVideoPlayer>().PlayTheVideo();
        }
        StartCoroutine(_startLoadingScene(5f));
    }

    public void LoadingEnd()
    {
        SceneManager.LoadSceneAsync(1);
    }

    IEnumerator _startLoadingScene(float time)
    {
        yield return new WaitForSeconds(time);
        //SceneManager.LoadSceneAsync(1);
        foreach (GameObject go in OnLoadingPhaseObjects)
        {
            go.SetActive(false);
        }
        GameManagerStart.GMS.StartIntro();
    }
}
