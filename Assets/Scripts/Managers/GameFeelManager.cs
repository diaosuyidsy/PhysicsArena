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
		float charge = ph.MeleeCharge;
		if (charge <= 0.3f) charge = 0f;
		CameraShake.CS.Shake(0.1f * charge, 0.1f * charge);
		_vibrateController(ph.HittedPlayerNumber, 1.0f * charge, 0.25f * charge);

		/// If the hiter number is below 0, means it's a block
		/// and blocked attack don't have a hitter
		if (ph.HiterPlayerNumber < 0) return;
		_vibrateController(ph.HiterPlayerNumber, 1.0f * charge, 0.15f * charge);

	}

	private void _onPlayerDied(PlayerDied pd)
	{
		CameraShake.CS.Shake(0.1f, 0.1f);
		_vibrateController(pd.PlayerNumber, 1.0f, 0.25f);
	}

	private void _onPlayerFireWaterGun(WaterGunFired wf)
	{
		_vibrateController(wf.WaterGunOwnerPlayerNumber);
	}

	private void _onPlayerFireHookGun(HookGunFired hf)
	{
		_vibrateController(hf.HookGunOwnerPlayerNumber, 1.0f, 0.1f);
	}

	private void _onHookHit(HookHit hh)
	{
		_vibrateController(hh.HookedPlayerNumber);
		_vibrateController(hh.HookerPlayerNumber);
	}

	private void _onSuckGunFire(SuckGunFired sf)
	{
		_vibrateController(sf.SuckGunOwnerPlayerNumber);
	}

	private void _onSuckGunSuck(SuckGunSuck ss)
	{
		_vibrateController(ss.SuckedPlayersNumber);
		_vibrateController(ss.SuckGunOwnerPlayerNumber);
	}

	#endregion

	private void _vibrateController(int playernumber, float motorlevel = 1.0f, float duration = 0.15f)
	{
		Player player = ReInput.players.GetPlayer(playernumber);
		player.SetVibration(0, motorlevel, duration);
		player.SetVibration(1, motorlevel, duration);
	}

	private void _vibrateController(List<int> playernumbers, float motorlevel = 1.0f, float duration = 0.15f)
	{
		foreach (int playernumber in playernumbers)
		{
			_vibrateController(playernumber, motorlevel, duration);
		}
	}

	private void OnEnable()
	{
		EventManager.Instance.AddHandler<PlayerHit>(_onPlayerHit);
		EventManager.Instance.AddHandler<PlayerDied>(_onPlayerDied);
		EventManager.Instance.AddHandler<WaterGunFired>(_onPlayerFireWaterGun);
		EventManager.Instance.AddHandler<HookGunFired>(_onPlayerFireHookGun);
		EventManager.Instance.AddHandler<HookHit>(_onHookHit);
	}

	private void OnDisable()
	{
		EventManager.Instance.RemoveHandler<PlayerHit>(_onPlayerHit);
		EventManager.Instance.RemoveHandler<PlayerDied>(_onPlayerDied);
		EventManager.Instance.RemoveHandler<WaterGunFired>(_onPlayerFireWaterGun);
		EventManager.Instance.RemoveHandler<HookGunFired>(_onPlayerFireHookGun);
		EventManager.Instance.RemoveHandler<HookHit>(_onHookHit);

	}
}
