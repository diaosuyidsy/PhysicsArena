using System.Collections.Generic;
using UnityEngine;
using Rewired;

public class CanvasController : MonoBehaviour
{
    [HideInInspector]
    public List<bool> CharacterSlots;
    [HideInInspector]
    public int[] PlayerHoveringSlots;

    private Player[] _players;
    private bool[] _playersMoveCharged;
    // Use this for initialization
    private void Awake()
    {
        CharacterSlots = new List<bool>(new bool[] { false, false, false, false, false, false });
        _playersMoveCharged = new bool[] { true, true, true, true, true, true };
        PlayerHoveringSlots = new int[] { 0, 0, 0, 0, 0, 0 };
        _players = new Player[6];
        for (int i = 0; i < 6; i++)
        {
            _players[i] = ReInput.players.GetPlayer(i);
        }
    }

    private void Update()
    {
        for (int i = 0; i < 6; i++)
        {
            CheckInput(i);
        }
    }

    // This function checks player input
    // Acts accordingly against player input
    private void CheckInput(int playernumber)
    {
        float horizontal = _players[playernumber].GetAxis("Move Horizontal");
        float vertical = _players[playernumber].GetAxis("Move Vertical");
        // If player Moved Horizontally then do something
        if ((Mathf.Abs(horizontal) - 0.9f > 0f) && _playersMoveCharged[playernumber])
        {
            _playersMoveCharged[playernumber] = false;
            PlayerHoveringSlots[playernumber] = truncate(PlayerHoveringSlots[playernumber], (horizontal > 0f ? 1 : -1));
        }
        else if (Mathf.Approximately(horizontal, 0f))
        {
            _playersMoveCharged[playernumber] = true;
        }

        // If player moved vertically then do something
        if ((Mathf.Abs(vertical) - 0.9f > 0f) && _playersMoveCharged[playernumber])
        {
            _playersMoveCharged[playernumber] = false;
            PlayerHoveringSlots[playernumber] = truncate(PlayerHoveringSlots[playernumber], (vertical > 0f ? 3 : -3));
        }
        else if (Mathf.Approximately(vertical, 0f))
        {
            _playersMoveCharged[playernumber] = true;
        }
    }

    private int truncate(int a, int b)
    {
        if (a + b > 5) return a;
        if (a + b < 0) return a;
        return a + b;
    }
}
