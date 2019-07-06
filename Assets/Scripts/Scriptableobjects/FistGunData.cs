using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FistGunData", menuName = "ScriptableObjects/Weapon/FistGun", order = 1)]
public class FistGunData : ScriptableObject
{
	public float FistSpeed = 12f;
	public int MaxHookTimes = 15;
	public float FistHitForce = 900f;
	public float MaxFlyDistance = 10f;
	public float ReloadTime = 2f;
}
