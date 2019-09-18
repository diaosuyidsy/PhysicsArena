using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeRespawnPoint : MonoBehaviour
{
    private SushiModeObjectiveManager _s;

    private void Awake()
    {
        _s = Services.GameObjectiveManager as SushiModeObjectiveManager;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag.Contains("Team"))
        {
            // It is a player that gets in the trigger
            // Change the Respawn Point of this specific Player
            int colorindex = 0;
            for (int j = 0; j < Services.GameStateManager.PlayersInformation.RewiredID.Length; j++)
            {
                if (other.GetComponent<PlayerController>().PlayerNumber == Services.GameStateManager.PlayersInformation.RewiredID[j]) colorindex = Services.GameStateManager.PlayersInformation.ColorIndex[j];
            }
            _s.ChangeRespawnPoint(colorindex, transform.position, other.tag);
        }
    }
}
