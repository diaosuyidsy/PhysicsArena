using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRespawn : MonoBehaviour
{
    private void OnTriggerEnter (Collider other)
    {
        if (other.CompareTag ("Team1") || other.CompareTag ("Team2"))
        {
            other.SendMessageUpwards ("OnEnterDeathZone");
        }
    }
}
