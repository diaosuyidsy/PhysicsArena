using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Rewired;

public class CanvasController : MonoBehaviour
{
    public RectTransform[] Characters;
    public GameObject[] CharactersInTeam;
    public GameObject[] PlayerSlots;
    [HideInInspector]
    public List<bool> CharacterSlotsTaken;
    [HideInInspector]
    public int[] PlayerHoveringSlots;
    [HideInInspector]
    public bool[] PlayersLockedIn;

    private Player[] _players;
    private bool[] _playersMoveCharged;
    // Use this for initialization
    private void Awake()
    {
        CharacterSlotsTaken = new List<bool>(new bool[] { false, false, false, false, false, false });
        _playersMoveCharged = new bool[] { true, true, true, true, true, true };
        PlayerHoveringSlots = new int[] { 0, 0, 0, 0, 0, 0 };
        PlayersLockedIn = new bool[6];
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
            if (PlayersLockedIn[i])
                continue;
            CheckInput(i);
            CheckSelectionInput(i);
        }
    }

    private void CheckSelectionInput(int playernumber)
    {
        if (_players[playernumber].GetButtonDown("Jump"))
        {
            int curPlayerSelectionIndex = PlayerHoveringSlots[playernumber];
            // If this slot is still available
            if (!CharacterSlotsTaken[curPlayerSelectionIndex])
            {
                // Take up the slot
                CharacterSlotsTaken[curPlayerSelectionIndex] = true;
                // Select the thing
                CharactersInTeam[curPlayerSelectionIndex].GetComponentInChildren<Image>().color = Color.white;
                // Lock the Player so they cannot make anymore choices
                PlayersLockedIn[playernumber] = true;
            }
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
            _changeMember(PlayerHoveringSlots[playernumber], playernumber);
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
            _changeMember(PlayerHoveringSlots[playernumber], playernumber);
        }
        else if (Mathf.Approximately(vertical, 0f))
        {
            _playersMoveCharged[playernumber] = true;
        }
    }

    private void OnHover(int playernumber)
    {
        int curPlayerSelectionIndex = PlayerHoveringSlots[playernumber];
        if (!CharacterSlotsTaken[curPlayerSelectionIndex])
        {

        }
    }

    // This function takes 3 arguements
    // Which Slot to change, which player to change, and whether it is an addition or subtraction
    private void _changeMember(int slotindex, int playernumber)
    {
        PlayerSlots[playernumber].SetActive(true);
        RadicalLayout rl = PlayerSlots[playernumber].transform.parent.GetComponent<RadicalLayout>();
        if (rl != null)
        {
            rl.StartAngle += 25f;
            rl.MaxAngle -= 50f;
        }

        PlayerSlots[playernumber].transform.SetParent(Characters[slotindex].transform, false);
        PlayerSlots[playernumber].transform.parent.GetComponent<RadicalLayout>().StartAngle -= 25f;
        PlayerSlots[playernumber].transform.parent.GetComponent<RadicalLayout>().MaxAngle += 50f;

    }

    private int truncate(int a, int b)
    {
        if (a + b > 5) return a;
        if (a + b < 0) return a;
        return a + b;
    }
}
