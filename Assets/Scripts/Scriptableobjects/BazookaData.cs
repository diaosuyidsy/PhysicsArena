﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BazookaData", menuName = "ScriptableObjects/Weapon/Bazooka", order = 1)]
public class BazookaData : ScriptableObject
{
	public int MaxAmmo = 1;
	public float MaxAffectionRange = 5f;
	public float MaxAffectionForce = 800f;
	[Header("The force player experienced would be Range * related multiplier * force")]
	public float MaxAffectionMultiplier = 1f;
	public float MinAffectionMultiplier = 0.2f;
	public float MarkMoveSpeed = 10f;
	public float MarkThrowThurst = 10f;
	public float MarkGravityScale = 1f;
	public LayerMask CanHideLayer;
	public LayerMask LineCastLayer;
	public LayerMask HitExplodeLayer;
}
