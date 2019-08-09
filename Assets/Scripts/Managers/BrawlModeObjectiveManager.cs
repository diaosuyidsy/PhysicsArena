using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrawlModeObjectiveManager : ObjectiveManager
{
    private int _team1Score;
    private int _team2Score;
    private int _team1FoodCount;
    private int _team2FoodCount;
    private int winner { get { return _team1Score > _team2Score ? 1 : 2; } }

    public BrawlModeObjectiveManager()
    {
        EventManager.Instance.AddHandler<PlayerDied>(_onPlayerDied);
        EventManager.Instance.AddHandler<FoodDelivered>(_onFoodDelivered);
    }

    public override void Destroy()
    {
        EventManager.Instance.RemoveHandler<PlayerDied>(_onPlayerDied);
        EventManager.Instance.RemoveHandler<FoodDelivered>(_onFoodDelivered);
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
            _team1FoodCount++;
        }
        else
        {
            _team2Score += 10;
            _team2FoodCount++;
        }
        _refreshScore();
        if (_team1FoodCount == 2 || _team2FoodCount == 2)
            EventManager.Instance.TriggerEvent(new GameEnd(winner, ev.Food.transform, GameWinType.ScoreWin));
    }

    private void _refreshScore()
    {

    }
}
