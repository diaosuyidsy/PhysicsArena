using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NetworkWeaponGenerator : NetworkBehaviour
{
    public GameObject[] TestWeapons;
    public override void OnStartServer()
    {
        for (int i = 0; i < TestWeapons.Length; i++)
        {
            GameObject weapon = Instantiate(TestWeapons[i], transform.position, Quaternion.identity);
            NetworkServer.Spawn(weapon);
        }

    }

}
