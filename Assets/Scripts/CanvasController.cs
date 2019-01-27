using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Rewired;

public class CanvasController : MonoBehaviour
{
    public static CanvasController CC = null;
    public RectTransform[] Characters;
    public GameObject[] GreyCharacters;
    public GameObject[] PlayerSlots;
    [Header("Character Information: Align with Image")]
    public Color[] CharacterColors;
    public string[] CharacterNames;
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
    // Use this for initialization
    private void Awake()
    {
        if (CC == null)
            CC = this;
        DontDestroyOnLoad(gameObject);
        CharacterSlotsTaken = new List<bool>(new bool[] { false, false, false, false, false, false });
        _playersMoveCharged = new bool[] { true, true, true, true, true, true };
        PlayerHoveringSlots = new int[] { 0, 0, 0, 0, 0, 0 };
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
        ConsoleProDebug.Watch("Player 0's Selection Information", FinalInformation[0].PlayerName + " , " + FinalInformation[0].PlayerColor.ToString());
        ConsoleProDebug.Watch("Player 1's Selection Information", FinalInformation[1].PlayerName + " , " + FinalInformation[1].PlayerColor.ToString());
        ConsoleProDebug.Watch("Player 2's Selection Information", FinalInformation[2].PlayerName + " , " + FinalInformation[2].PlayerColor.ToString());
        ConsoleProDebug.Watch("Player 3's Selection Information", FinalInformation[3].PlayerName + " , " + FinalInformation[3].PlayerColor.ToString());
        ConsoleProDebug.Watch("Player 4's Selection Information", FinalInformation[4].PlayerName + " , " + FinalInformation[4].PlayerColor.ToString());
        ConsoleProDebug.Watch("Player 5's Selection Information", FinalInformation[5].PlayerName + " , " + FinalInformation[5].PlayerColor.ToString());



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
            }
        }
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
            PlayerHoveringSlots[playernumber] = nmod(PlayerHoveringSlots[playernumber] + (horizontal > 0f ? 1 : -1), 6);
            _changeMember(PlayerHoveringSlots[playernumber], playernumber);
        }
        else if (Mathf.Approximately(horizontal, 0f))
        {
            _playersMoveCharged[playernumber] = true;
        }

        //// If player moved vertically then do something
        //if ((Mathf.Abs(vertical) - 0.9f > 0f) && _playersMoveCharged[playernumber])
        //{
        //    _playersMoveCharged[playernumber] = false;
        //    PlayerHoveringSlots[playernumber] = truncate(PlayerHoveringSlots[playernumber], (vertical > 0f ? 3 : -3));
        //    _changeMember(PlayerHoveringSlots[playernumber], playernumber);
        //}
        //else if (Mathf.Approximately(vertical, 0f))
        //{
        //    _playersMoveCharged[playernumber] = true;
        //}
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
