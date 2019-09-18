using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnTriggerWin : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag.Contains("Team"))
        {
            SushiModeObjectiveManager sushimodeobjectivemanager = Services.GameObjectiveManager as SushiModeObjectiveManager;
            if (other.CompareTag("Team1"))
            {
                sushimodeobjectivemanager.OnWin(1, transform);
            }
            else
            {
                sushimodeobjectivemanager.OnWin(2, transform);
            }
        }

    }
}
