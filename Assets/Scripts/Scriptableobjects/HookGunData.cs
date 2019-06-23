using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "HookGunData", menuName = "ScriptableObjects/Weapon/HookGun", order = 1)]
public class HookGunData : ScriptableObject
{
	public float HookSpeed = 12f;
	public int MaxHookTimes = 15;
	public float HookAwayForce = 450f;
	public float HookedTime = 0.25f;
}
