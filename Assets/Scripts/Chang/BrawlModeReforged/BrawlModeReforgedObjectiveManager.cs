﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BrawlModeReforgedObjectiveManager : ObjectiveManager
{
    public int TeamAScore;
    public int TeamBScore;
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

    private BrawlModeReforgedModeData ModeData;

    private bool gameEnd;
    private bool gameStart;

    private float OneSecTimer;
    private int Timer;

    public BrawlModeReforgedObjectiveManager(BrawlModeReforgedModeData Data) : base()
    {

        ModeData = Data;
        Timer = Data.TotalTime;

        Debug.Log(Data.name);

        EventManager.Instance.AddHandler<GameStart>(OnGameStart);
        EventManager.Instance.AddHandler<PlayerDied>(OnPlayerDied);
        EventManager.Instance.AddHandler<BagelSent>(OnBagelSent);


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
        EventManager.Instance.RemoveHandler<PlayerDied>(OnPlayerDied);
        EventManager.Instance.RemoveHandler<BagelSent>(OnBagelSent);
    }

    public override void Update()
    {
        if (gameEnd || !gameStart) return;

        UpdateTime();

    }

    private void UpdateTime()
    {
        OneSecTimer += Time.deltaTime;
        if (OneSecTimer >= 1f)
        {
            OneSecTimer = 0f;
            Timer--;
        }
        if (Timer <= 0)
        {
            TimerText.text = "0:00";
            if (TeamAScore != TeamBScore)
            {
                EventManager.Instance.TriggerEvent(new GameEnd(winner, Camera.main.ScreenToWorldPoint(TimerText.transform.position), GameWinType.ScoreWin));
                gameEnd = true;
                return;
            }
        }
        else
        {
            TimerText.text = TimerToMinute();
        }

    }

    private string TimerToMinute()
    {
        int seconds = Timer % 10;
        int tenseconds = (Timer % 60) / 10;
        int minute = Timer / 60;
        return minute.ToString("F0") + ":" + tenseconds.ToString("F0") + seconds.ToString("F0");
    }

    private void RefreshScore()
    {
        TeamAScoreText.text = TeamAScore.ToString();
        TeamBScoreText.text = TeamBScore.ToString();
    }

    private void OnBagelSent(BagelSent e)
    {
        if (gameEnd || !gameStart)
        {
            return;
        }

        if (e.Basket == BrawlModeReforgedArenaManager.Team1Basket)
        {
            TeamAScore += 3;
        }
        else
        {
            TeamBScore += 3;
        }

        RefreshScore();
    }

    private void OnPlayerDied(PlayerDied e)
    {
        if (gameEnd || !gameStart)
        {
            return;
        }

        if (e.Player.tag.Contains("1"))
        {
            TeamBScore += ModeData.NormalKillPoint;
        }
        else
        {
            TeamAScore += ModeData.NormalKillPoint;
        }

        RefreshScore();
    }

    private void OnGameStart(GameStart e)
    {
        gameStart = true;
    }
}
