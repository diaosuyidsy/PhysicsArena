using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AudioData", menuName = "ScriptableObjects/AudioData", order = 1)]
public class AudioData : ScriptableObject
{
	public AudioClip BlockAudioClip;
	public AudioClip PunchChargingAudioClip;
	public AudioClip PunchReleasedAudioClip;
	public AudioClip HookGunFiredAudioClip;
	public AudioClip HookGunHitAudioClip;
	public AudioClip SuckGunFiredAudioClip;
	public AudioClip SuckGunSuckAudioClip;
	public AudioClip DeathAudioClip;
	public AudioClip[] PlayerHitAudioClip;
	public AudioClip FootstepConcreteAudioClip;
	public AudioClip FootstepYellowStoneAudioClip;
	public AudioClip FootstepGrassAudioClip;
	public AudioClip JumpAudioClip;

	[Header("Optional")]
	public AudioClip ObjectDespawnedAudioClip;
	public AudioClip PlayerRespawnedAudioClip;
	public AudioClip WeaponSpawnedAudioClip;
}
