﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WeaponData", menuName = "ScriptableObjects/WeaponData", order = 1)]
public class WeaponData : ScriptableObject
{
	public WaterGunData WaterGunDataStore;
	public SuckGunData SuckGunDataStore;
	public HookGunData HookGunDataStore;
	public FistGunData FistGunDataStore;
	public BazookaData BazookaDataStore;

	public LayerMask Ground;
	public LayerMask OnHitDisappear;
}
