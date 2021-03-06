﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterMovementData", menuName = "ScriptableObjects/Character/MovementData", order = 1)]
public class CharacterMovementData : ScriptableObject
{
    public float WalkSpeed = 2f;
    public float MaxVelocityChange = 10f;
    public float InAirSpeedMultiplier = 0.5f;
    public float FacingCliffMultiplier = 0.3f;
    public float PickupSpeed = 2f;
    public float JumpCD = 0f;
    public float JumpForce = 230f;
    public LayerMask JumpMask;
    public float MinRotationSpeed = 4f;
    public float MaxRotationSpeed = 15f;
    public float DropWeaponForceThreshold = 500f;
    public float FrontIsCliff = 0.2f;
    public float CliffPreventionForce = 100f;
    public float CliffPreventionTimer = 0.4f;
}
