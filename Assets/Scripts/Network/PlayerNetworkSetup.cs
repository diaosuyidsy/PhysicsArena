using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerNetworkSetup : NetworkBehaviour
{
    public override void OnStartLocalPlayer()
    {
        GetComponent<PlayerControllerNetworking>().enabled = true;
    }
}
