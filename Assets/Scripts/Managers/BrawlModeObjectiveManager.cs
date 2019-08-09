using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BrawlModeObjectiveManager : ObjectiveManager
{
    private int _team1Score;
    private int _team2Score;
    private int winner { get { return _team1Score > _team2Score ? 1 : 2; } }
    private float _timer;
    private BrawlModeData _brawlModeData;
    private TextMeshProUGUI _team1ScoreText;
    private TextMeshProUGUI _team2ScoreText;
    public BrawlModeObjectiveManager(BrawlModeData _bmd) : base()
    {
        _brawlModeData = _bmd;
        _timer = _brawlModeData.TotalTime;
        EventManager.Instance.AddHandler<PlayerDied>(_onPlayerDied);
        EventManager.Instance.AddHandler<FoodDelivered>(_onFoodDelivered);
        _team1ScoreText = GameUI.Find("Team1Score").GetComponent<TextMeshProUGUI>();
        _team2ScoreText = GameUI.Find("Team2Score").GetComponent<TextMeshProUGUI>();
    }

    public override void Destroy()
    {
        EventManager.Instance.RemoveHandler<PlayerDied>(_onPlayerDied);
        EventManager.Instance.RemoveHandler<FoodDelivered>(_onFoodDelivered);
    }

    public override void Update()
    {
        _timer -= Time.deltaTime * _brawlModeData.TimeSpeed;
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
