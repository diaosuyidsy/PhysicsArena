using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SoccerModeObjectiveManager : ObjectiveManager
{
    private int _team1Score;
    private int _team2Score;
    private int winner
    {
        get
        {
            if (_team1Score == _team2Score) return 0;
            return _team1Score > _team2Score ? 1 : 2;
        }
    }
    private int _timer;
    private SoccerMapData _soccerModeData;
    private TextMeshProUGUI _team1ScoreText;
    private TextMeshProUGUI _team2ScoreText;
    private TextMeshProUGUI _timerText;
    private Image _counter;
    private bool _winned;
    private bool _gamestart;
    private float _onesecTimer;
    public SoccerModeObjectiveManager(SoccerMapData _bmd) : base()
    {
        _soccerModeData = _bmd;
        _timer = _soccerModeData.TotalTime;
        EventManager.Instance.AddHandler<GameStart>(_onGameStart);
        EventManager.Instance.AddHandler<OnScore>(_onScore);
        _team1ScoreText = GameUI.Find("Team1Score").GetComponent<TextMeshProUGUI>();
        _team2ScoreText = GameUI.Find("Team2Score").GetComponent<TextMeshProUGUI>();
        _timerText = GameUI.Find("TimerText").GetComponent<TextMeshProUGUI>();
        _counter = GameUI.Find("Counter").GetComponent<Image>();
        _team1Score = 0;
        _team2Score = 0;
        _refreshScore();
        _winned = false;
    }

    public override void Destroy()
    {
        EventManager.Instance.RemoveHandler<GameStart>(_onGameStart);
        EventManager.Instance.RemoveHandler<OnScore>(_onScore);

    }

    public override void Update()
    {
        if (_winned || !_gamestart) return;
        _onesecTimer += Time.deltaTime;
        if (_onesecTimer >= 1f)
        {
            _onesecTimer = 0f;
            _timer--;
        }
        _counter.fillAmount = 1f * _timer / _soccerModeData.TotalTime;
        if (_timer <= 0)
        {
            _timerText.text = "0:00";
            EventManager.Instance.TriggerEvent(new GameEnd(winner, Camera.main.ScreenToWorldPoint(_timerText.transform.position), GameWinType.ScoreWin));
            _winned = true;
            return;
        }
        _timerText.text = _timerToMinute();
    }

    private string _timerToMinute()
    {
        int seconds = _timer % 10;
        int tenseconds = (_timer % 60) / 10;
        int minute = _timer / 60;
        return minute.ToString("F0") + ":" + tenseconds.ToString("F0") + seconds.ToString("F0");
    }


    private void _onGameStart(GameStart ev)
    {
        _gamestart = true;
    }

    private void _onScore(OnScore ev)
    {
        if (ev.Team == 1)
        {
            _team2Score += 1;
        }
        else
        {
            _team1Score += 1;
        }
        _refreshScore();
    }


    private void _refreshScore()
    {
        _team1ScoreText.text = _team1Score.ToString();
        _team2ScoreText.text = _team2Score.ToString();
    }
}
