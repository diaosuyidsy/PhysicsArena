using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TinylyticsHandler : MonoBehaviour
{
	#region Event Handlers
	private void _onPlayerHit(PlayerHit ph)
	{
		if (ph.Hiter == null)
		{
			///If hiter is null, then it's a block
			string message = "Player is Blocked";
			Tinylytics.AnalyticsManager.LogCustomMetric(message, ph.HittedPlayerNumber.ToString());
		}
		else
		{
			///If it's not null, then it's a hit
			string message = "Player Hit";
			Tinylytics.AnalyticsManager.LogCustomMetric(message, "By Player " + ph.HittedPlayerNumber.ToString());
			Tinylytics.AnalyticsManager.LogCustomMetric(message, "Hit Player " + ph.HiterPlayerNumber.ToString());
		}
	}

	private void _onHookGunFired(HookGunFired hgf)
	{
		string message = "Hook Gun Fired";
		Tinylytics.AnalyticsManager.LogCustomMetric(message, "Player " + hgf.HookGunOwnerPlayerNumber.ToString() + "Fired Hook Gun");
	}

	private void _onHookGunHit(HookHit hh)
	{
		string message = "Hook Gun Hit";
		Tinylytics.AnalyticsManager.LogCustomMetric(message, "Hitted Player " + hh.HookedPlayerNumber.ToString());
		Tinylytics.AnalyticsManager.LogCustomMetric(message, "Player " + hh.HookerPlayerNumber.ToString() + "Fired Hook Gun");
	}

	private void _onSuckGunFired(SuckGunFired sgf)
	{
		string message = "Suck Gun Fired";
		Tinylytics.AnalyticsManager.LogCustomMetric(message, "Player " + sgf.SuckGunOwnerPlayerNumber.ToString() + "Fired Suck Gun");
	}

	private void _onSuckGunSuck(SuckGunSuck sgs)
	{
		string message = "Suck Gun Suck";
		string messageSucked = "Sucked Player ";
		foreach (int p in sgs.SuckedPlayersNumber)
		{
			messageSucked += (p + ", ");
		}
		Tinylytics.AnalyticsManager.LogCustomMetric(message, messageSucked);
		Tinylytics.AnalyticsManager.LogCustomMetric(message, "Player " + sgs.SuckGunOwnerPlayerNumber.ToString() + "Used this Suck Gun");
	}

	private void _onPlayerDied(PlayerDied pd)
	{
		string message = "Player Died";
		Tinylytics.AnalyticsManager.LogCustomMetric(message, pd.PlayerNumber.ToString());
	}

	private void _onGameStart(GameStart gs)
	{
		Tinylytics.AnalyticsManager.LogCustomMetric("Game Start", "");
	}

	private void _onGameEnd(GameEnd ge)
	{
		Tinylytics.AnalyticsManager.LogCustomMetric("Game End", "Winner: Team " + ge.Winner.ToString());
	}


	#endregion

	private void OnEnable()
	{
		EventManager.Instance.AddHandler<PlayerHit>(_onPlayerHit);
		EventManager.Instance.AddHandler<HookGunFired>(_onHookGunFired);
		EventManager.Instance.AddHandler<HookHit>(_onHookGunHit);
		EventManager.Instance.AddHandler<SuckGunFired>(_onSuckGunFired);
		EventManager.Instance.AddHandler<SuckGunSuck>(_onSuckGunSuck);
		EventManager.Instance.AddHandler<PlayerDied>(_onPlayerDied);
		EventManager.Instance.AddHandler<GameStart>(_onGameStart);
		EventManager.Instance.AddHandler<GameEnd>(_onGameEnd);
	}

	private void OnDisable()
	{
		EventManager.Instance.RemoveHandler<PlayerHit>(_onPlayerHit);
		EventManager.Instance.RemoveHandler<HookGunFired>(_onHookGunFired);
		EventManager.Instance.RemoveHandler<HookHit>(_onHookGunHit);
		EventManager.Instance.RemoveHandler<SuckGunFired>(_onSuckGunFired);
		EventManager.Instance.RemoveHandler<SuckGunSuck>(_onSuckGunSuck);
		EventManager.Instance.RemoveHandler<PlayerDied>(_onPlayerDied);
		EventManager.Instance.RemoveHandler<GameStart>(_onGameStart);
		EventManager.Instance.RemoveHandler<GameEnd>(_onGameEnd);
	}
}
