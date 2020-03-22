using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;

public class NetworkMenuGameState : NetworkBehaviour
{
    public TextMeshPro PromptText;
    public string BrawlModeName = "BrawlModeReforgedMirror";

    private int TotalConnectionCount
    {
        get
        {
            return NetworkServer.connections.Count;
        }
    }
    public static NetworkMenuGameState instance;
    private bool[] selectedCharacter;
    private IEnumerator _startGameCoroutine;

    private void Awake()
    {
        instance = this;
        selectedCharacter = new bool[6];
    }

    public void ConfirmSelection(NetworkConnection connection, int selectedCharacterIndex, bool select)
    {
        selectedCharacter[selectedCharacterIndex] = select;
        if (select)
            (NetworkManager.singleton as NetworkManagerBirfia).PlayerSelection.Add(connection.connectionId, selectedCharacterIndex);
        else
            (NetworkManager.singleton as NetworkManagerBirfia).PlayerSelection.Remove(connection.connectionId);
        if (!select && _startGameCoroutine != null)
            StopCoroutine(_startGameCoroutine);

        int team1Num = 0;
        int team2Num = 0;
        for (int i = 0; i < selectedCharacter.Length; i++)
        {
            if (i < 3 && selectedCharacter[i])
                team1Num++;
            if (i > 2 && selectedCharacter[i])
                team2Num++;
        }
        int totalNum = team1Num + team2Num;
        if (totalNum == 1)
            RpcChangeText("Need more players");
        else if (totalNum > 1 && (team1Num == 0 || team2Num == 0))
            RpcChangeText("Need players on both team");
        else if (TotalConnectionCount > totalNum)
            RpcChangeText("Everybody needs to select a character!");
        else if (TotalConnectionCount == totalNum && team1Num > 0 && team2Num > 0)
        {
            RpcChangeText("Game Starting");
            _startGameCoroutine = startingGame();
            StartCoroutine(_startGameCoroutine);
        }
        else
        {
            RpcChangeText("Select Character");
        }
        // GameStart();
    }

    IEnumerator startingGame()
    {
        for (int i = 5; i > 0; i--)
        {
            yield return new WaitForSeconds(1f);
            RpcChangeText(i.ToString());
        }
        GameStart();
    }

    private void GameStart()
    {
        NetworkManager.singleton.ServerChangeScene(BrawlModeName);
    }

    [ClientRpc]
    private void RpcChangeText(string text)
    {
        PromptText.text = text;
    }
}
