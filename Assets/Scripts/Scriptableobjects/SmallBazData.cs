using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[CreateAssetMenu(fileName = "SmallBazData", menuName = "ScriptableObjects/Weapon/SmallBazData", order = 1)]
public class SmallBazData : WeaponDataBase
{
	public float Radius;
	public float OnHitForce = 1200f;
	public float UpwardFinalY = 10f;
	public float UpwardDuration = 0.5f;
	public Ease UpwardEase = Ease.OutQuad;
	public float InAirDuration = 2f;
	public float DownwardAccelaration = 5f;
	public int MaxAmmo = 1;
	public LayerMask HitExplodeLayer;
	public LayerMask CanHideLayer;
	//[Header("Hit Scan Config")]
	//public float HitRadius = 0.5f;
	//public float HitMaxDistance = 0.1f;
}
