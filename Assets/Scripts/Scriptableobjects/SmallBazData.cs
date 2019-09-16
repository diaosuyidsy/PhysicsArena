using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[CreateAssetMenu(fileName = "SmallBazData", menuName = "ScriptableObjects/Weapon/SmallBazData", order = 1)]
public class SmallBazData : WeaponDataBase
{
	public float BoomerangSpeed = 9f;
	public float OnHitForce = 900f;
	public float CurveMultiplier = 1f;
	public Vector3[] LocalMovePoints;
	public float BoomerangYOffset;
	public AnimationCurve BoomEase;
	public int MaxAmmo = 1;
	public LayerMask CanHitLayer;
	public LayerMask ObstacleLayer;
	[Header("Hit Scan Config")]
	public float HitRadius = 0.5f;
	public float HitMaxDistance = 0.1f;
}
