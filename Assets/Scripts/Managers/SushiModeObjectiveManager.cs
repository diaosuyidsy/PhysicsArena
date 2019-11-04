using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SushiModeObjectiveManager : ObjectiveManager
{
    public Transform Scores;
    public Transform CurrentHolding;
    private SushiModeData _sushiModeData;
    private int[] _playerCurrentHoldingEggs;
    private int[] _playerTotalCollectedEggs;
    private float _playerStartTime;
    private float[] _playerSpentTime;
    private int _playerThatEndedTheMatch;

    public SushiModeObjectiveManager(SushiModeData _smd) : base()
    {
        _sushiModeData = _smd;
        _playerCurrentHoldingEggs = new int[6];
        _playerTotalCollectedEggs = new int[6];
        _playerSpentTime = new float[6];
        EventManager.Instance.AddHandler<GameStart>(OnGameStart);
        Scores = GameUI.Find("RacingScores").Find("Scores");
        CurrentHolding = GameUI.Find("RacingScores").Find("CurrentHolding");
        Debug.Assert(CurrentHolding != null, "GAME UI NEEDS ATTENTION");
    }

    public void ChangeRespawnPoint(int coloindex, Vector3 Pos, string tag)
    {
        Debug.Log(coloindex);
        if (tag.Equals("Team1"))
        {
            Services.Config.Team1RespawnPoints[coloindex % 3] = Pos;
        }
        else
        {
            Services.Config.Team2RespawnPoints[coloindex % 3] = Pos;
        }
    }

    public void OnGameStart(GameStart ev)
    {
        _playerStartTime = Time.time;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="winner">0 is draw, 1 is team1, 2 is team2</param>
    public void OnWin(int winner, Transform winnedTransform)
    {
        EventManager.Instance.TriggerEvent(new GameEnd(winner, winnedTransform, GameWinType.RaceWin));
    }

    public void OnWin(int winner, Vector3 winnedTransform)
    {
        EventManager.Instance.TriggerEvent(new GameEnd(winner, winnedTransform, GameWinType.RaceWin));
    }

    /// <summary>
    /// Takes player's color index, return true if player can pick up egg
    /// False if player cannot
    /// </summary>
    /// <param name="colorindex"></param>
    /// <returns></returns>
    public bool PickUpEgg(int colorindex)
    {
        if (_playerCurrentHoldingEggs[colorindex] < _sushiModeData.PlayerMaxHoldingEggs)
        {
            _playerCurrentHoldingEggs[colorindex]++;
            CurrentHolding.GetChild(colorindex).GetComponent<TextMeshProUGUI>().text = _playerCurrentHoldingEggs[colorindex].ToString();
            return true;
        }
        else
        {
            return false;
        }
    }

    public void CollectEgg(int colorindex)
    {
        _playerTotalCollectedEggs[colorindex] += _playerCurrentHoldingEggs[colorindex];
        _playerCurrentHoldingEggs[colorindex] = 0;
        CurrentHolding.GetChild(colorindex).GetComponent<TextMeshProUGUI>().text = _playerCurrentHoldingEggs[colorindex].ToString();
        Scores.GetChild(colorindex).GetComponent<TextMeshProUGUI>().text = _playerTotalCollectedEggs[colorindex].ToString();
        if (_playerTotalCollectedEggs[colorindex] >= 9)
        {
            _playerSpentTime[colorindex] = Time.time - _playerStartTime;
            _playerThatEndedTheMatch++;
            Debug.Log("Player that ended the match" + _playerThatEndedTheMatch);
        }
        if (_playerThatEndedTheMatch >= Services.GameStateManager.PlayersInformation.RewiredID.Length)
        {
            // Calculate Winner
            float team2Score = 0f;
            float team1Score = 0f;
            for (int i = 0; i < 3; i++)
            {
                team2Score += _playerSpentTime[i];
                team1Score += _playerSpentTime[i + 3];
            }
            Debug.Log("TEAM 1 Score: " + team1Score.ToString());
            Debug.Log("TEAM 2 Score: " + team2Score.ToString());
            int winner = team1Score > team2Score ? 2 : 1;
            // OnWin(winner, Vector3.zero);
        }
    }
}
