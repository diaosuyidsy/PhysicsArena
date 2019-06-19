using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;

/// <summary>
/// Game Feel Manager Manages Controller vibration
/// and screen shakes
/// </summary>
public class GameFeelManager : MonoBehaviour
{
	#region Event Handlers
	private void _onPlayerHit(PlayerHit ph)
	{
		Player _hitted = ReInput.players.GetPlayer(ph.HittedPlayerNumber);
		CameraShake.CS.Shake(0.1f, 0.1f);
		_hitted.SetVibration(0, 1.0f, 0.25f);
		_hitted.SetVibration(1, 1.0f, 0.25f);

		if (ph.HiterPlayerNumber < 0) return;
		Player _hiter = ReInput.players.GetPlayer(ph.HiterPlayerNumber);
		_hiter.SetVibration(0, 1.0f, 0.15f);
		_hiter.SetVibration(1, 1.0f, 0.15f);
	}

	private void _onPlayerDied(PlayerDied pd)
	{
		Player thedead = ReInput.players.GetPlayer(pd.PlayerNumber);
		CameraShake.CS.Shake(0.1f, 0.1f);
		thedead.SetVibration(0, 1.0f, 0.25f);
		thedead.SetVibration(1, 1.0f, 0.25f);
	}

	private void _onPlayerFireWaterGun(WaterGunFired wf)
	{
		Player p = ReInput.players.GetPlayer(wf.WaterGunOwnerPlayerNumber);
		p.SetVibration(0, 1.0f, 0.15f);
		p.SetVibration(1, 1.0f, 0.15f);
	}
	#endregion

	private void OnEnable()
	{
		EventManager.Instance.AddHandler<PlayerHit>(_onPlayerHit);
		EventManager.Instance.AddHandler<PlayerDied>(_onPlayerDied);
		EventManager.Instance.AddHandler<WaterGunFired>(_onPlayerFireWaterGun);
	}

	private void OnDisable()
	{
		EventManager.Instance.RemoveHandler<PlayerHit>(_onPlayerHit);
		EventManager.Instance.RemoveHandler<PlayerDied>(_onPlayerDied);
		EventManager.Instance.RemoveHandler<WaterGunFired>(_onPlayerFireWaterGun);

	}
}
