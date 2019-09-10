using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NetworkManagerBrawlMode : NetworkManager
{
    public GameObject[] PlayerPrefabs;

    public override void OnServerAddPlayer(NetworkConnection conn, AddPlayerMessage extraMessage)
    {
        if (numPlayers > 6) return;
        Transform startPos = GetStartPosition();
        GameObject player = startPos != null ? Instantiate(PlayerPrefabs[numPlayers], startPos.position, startPos.rotation) : Instantiate(PlayerPrefabs[numPlayers]);
        NetworkServer.AddPlayerForConnection(conn, player);
    }
}
