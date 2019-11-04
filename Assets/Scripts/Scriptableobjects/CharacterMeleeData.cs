using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterMeleeData", menuName = "ScriptableObjects/Character/MeleeData", order = 1)]
public class CharacterMeleeData : ScriptableObject
{
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
}
