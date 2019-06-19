using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
	public AudioClip BlockAudioClip;
	public AudioClip PunchChargingAudioClip;
	public AudioClip PunchHitAudioClip;
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

	private void _onPlayerHit(PlayerHit ph)
	{
		if (ph.Hiter == null)
		{
			///If hiter is null, then it's a block
		}
		else
		{
			///If it's not null, then it's a hit
		}
	}



	private void OnEnable()
	{
		EventManager.Instance.AddHandler<PlayerHit>(_onPlayerHit);
	}

	private void OnDisable()
	{
		EventManager.Instance.RemoveHandler<PlayerHit>(_onPlayerHit);
	}
}
