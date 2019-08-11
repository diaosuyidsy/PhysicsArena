using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class BrawlModeObjectiveManager : ObjectiveManager
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
    private BrawlModeData _brawlModeData;
    private TextMeshProUGUI _team1ScoreText;
    private TextMeshProUGUI _team2ScoreText;
    private TextMeshProUGUI _timerText;
    private Image _counter;
    private bool _winned;
    private bool _gamestart;
    private float _onesecTimer;
    public BrawlModeObjectiveManager(BrawlModeData _bmd) : base()
    {
        _brawlModeData = _bmd;
        _timer = _brawlModeData.TotalTime;
        EventManager.Instance.AddHandler<PlayerDied>(_onPlayerDied);
        EventManager.Instance.AddHandler<FoodDelivered>(_onFoodDelivered);
        EventManager.Instance.AddHandler<GameStart>(_onGameStart);
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
        EventManager.Instance.RemoveHandler<PlayerDied>(_onPlayerDied);
        EventManager.Instance.RemoveHandler<FoodDelivered>(_onFoodDelivered);
        EventManager.Instance.RemoveHandler<GameStart>(_onGameStart);
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
        _counter.fillAmount = 1f * _timer / _brawlModeData.TotalTime;
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

    // 杀人，自杀
    private void _onPlayerDied(PlayerDied ev)
    {
        // Suicide
        if (!ev.HitterIsValid || ev.PlayerHitter == null)
        {
            if (ev.Player.tag.Contains("1")) _team1Score--;
            else _team2Score--;
        }
        // Teammate Murdur
        if (ev.HitterIsValid && ev.Player.tag == ev.PlayerHitter.tag)
        {
            if (ev.Player.tag.Contains("1")) _team1Score--;
            else _team2Score--;
        }
        // Kill Record
        if (ev.HitterIsValid && ev.Player.tag != ev.PlayerHitter.tag)
        {
            if (ev.Player.tag.Contains("1")) _team2Score += 2;
            else _team1Score += 2;
        }
        _refreshScore();
    }

    private void _onGameStart(GameStart ev)
    {
        _gamestart = true;
    }

    private void _onFoodDelivered(FoodDelivered ev)
    {
        if (ev.FoodTag.Contains("1"))
        {
            _team1Score += 10;
            _timer -= _brawlModeData.FoodReduceTime;
        }
        else
        {
            _team2Score += 10;
            _timer -= _brawlModeData.FoodReduceTime;
        }
        _refreshScore();
    }

    private void _refreshScore()
    {
        _team1ScoreText.text = _team1Score.ToString();
        _team2ScoreText.text = _team2Score.ToString();
    }
}
