using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ComicMenu : MonoBehaviour
{
    private FSM<ComicMenu> ComicMenuFSM;

    private void Awake()
    {
        ComicMenuFSM = new FSM<ComicMenu>(this);

    }


    // Update is called once per frame
    void Update()
    {
        ComicMenuFSM.Update();
    }

    private abstract class MenuState : FSM<ComicMenu>.State
    { }

    private class FirstMenuState : MenuState
    {

    }
}
