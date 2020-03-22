using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;
using UnityEngine.SceneManagement;

public class NetworkManagerBirfia : NetworkManager
{
    public GameObject[] PlayerPrefabs;
    public string Name;
    public Dictionary<int, int> PlayerSelection = new Dictionary<int, int>();
    public override void ServerChangeScene(string newSceneName)
    {
        base.ServerChangeScene(newSceneName);
    }

    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        if (SceneManager.GetActiveScene().name == "OnlineMenu")
        {
            base.OnServerAddPlayer(conn);
        }
        else
        {
            Transform startPos = GetStartPosition();
            GameObject player = Instantiate(PlayerPrefabs[PlayerSelection[conn.connectionId]], startPos.position, startPos.rotation);
            NetworkServer.AddPlayerForConnection(conn, player);
        }
    }
}
