using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[CreateAssetMenu(fileName = "CharacterData", menuName = "ScriptableObjects/CharacterData", order = 1)]
public class CharacterData : ScriptableObject
{
    [Header("Character Movement Related Settings")]
    public float WalkSpeed = 2f;
    public float MaxVelocityChange = 10f;
    public float InAirSpeedMultiplier = 0.5f;
    public float FacingCliffMultiplier = 0.3f;
    public float PickupSpeed = 2f;
    public float JumpCD = 0f;
    public float JumpForce = 230f;
    public LayerMask JumpMask;
    public float MinRotationSpeed = 4f;
    public float FrontIsCliff = 0.2f;
    public float CliffPreventionForce = 100f;
    public float CliffPreventionTimer = 0.4f;

    [Header("Character Defend Related Settings")]
    public float MaxStamina = 1.5f;
    public float StaminaRegenInterval = 3f;
    public float StaminaRegenRate = 1f;
    public float BlockLingerDuration = 0.2f;
    public float BlockStaminaDrain = 1f;
    public float BlockMultiplier = 2f;
    public float BlockStunDuration = 1.3f;
    public float BlockAngle = 90f;
    public float BlockSpeedMultiplier = 0.5f;
    public float MinBlockUpTime = 0.3f;
    public float BlockPushForce = 30f;

    [Header("Character Attack Related Settings")]

    [Tooltip("How much time it takes to charge to full")]
    public float ClockFistTime = 1f;
    [Tooltip("How much time it takes to release the punch (before it resets to normal state)")]
    public float FistReleaseTime = 0.2f;
    [Tooltip("Actual Fist Punchable Time")]
    public float PunchActivateTime = 0.2f;
    [Tooltip("Self Push Force adjust how much yourself will be launched forward when punching")]
    public float SelfPushForce = 200f;
    public float IdleSelfPushForce = 65f;
    public float FistHoldSpeedMultiplier = 0.5f;
    public float FistHoldRotationMutiplier = 1.5f;
    public float PunchBackwardCastDistance = 0.3f;
    public float PunchHelpAimDistance = 4f;
    public float PunchHelpAimAngle = 60f;
    [Tooltip("MeleeCharge Threshold must be over this to take effect")]
    public float MeleeChargeThreshold = 0.75f;
    [Tooltip("The Radius of punching")]
    public float PunchRadius = 0.3f;
    [Tooltip("The Raycast Distance of Punching")]
    public float PunchDistance = 0.05f;
    [Tooltip("How much force player experience when getting punched")]
    public float PunchForce = 800f;
    [Tooltip("How much time player can hold the punch, default is infinity")]
    public float MeleeHoldTime = Mathf.Infinity;
    public float HitUncontrollableTimeSmall = 0.5f;
    public int HitStopFramesSmall = 6;
    public float HitSmallThreshold = 500f;
    public float HitUncontrollableTimeBig = 0.5f;
    public float HitBigThreshold = 900f;
    public int HitStopFramesBig = 12;

    public float PunchReleaseRotationMultiplier = 0.2f;
    public float PunchResetVelocityBeforeHitDuration = 0.5f;
    public LayerMask CanHitLayer;

    [Header("Character Other Settings")]
    public float Radius = 1f;
    public float PickUpCD = 0f;
    public LayerMask PickUpLayer;
    public float DropRecoveryTime = 0.2f;
    public Vector3 HitStopViberation = new Vector3(1f, 0.1f, 1f);
    public int HitStopViberato = 90;
    public float HitStopRandomness = 90f;
    public Ease HitStopViberationEase = Ease.OutBack;
    public float HitSweepBackwardDistance = 0.2f;
    public float HitSweepPushBackForce = 100f;
}
