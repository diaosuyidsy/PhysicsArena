using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "HammerData", menuName = "ScriptableObjects/Weapon/Hammer", order = 1)]
public class HammerData : ScriptableObject
{
	public int MaxAmmo = 1;
	public float MaxTravelTime = 10f;
	public float Speed = 4f;
	public float RotationSpeed = 0.1f;
	public LayerMask CanCollideLayer;
}
