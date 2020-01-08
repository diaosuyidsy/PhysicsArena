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
    public float JumpStaminaDrain = 0f;
    public float JumpForce = 230f;
    public LayerMask JumpMask;
    public float MinRotationSpeed = 4f;
    public float MaxRotationSpeed = 15f;
    public float DropWeaponForceThreshold = 500f;
    public float FrontIsCliff = 0.2f;
    public float CliffPreventionForce = 100f;
    public float CliffPreventionTimer = 0.4f;

    [Header("Character Defend Related Settings")]
    public float MaxStamina = 1.5f;
    public float StaminaRegenInterval = 3f;
    public float StaminaRegenRate = 1f;
    public float BlockUILingerDuration = 0.2f;
    public float ArmTargetPosition = 100f;
    public float HandTargetPosition = 120f;
    public float BlockStaminaDrain = 1f;
    public float BlockMultiplier = 2f;
    public float BlockAngle = 90f;
    public bool IsSideStepping = false;
    public float SideSteppingInitForce = 300f;
    public float SideSteppingDuration = 0.2f;
    public float SideSteppingCD = 0.5f;
    public float SideSteppingStaminaDrain = 0.3f;

    [Header("Character Attack Related Settings")]

    [Tooltip("How much time it takes to charge to full")]
    public float ClockFistTime = 1f;
    [Tooltip("How much time it takes to release the punch (before it resets to normal state)")]
    public float FistReleaseTime = 0.2f;
    [Tooltip("Self Push Force adjust how much yourself will be launched forward when punching")]
    public float SelfPushForce = 200f;
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
    public LayerMask CanHitLayer;
    public bool IsButtHitting;
    public float ButtAnticipationDuration = 0.3f;
    public float ButtAnticipationForwardPush = 50f;
    public float ButtStrikeDuration = 0.2f;
    public float ButtStrikeStrength = 500f;
    public float ButtStrikeRaidus = 0.3f;
    public float ButtStrikeDistance = 0.05f;
    public float ButtStrikeStopDistance = 0.05f;
    public float ButtStrikeForwardPush = 3f;
    public Ease ButtStrikePushEase = Ease.OutQuad;
    public float ButtRecoveryDuration = 0f;
    public LayerMask ButtHitStopLayer;

    [Header("Character Other Settings")]
    public float Radius = 1f;
    public LayerMask PickUpLayer;
}
