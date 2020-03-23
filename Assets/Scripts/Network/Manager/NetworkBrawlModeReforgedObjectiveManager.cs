using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;

public class NetworkBrawlModeReforgedObjectiveManager : NetworkBehaviour
{
    [SyncVar(hook = nameof(RefreshScore))]
    public int TeamAScore;
    [SyncVar(hook = nameof(RefreshScore))]
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

    public BrawlModeReforgedModeData Data;

    private bool gameEnd;
    private bool gameStart;
    [SyncVar]
    private float OneSecTimer;
    [SyncVar(hook = nameof(UpdateTime))]
    public int Timer;

    public void Awake()
    {
        Transform GameUI = GameObject.Find("GameUI").transform;
        Debug.Assert(GameUI != null, "Cannot find GameUI GameObject");
        TeamAScoreText = GameUI.Find("Team1Score").GetComponent<TextMeshProUGUI>();
        TeamBScoreText = GameUI.Find("Team2Score").GetComponent<TextMeshProUGUI>();
        TimerText = GameUI.Find("TimerText").GetComponent<TextMeshProUGUI>();
        Timer = Data.TotalTime;
        TeamAScore = 0;
        TeamBScore = 0;
        gameEnd = false;

    }

    private void OnEnable()
    {
        EventManager.Instance.AddHandler<GameStart>(OnGameStart);
        EventManager.Instance.AddHandler<PlayerDied>(OnPlayerDied);
        EventManager.Instance.AddHandler<BagelSent>(OnBagelSent);
    }

    private void OnDisable()
    {
        EventManager.Instance.RemoveHandler<GameStart>(OnGameStart);
        EventManager.Instance.RemoveHandler<PlayerDied>(OnPlayerDied);
        EventManager.Instance.RemoveHandler<BagelSent>(OnBagelSent);
    }

    public void Update()
    {
        if (!isServer) return;
        if (gameEnd || !gameStart) return;

        OneSecTimer += Time.deltaTime;
        if (OneSecTimer >= 1f)
        {
            OneSecTimer = 0f;
            Timer--;
            if (TeamAScore != TeamBScore)
            {
                EventManager.Instance.TriggerEvent(new GameEnd(winner, Camera.main.ScreenToWorldPoint(TimerText.transform.position), GameWinType.ScoreWin));
                gameEnd = true;
                return;
            }
        }
    }

    private void UpdateTime(int oldTime, int newTime)
    {
        if (newTime <= 0)
        {
            TimerText.text = "0:00";
        }
        else
        {
            TimerText.text = TimerToMinute(newTime);
        }
    }

    private string TimerToMinute(int newTime)
    {
        int seconds = newTime % 10;
        int tenseconds = (newTime % 60) / 10;
        int minute = newTime / 60;
        return minute.ToString("F0") + ":" + tenseconds.ToString("F0") + seconds.ToString("F0");
    }

    private void RefreshScore(int oldScore, int newScore)
    {
        TeamAScoreText.text = TeamAScore.ToString();
        TeamBScoreText.text = TeamBScore.ToString();
    }

    private void OnBagelSent(BagelSent e)
    {
        if (!isServer) return;
        if (gameEnd || !gameStart)
        {
            return;
        }

        if (e.Basket.name.Contains("LForceField"))
        {
            TeamAScore += 3;
        }
        else
        {
            TeamBScore += 3;
        }
    }

    private void OnPlayerDied(PlayerDied e)
    {
        if (!isServer) return;
        if (gameEnd || !gameStart)
        {
            return;
        }

        if (e.ImpactObject.name.Contains("Canon"))
        {
            if (e.Player.tag.Contains("1"))
            {
                TeamBScore += Data.BagelKillPoint;
            }
            else
            {
                TeamAScore += Data.BagelKillPoint;
            }
        }
        else
        {
            if (e.Player.tag.Contains("1"))
            {
                TeamBScore += Data.NormalKillPoint;
            }
            else
            {
                TeamAScore += Data.NormalKillPoint;
            }
        }
    }

    private void OnGameStart(GameStart e)
    {
        gameStart = true;
    }
}
