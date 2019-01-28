using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Rewired;
using UnityEngine.SceneManagement;

public class CanvasController : MonoBehaviour
{
    public static CanvasController CC = null;
    public RectTransform[] Characters;
    public GameObject[] GreyCharacterIcons;
    public GameObject[] GreyCharacters;
    public GameObject[] PureGreyBackgrounds;
    public GameObject[] PlayerSlots;
    public Text HeaderText;
    public GameObject StartCountDown;

    [Header("Character Information: Align with Image")]
    public Color[] CharacterColors;
    public string[] CharacterNames;
    // Final Information is what is kept to the playing scene
    [HideInInspector]
    public PlayerInformation[] FinalInformation;
    [HideInInspector]
    public bool[] CharacterSlotsTaken;
    [HideInInspector]
    public int[] PlayerHoveringSlots;
    [HideInInspector]
    public bool[] PlayersLockedIn;

    private Player[] _players;
    private bool[] _playersMoveCharged;
    private int _playersLockedInCount = 0;
    private bool _chickenSelected = false;
    private bool _duckSelected = false;
    private bool[] _characterShadowLocked;

    // Use this for initialization
    private void Awake()
    {
        if (CC == null)
            CC = this;
        DontDestroyOnLoad(gameObject);
        CharacterSlotsTaken = new bool[] { false, false, false, false, false, false };
        _characterShadowLocked = new bool[6];
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
                // Bubble Animation chosen icon
                Characters[curPlayerSelectionIndex].GetComponent<Animator>().SetTrigger("OnChosen");
                // Balance the chicken and duck
                _balanceChickDuck(curPlayerSelectionIndex);
                // Kick Out other hovering players
                _kickOutOtherPlayers(playernumber, curPlayerSelectionIndex);
                // Add to player selection count
                OnPlayerSelection();
            }
        }
    }

    private void _kickOutOtherPlayers(int playernumber, int currentSelectionIndex)
    {
        for (int i = 0; i < 6; i++)
        {
            // Meaning ith player is hovering over the current selection,
            // Need to be kicked out
            if (i != playernumber && PlayerHoveringSlots[i] == currentSelectionIndex)
            {
                PlayerHoveringSlots[i] = _advancePlace(PlayerHoveringSlots[i], 1);
                Characters[PlayerHoveringSlots[i]].GetComponent<Animator>().SetTrigger("OnHover");
                PureGreyBackgrounds[PlayerHoveringSlots[i]].SetActive(false);

                _changeMember(PlayerHoveringSlots[i], i);
            }
        }
    }

    private void _balanceChickDuck(int curSelectionIndex)
    {
        if (curSelectionIndex <= 2) _chickenSelected = true;
        else _duckSelected = true;

        if (_chickenSelected && !_duckSelected)
        {
            // Block Out the Chicken
            for (int i = 0; i < 3; i++)
            {
                if (i != curSelectionIndex)
                {
                    _characterShadowLocked[i] = true;
                    CharacterSlotsTaken[i] = true;
                    // Deactivate the icons
                    GreyCharacterIcons[i].SetActive(true);
                    _kickOutOtherPlayers(-1, i);
                }
            }
        }
        else if (!_chickenSelected && _duckSelected)
        {
            // Block Out the Duck
            for (int i = 3; i < 6; i++)
            {
                if (i != curSelectionIndex)
                {
                    _characterShadowLocked[i] = true;
                    CharacterSlotsTaken[i] = true;
                    // Deactivate the icons
                    GreyCharacterIcons[i].SetActive(true);
                    _kickOutOtherPlayers(-1, i);
                }
            }
        }
        else
        {
            // UnBlock All
            for (int i = 0; i < 6; i++)
            {
                if (_characterShadowLocked[i])
                {
                    _characterShadowLocked[i] = false;
                    CharacterSlotsTaken[i] = false;
                    // Actate the icons
                    GreyCharacterIcons[i].SetActive(false);
                }
            }
        }
    }

    private void _greyOutUnselectedIcons()
    {
        for (int i = 0; i < 6; i++)
        {
            if (!CharacterSlotsTaken[i])
            {
                GreyCharacterIcons[i].SetActive(true);
            }
        }
    }

    private void _switchToEndingHeader()
    {
        HeaderText.text = "Game Starting In...";
        HeaderText.transform.GetChild(0).GetComponent<Text>().text = "Game Starting In...";
    }

    private void OnPlayerSelection()
    {
        _playersLockedInCount++;
        if (_playersLockedInCount >= ReInput.controllers.joystickCount && ReInput.controllers.joystickCount > 1)
        {
            // Meaning We are ready to start
            _greyOutUnselectedIcons();
            _switchToEndingHeader();
            StartCoroutine(ChangeNumber());
        }
    }

    IEnumerator ChangeNumber()
    {
        yield return new WaitForSeconds(1f);
        StartCountDown.SetActive(true);

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
        // Switch to Loading State instead of straightly loading the scene
        MenuController.MC.ChangeToLoadingState();
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
        // If player Moved Horizontally then do something
        if ((Mathf.Abs(horizontal) - 0.9f > 0f) && _playersMoveCharged[playernumber])
        {
            _playersMoveCharged[playernumber] = false;
            // Change Grey Background Image
            if (!_isOtherPlayerHoveringHere(playernumber, PlayerHoveringSlots[playernumber]))
                PureGreyBackgrounds[PlayerHoveringSlots[playernumber]].SetActive(true);
            PlayerHoveringSlots[playernumber] = _advancePlace(PlayerHoveringSlots[playernumber], (horizontal > 0f ? 1 : -1));
            Characters[PlayerHoveringSlots[playernumber]].GetComponent<Animator>().SetTrigger("OnHover");
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
