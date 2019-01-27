﻿using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Rewired;
using UnityEngine.SceneManagement;

public class CanvasController : MonoBehaviour
{
    public static CanvasController CC = null;
    public RectTransform[] Characters;
    public GameObject[] GreyCharacters;
    public GameObject[] PureGreyBackgrounds;
    public GameObject[] PlayerSlots;
    [Header("Character Information: Align with Image")]
    public Color[] CharacterColors;
    public string[] CharacterNames;
    public GameObject StartCountDown;
    // Final Information is what is kept to the playing scene
    [HideInInspector]
    public PlayerInformation[] FinalInformation;
    [HideInInspector]
    public List<bool> CharacterSlotsTaken;
    [HideInInspector]
    public int[] PlayerHoveringSlots;
    [HideInInspector]
    public bool[] PlayersLockedIn;

    private Player[] _players;
    private bool[] _playersMoveCharged;
    private int _playersLockedInCount = 0;
    // Use this for initialization
    private void Awake()
    {
        if (CC == null)
            CC = this;
        DontDestroyOnLoad(gameObject);
        CharacterSlotsTaken = new List<bool>(new bool[] { false, false, false, false, false, false });
        _playersMoveCharged = new bool[] { true, true, true, true, true, true };
        PlayerHoveringSlots = new int[] { -1, -1, -1, -1, -1, -1 };
        FinalInformation = new PlayerInformation[6];
        for (int i = 0; i < 6; i++) FinalInformation[i] = new PlayerInformation();
        PlayersLockedIn = new bool[6];
        _players = new Player[6];

    }

    private void Start()
    {
        for (int i = 0; i < 6; i++)
        {
            _players[i] = ReInput.players.GetPlayer(i);
        }
        StartCountDown.SetActive(false);
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
        //ConsoleProDebug.Watch("Player 0's Selection Information", FinalInformation[0].PlayerName + " , " + FinalInformation[0].PlayerColor.ToString());
        //ConsoleProDebug.Watch("Player 1's Selection Information", FinalInformation[1].PlayerName + " , " + FinalInformation[1].PlayerColor.ToString());
        //ConsoleProDebug.Watch("Player 2's Selection Information", FinalInformation[2].PlayerName + " , " + FinalInformation[2].PlayerColor.ToString());
        //ConsoleProDebug.Watch("Player 3's Selection Information", FinalInformation[3].PlayerName + " , " + FinalInformation[3].PlayerColor.ToString());
        //ConsoleProDebug.Watch("Player 4's Selection Information", FinalInformation[4].PlayerName + " , " + FinalInformation[4].PlayerColor.ToString());
        //ConsoleProDebug.Watch("Player 5's Selection Information", FinalInformation[5].PlayerName + " , " + FinalInformation[5].PlayerColor.ToString());
        //ConsoleProDebug.Watch("Max Joysticks Count", ReInput.controllers.joystickCount.ToString());
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
                GreyCharacters[curPlayerSelectionIndex].SetActive(false);
                // Record the information
                RecordSelectionInformation(playernumber, curPlayerSelectionIndex);
                // Lock the Player so they cannot make anymore choices
                PlayersLockedIn[playernumber] = true;
                // Add to player selection count
                OnPlayerSelection();
            }
        }
    }

    private void OnPlayerSelection()
    {
        _playersLockedInCount++;
        if (_playersLockedInCount >= ReInput.controllers.joystickCount)
        {
            // Meaning We are ready to start
            StartCountDown.SetActive(true);
            StartCoroutine(ChangeNumber());
        }
    }

    IEnumerator ChangeNumber()
    {
        yield return new WaitForSeconds(1f);
        foreach (var text in StartCountDown.GetComponentsInChildren<Text>())
        {
            text.text = "2";
        }
        yield return new WaitForSeconds(1f);
        foreach (var text in StartCountDown.GetComponentsInChildren<Text>())
        {
            text.text = "1";
        }
        yield return new WaitForSeconds(1f);
        foreach (var text in StartCountDown.GetComponentsInChildren<Text>())
        {
            text.text = "0";
        }
        yield return new WaitForSeconds(1f);
        StartCountDown.SetActive(false);
        SceneManager.LoadScene(1);
    }

    private void RecordSelectionInformation(int playerNumber, int characterSlotNumber)
    {
        FinalInformation[playerNumber].PlayerName = CharacterNames[characterSlotNumber];
        FinalInformation[playerNumber].PlayerColor = CharacterColors[characterSlotNumber];
    }

    // This function checks player input
    // Acts accordingly against player input
    private void CheckInput(int playernumber)
    {
        float horizontal = _players[playernumber].GetAxis("Move Horizontal");
        //float vertical = _players[playernumber].GetAxis("Move Vertical");
        // If player Moved Horizontally then do something
        if ((Mathf.Abs(horizontal) - 0.9f > 0f) && _playersMoveCharged[playernumber])
        {
            _playersMoveCharged[playernumber] = false;
            // Change Grey Background Image
            if (!_isOtherPlayerHoveringHere(playernumber, PlayerHoveringSlots[playernumber]))
                PureGreyBackgrounds[PlayerHoveringSlots[playernumber]].SetActive(true);
            //PlayerHoveringSlots[playernumber] = nmod(PlayerHoveringSlots[playernumber] + (horizontal > 0f ? 1 : -1), 6);
            PlayerHoveringSlots[playernumber] = _advancePlace(PlayerHoveringSlots[playernumber], (horizontal > 0f ? 1 : -1));
            PureGreyBackgrounds[PlayerHoveringSlots[playernumber]].SetActive(false);

            _changeMember(PlayerHoveringSlots[playernumber], playernumber);
        }
        else if (Mathf.Approximately(horizontal, 0f))
        {
            _playersMoveCharged[playernumber] = true;
        }
    }

    private bool _isOtherPlayerHoveringHere(int thisPlayerNumber, int slotIndex)
    {
        for (int i = 0; i < 6; i++)
        {
            if (i != thisPlayerNumber && PlayerHoveringSlots[i] == slotIndex)
            {
                return true;
            }
        }
        return false;
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

    private int _advancePlace(int curPlace, int advanceAmount)
    {
        int nextPlace = 0;
        for (int i = 0; i < 6; i++)
        {
            nextPlace = nmod(curPlace + advanceAmount, 6);
            if (!CharacterSlotsTaken[nextPlace]) return nextPlace;
            else curPlace = nextPlace;
        }
        return nextPlace;
    }

    private int nmod(int x, int m)
    {
        return (x % m + m) % m;
    }

    private int truncate(int a, int b)
    {
        if (a + b > 5) return a;
        if (a + b < 0) return a;
        return a + b;
    }
}
