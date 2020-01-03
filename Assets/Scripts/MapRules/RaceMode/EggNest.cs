﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EggNest : MonoBehaviour
{
    private bool[] Eggs;
    private SushiModeObjectiveManager _s;
    private void Awake()
    {
        Eggs = new bool[6];
        _s = Services.GameObjectiveManager as SushiModeObjectiveManager;
        EventManager.Instance.AddHandler<CollectEgg>(OnCollectEgg);
    }

    private void OnDisable()
    {
        EventManager.Instance.RemoveHandler<CollectEgg>(OnCollectEgg);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag.Contains("Team"))
        {
            int colorindex = 0;
            for (int j = 0; j < Services.GameStateManager.PlayersInformation.RewiredID.Length; j++)
            {
                if (other.GetComponent<PlayerController>().PlayerNumber == Services.GameStateManager.PlayersInformation.RewiredID[j]) colorindex = Services.GameStateManager.PlayersInformation.ColorIndex[j];
            }
            if (!Eggs[colorindex] && _s.PickUpEgg(colorindex))
            {
                Eggs[colorindex] = true;
                // Pick up the Egg as UI
                transform.parent.GetChild(1).GetChild(colorindex).GetComponent<Renderer>().enabled = false;
            }
        }
    }

    private void OnCollectEgg(CollectEgg ev)
    {
        Eggs[ev.ColorIndex] = false;
        transform.parent.GetChild(1).GetChild(ev.ColorIndex).GetComponent<Renderer>().enabled = true;
    }
}