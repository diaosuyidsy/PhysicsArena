using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;
using UnityEngine.SceneManagement;
using System.Linq;

public class NetworkManagerBirfia : NetworkManager
{
    public GameObject[] PlayerPrefabs;
    public string Name;
    // Key Value Pair: ConnectionID, ColorIndex
    public Dictionary<int, int> PlayerSelection = new Dictionary<int, int>();
    public Dictionary<int, string> PlayerNames = new Dictionary<int, string>();
    public Dictionary<int, bool> PlayerReady = new Dictionary<int, bool>();
    public override void ServerChangeScene(string newSceneName)
    {
        base.ServerChangeScene(newSceneName);
        if (newSceneName == "OnlineMenu")
        {
            PlayerSelection.Clear();
            foreach (int readyKey in PlayerReady.Keys.ToList())
            {
                PlayerReady[readyKey] = false;
            }
        }
    }

    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        if (SceneManager.GetActiveScene().name == "OnlineMenu")
        {
            base.OnServerAddPlayer(conn);
        }
        else
        {
            if (conn.identity != null) return;
            Transform startPos = GetStartPosition();
            GameObject player = Instantiate(PlayerPrefabs[PlayerSelection[conn.connectionId]], startPos.position, startPos.rotation);
            // 6 means spectator,
            // otherwise a player
            if (PlayerSelection[conn.connectionId] != 6)
            {
                player.GetComponent<PlayerControllerMirror>().PlayerName = PlayerNames[conn.connectionId];
                player.transform.parent = GameObject.Find("Players").transform;
            }
            NetworkServer.AddPlayerForConnection(conn, player);
        }
    }

    public override void OnServerReady(NetworkConnection conn)
    {
        base.OnServerReady(conn);
        if (SceneManager.GetActiveScene().name == "OnlineMenu") return;
        PlayerReady[conn.connectionId] = true;
        bool allready = true;
        foreach (var kvpair in PlayerReady)
        {
            if (!kvpair.Value)
                allready = false;
        }
        if (allready)
        {
            GameObject.Find("NetworkGameManager").GetComponent<NetworkGameStateManager>().OnStart();
            for (int i = 0; i < PlayerReady.Count; i++)
            {
                var item = PlayerReady.ElementAt(i);
                PlayerReady[item.Key] = false;
            }
        }
    }
}
