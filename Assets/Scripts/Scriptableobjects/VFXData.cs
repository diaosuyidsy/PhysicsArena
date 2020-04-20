using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "VFXData", menuName = "ScriptableObjects/VFXData", order = 1)]
public class VFXData : ScriptableObject
{
    public GameObject DeliverFoodVFX;
    public GameObject[] DeathVFX;
    public GameObject[] HugeDeathVFX;
    public GameObject VanishVFX;
    [Tooltip("在击打位置的VFX")]
    public GameObject[] ChickenHitVFX;
    public GameObject[] DuckHitVFX;

    [Tooltip("被打的人脚下的VFX，是子物体")]
    public GameObject[] ChickenHittedFeetVFX;
    public GameObject[] DuckHittedFeetVFX;
    [Tooltip("被打的人身上的VFX")]
    public GameObject[] ChickenHittedBodyVFX;
    public GameObject[] DuckHittedBodyVFX;
    [Tooltip("被打的时候Camera VFX，子物体")]
    public GameObject[] ChickenHittedCameraVFX;
    [Tooltip("被打的时候Camera VFX，子物体")]
    public GameObject[] DuckHittedCameraVFX;
    public GameObject JumpGrassVFX;
    public GameObject JumpConcreteVFX;
    public GameObject JumpYellowStoneVFX;
    public GameObject LandGrassVFX;
    public GameObject LandConcreteVFX;
    public GameObject LandYellowStoneVFX;
    public GameObject ChickenMeleeChargingVFX;
    public GameObject ChickenUltimateVFX;
    public GameObject DuckMeleeChargingVFX;
    public GameObject DuckUltimateVFX;
    public GameObject[] ChickenReleasePunchHandVFX;
    public GameObject[] DuckReleasePunchHandVFX;
    public GameObject[] ChickenReleasePunchFootVFX;
    public GameObject[] DuckReleasePunchFootVFX;
    public GameObject[] ChickenReleaseBodyVFX;
    public GameObject[] DuckReleaseBodyVFX;
    public GameObject BazookaExplosionVFX;
    public GameObject BazookaTrailVFX;
    public GameObject ChickenBlockVFX;
    public GameObject DuckBlockVFX;
    public GameObject ChickenBlockUIVFX;
    public GameObject DuckBlockUIVFX;
    public GameObject ChickenStunnedVFX;
    public GameObject DuckStunnedVFX;
    public GameObject ChickenSlowedVFX;
    public GameObject DuckSlowedVFX;
    public GameObject CartExplosionVFX;
    public GameObject BagelExplosionVFX;
    public GameObject ChickenFoodVFX;
    public GameObject DuckFoodVFX;
    public GameObject FistGunFistTrailVFX;
    public GameObject ChickenFoodGuideVFX;
    public GameObject DuckFoodGuideVFX;
    public GameObject[] ChickenBlockParryVFX;
    public GameObject[] DuckBlockParryVFX;
    public GameObject ChickenLeftFootStepVFX;
    public GameObject ChickenRightFootStepVFX;
    public GameObject DuckLeftFootStepVFX;
    public GameObject DuckRightFootStepVFX;
    public GameObject[] ChickenFootVFX;
    public GameObject[] DuckFootVFX;
    public GameObject[] FistGunHitVFX;
    public GameObject[] HookGunFireVFX;
    public GameObject[] HookGunHitVFX;
    public LayerMask HitBlockedLayer;
    public GameObject[] EmojiVFXs;

}
