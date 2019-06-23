using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterMovementData", menuName = "ScriptableObjects/Character/MovementData", order = 1)]
public class CharacterMovementData : ScriptableObject
{
	public float Thrust = 100f;
	public float WalkSpeed = 2f;
	public float PickupSpeed = 2f;
	public float JumpForce = 230f;
	public LayerMask JumpMask;
	public float MinRotationSpeed = 4f;
	public float MaxRotationSpeed = 15f;
}
