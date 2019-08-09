using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AudioData", menuName = "ScriptableObjects/AudioData", order = 1)]
public class AudioData : ScriptableObject
{
    public AudioClip BlockAudioClip;
    public AudioClip PunchChargingAudioClip;
    public AudioClip[] PunchReleasedAudioClip;
    public AudioClip[] WaterGunFiredAudioClip;
    public AudioClip HookGunFiredAudioClip;
    public AudioClip HookGunHitAudioClip;
    public AudioClip HookGunBlockedAudioClip;
    public AudioClip SuckGunFiredAudioClip;
    public AudioClip SuckGunSuckAudioClip;
    public AudioClip DeathAudioClip;
    public AudioClip[] PlayerHitAudioClip;
    public AudioClip[] FootstepConcreteAudioClip;
    public AudioClip[] FootstepYellowStoneAudioClip;
    public AudioClip[] FootstepGrassAudioClip;
    public AudioClip[] LandAudioClip;
    public AudioClip[] JumpAudioClip;
    public AudioClip[] ObjectDespawnedAudioClip;
    public AudioClip WrongFoodAudioClip;
    public AudioClip PlayerRespawnedAudioClip;
    public AudioClip WeaponSpawnedAudioClip;
    public AudioClip WeaponPickedUpAudioClip;
    public AudioClip BazookaFiredAudioClip;
    public AudioClip FistGunBlockedAudioClip;
    public AudioClip FistGunFiredAudioClip;
    public AudioClip FistGunHitAudioClip;
    public AudioClip FistGunChargeAudioClip;
    public AudioClip FoodPickedUpCorrectAudioClip;
    public AudioClip FoodPickedUpWrongAudioClip;
    public AudioClip FoodDeliveredAudioClip;
    public AudioClip FistGunHitGroundAudioClip;
    public AudioClip FistHitGroundAudioClip;
    public AudioClip WaterGunHitGroundAudioClip;
    public AudioClip BazookaGunHitGroundAudioClip;
    public AudioClip HookGunHitGroundAudioClip;
    public AudioClip HookHitGroundAudioClip;
    public AudioClip SuckGunHitGroundAudioClip;
    public AudioClip[] FoodHitGroundAudioClip;
}
