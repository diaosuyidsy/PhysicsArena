using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;

public class InputController : MonoBehaviour
{
    private Player[] _players;
    private bool[] _enteredGame;
    private int playerNum = 0;

    private void Awake()
    {
        _players = new Player[6];
        _enteredGame = new bool[6];
        for (int i = 0; i < 6; i++)
        {
            _players[i] = ReInput.players.GetPlayer(i);
        }
    }

    private void Update()
    {
        for (int i = 0; i < 6; i++)
        {
            if (!_enteredGame[i] && _players[i].GetButton("Jump"))
            {
                _enteredGame[i] = true;
                EnterGame(i);
            }
        }
    }

    private void EnterGame(int playerID)
    {
        GameObject curPlayer = GameManager.GM.APlayers[playerNum];
        curPlayer.GetComponent<PlayerController>().Init(playerID);
        curPlayer.transform.parent.parent.gameObject.SetActive(true);
        if (GameManager.GM.Players.Count <= playerNum) GameManager.GM.Players.Add(curPlayer);
        GameManager.GM.Players[playerNum] = curPlayer;
        playerNum++;
    }
}
