using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class PlayerNetworkSetup : MonoBehaviourPun
{
    private void Awake()
    {
        if (!photonView.IsMine)
        {
            GetComponent<PlayerControllerNetworking>().enabled = false;
        }
    }
}
