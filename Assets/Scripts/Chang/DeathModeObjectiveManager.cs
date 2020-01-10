using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DeathModeObjectiveManager : ObjectiveManager
{
    private int TeamAScore;
    private int TeamBScore;
    private int winner
    {
        get
        {
            if (TeamAScore == TeamBScore) return 0;
            return TeamAScore > TeamBScore ? 1 : 2;
        }
    }

    private TextMeshProUGUI TeamAScoreText;
    private TextMeshProUGUI TeamBScoreText;
    private TextMeshProUGUI TimerText;

    private DeathModeData ModeData;

    private bool gameEnd;
    private bool gameStart;

    public DeathModeObjectiveManager(DeathModeData Data) : base()
    {
        ModeData = Data;

        EventManager.Instance.AddHandler<GameStart>(OnGameStart);
        EventManager.Instance.AddHandler<PlayerDiedInDeathMode>(OnPlayerDied);


        TeamAScoreText = GameUI.Find("Team1Score").GetComponent<TextMeshProUGUI>();
        TeamBScoreText = GameUI.Find("Team2Score").GetComponent<TextMeshProUGUI>();
        TimerText = GameUI.Find("TimerText").GetComponent<TextMeshProUGUI>();

        TeamAScore = 0;
        TeamBScore = 0;
        RefreshScore();
        gameEnd = false;
    }

    public override void Destroy()
    {
        EventManager.Instance.RemoveHandler<GameStart>(OnGameStart);
        EventManager.Instance.RemoveHandler<PlayerDiedInDeathMode>(OnPlayerDied);
    }

    public override void Update()
    {

    }

    private void RefreshScore()
    {
        TeamAScoreText.text = TeamAScore.ToString();
        TeamBScoreText.text = TeamBScore.ToString();
    }

    private void OnPlayerDied(PlayerDiedInDeathMode e)
    {
        if (gameEnd || !gameStart)
        {
            return;
        }

        if (e.DeadInCircle)
        {
            if (e.Player.tag.Contains("1"))
            {
                TeamBScore += ModeData.CircleDeathScore;
            }
            else
            {
                TeamAScore += ModeData.CircleDeathScore;
            }
        }
        else
        {
            if (e.Player.tag.Contains("1"))
            {
                TeamBScore += ModeData.NormalDeathScore;
            }
            else
            {
                TeamAScore += ModeData.NormalDeathScore;
            }
        }

        RefreshScore();

        if (TeamAScore >= ModeData.WinningScore || TeamBScore >= ModeData.WinningScore)
        {
            gameEnd = true;
            EventManager.Instance.TriggerEvent(new GameEnd(winner, Camera.main.ScreenToWorldPoint(TimerText.transform.position), GameWinType.ScoreWin));
        }
    }

    private void OnGameStart(GameStart e)
    {
        gameStart = true;
    }
}
