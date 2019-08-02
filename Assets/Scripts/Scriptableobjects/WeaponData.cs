using System.Collections;
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

	public float WeaponSpawnCD = 12f;
	public LayerMask Ground;
	public LayerMask OnHitDisappear;

	//teleport a weapon to a random position within a given space
	[Header("Weapon Spawn Setting")]
	public Vector3 WeaponSpawnerSize;

	// Need to manually set up world center until we figure out a way to automatically do it.
	public Vector3 WorldCenter;
	public Vector3 WorldSize;

	[Header("Weapon Prefabs")]
	public GameObject SuckGunPrefab;
	public GameObject HookGunPrefab;
	public GameObject FistGunPrefab;
	public GameObject BazookaPrefab;
}
