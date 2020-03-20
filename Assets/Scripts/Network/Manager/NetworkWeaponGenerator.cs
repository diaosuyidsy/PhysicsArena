using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NetworkWeaponGenerator : NetworkBehaviour
{
    public GameObject FistGun;
    public override void OnStartServer()
    {
        GameObject fistgun = Instantiate(FistGun, transform.position, Quaternion.identity);
        NetworkServer.Spawn(fistgun);
    }

}
