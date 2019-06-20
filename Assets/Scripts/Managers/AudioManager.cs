using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
	public AudioClip BlockAudioClip;
	public AudioClip PunchChargingAudioClip;
	public AudioClip PunchReleasedAudioClip;
	public AudioClip HookGunFiredAudioClip;
	public AudioClip HookGunHitAudioClip;
	public AudioClip SuckGunFiredAudioClip;
	public AudioClip SuckGunSuckAudioClip;
	public AudioClip DeathAudioClip;
	public AudioClip PlayerHitAudioClip;

	[Header("Optional")]
	public AudioClip ObjectDespawnedAudioClip;
	public AudioClip PlayerRespawnedAudioClip;
	public AudioClip WeaponSpawnedAudioClip;

	private void _playSound(GameObject obj, AudioClip clip, bool oneshot = true)
	{
		AudioSource objas = obj.GetComponent<AudioSource>();
		if (objas == null) objas = obj.AddComponent<AudioSource>();
		Debug.Assert(objas != null);

		if (oneshot)
			objas.PlayOneShot(clip);
		else
		{
			objas.clip = clip;
			objas.Play();
		}
	}

	private void _onPlayerHit(PlayerHit ph)
	{
		AudioSource hittedas = ph.Hitted.GetComponent<AudioSource>();
		if (hittedas == null) hittedas = ph.Hiter.AddComponent<AudioSource>();
		Debug.Assert(hittedas != null);

		if (ph.Hiter == null)
		{
			///If hiter is null, then it's a block
			hittedas.PlayOneShot(BlockAudioClip);
		}
		else
		{
			///If it's not null, then it's a hit
			hittedas.PlayOneShot(PlayerHitAudioClip);
		}
	}

	private void _onHookGunFired(HookGunFired hgf)
	{
		_playSound(hgf.HookGun, HookGunFiredAudioClip);
	}

	private void _onHookGunHit(HookHit hh)
	{
		_playSound(hh.Hook, HookGunHitAudioClip);
	}

	private void _onSuckGunFired(SuckGunFired sgf)
	{
		_playSound(sgf.SuckGun, SuckGunFiredAudioClip);
	}

	private void _onSuckGunSuck(SuckGunSuck sgs)
	{
		_playSound(sgs.SuckBall, SuckGunSuckAudioClip);
	}

	private void _onPlayerDied(PlayerDied pd)
	{
		_playSound(pd.Player, DeathAudioClip);
	}

	private void _onObjectDespawned(ObjectDespawned od)
	{

	}

	private void _onPlayerRespawned(PlayerRespawned pr)
	{

	}

	private void _onWeaponSpawned(WeaponSpawned ws)
	{

	}

	private void _onPunchHolding(PunchHolding ph)
	{
		_playSound(ph.Player, PunchChargingAudioClip, false);
	}

	private void _onPunchReleased(PunchReleased pr)
	{
		_playSound(pr.Player, PunchReleasedAudioClip, false);

	}


	private void OnEnable()
	{
		EventManager.Instance.AddHandler<PlayerHit>(_onPlayerHit);
		EventManager.Instance.AddHandler<HookGunFired>(_onHookGunFired);
		EventManager.Instance.AddHandler<HookHit>(_onHookGunHit);
		EventManager.Instance.AddHandler<SuckGunFired>(_onSuckGunFired);
		EventManager.Instance.AddHandler<SuckGunSuck>(_onSuckGunSuck);
		EventManager.Instance.AddHandler<PlayerDied>(_onPlayerDied);
		EventManager.Instance.AddHandler<ObjectDespawned>(_onObjectDespawned);
		EventManager.Instance.AddHandler<PlayerRespawned>(_onPlayerRespawned);
		EventManager.Instance.AddHandler<WeaponSpawned>(_onWeaponSpawned);
		EventManager.Instance.AddHandler<PunchHolding>(_onPunchHolding);
		EventManager.Instance.AddHandler<PunchReleased>(_onPunchReleased);
	}

	private void OnDisable()
	{
		EventManager.Instance.RemoveHandler<PlayerHit>(_onPlayerHit);
		EventManager.Instance.RemoveHandler<HookGunFired>(_onHookGunFired);
		EventManager.Instance.RemoveHandler<HookHit>(_onHookGunHit);
		EventManager.Instance.RemoveHandler<SuckGunFired>(_onSuckGunFired);
		EventManager.Instance.RemoveHandler<SuckGunSuck>(_onSuckGunSuck);
		EventManager.Instance.RemoveHandler<PlayerDied>(_onPlayerDied);
		EventManager.Instance.RemoveHandler<ObjectDespawned>(_onObjectDespawned);
		EventManager.Instance.RemoveHandler<PlayerRespawned>(_onPlayerRespawned);
		EventManager.Instance.RemoveHandler<WeaponSpawned>(_onWeaponSpawned);
		EventManager.Instance.RemoveHandler<PunchHolding>(_onPunchHolding);
		EventManager.Instance.RemoveHandler<PunchReleased>(_onPunchReleased);
	}
}
