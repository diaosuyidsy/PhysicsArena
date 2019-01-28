using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This Class is for the entire 2Dstartscene progress
public class MenuController : MonoBehaviour
{
    public GameObject CanvasManager;

    private enum State
    {
        OnStartImage,
        OnCharacterSelection,
        OnLoading,
    }

    private State MenuState;

    // Use this for initialization
    void Start()
    {
        CanvasManager.SetActive(false);
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
    }
}
