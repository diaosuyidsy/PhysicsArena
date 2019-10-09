using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdNest : MonoBehaviour
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
            int colorindex = 0;
            for (int j = 0; j < Services.GameStateManager.PlayersInformation.RewiredID.Length; j++)
            {
                if (other.GetComponent<PlayerController>().PlayerNumber == Services.GameStateManager.PlayersInformation.RewiredID[j])
                    colorindex = Services.GameStateManager.PlayersInformation.ColorIndex[j];
            }
            _s.ChangeRespawnPoint(colorindex, transform.position, other.tag);
            _s.CollectEgg(colorindex);
            EventManager.Instance.TriggerEvent(new CollectEgg(colorindex));
        }
    }
}


public class CollectEgg : GameEvent
{
    public int ColorIndex;

    public CollectEgg(int colorIndex)
    {
        ColorIndex = colorIndex;
    }
}