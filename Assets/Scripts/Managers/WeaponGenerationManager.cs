using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class WeaponGenerationManager : MonoBehaviour
{
	public static WeaponGenerationManager WGM;

	public GameObject WeaponsHolder;
	[HideInInspector]
	public GameObject[] Weapons;
	public int NextSpawnAmount;
	public WeaponData WeaponDataStore;

	private int _activatedWeaponNum = 0;
	private int _nextspawnamount;
	private Vector3 _weaponSpawnerCenter = new Vector3(0f, 6.5f, 0f);
	private CameraController _cc;
	private bool _gamestart;

	private void Awake()
	{
		WGM = this;
		_cc = Camera.main.GetComponent<CameraController>();
	}

	private void Start()
	{
		_nextspawnamount = NextSpawnAmount;
		Weapons = new GameObject[WeaponsHolder.transform.childCount];
		for (int i = 0; i < Weapons.Length; i++)
		{
			Weapons[i] = WeaponsHolder.transform.GetChild(i).gameObject;
			Weapons[i].SetActive(false);
		}
	}

	private void Update()
	{
		if (_gamestart)
			_setWeaponSpawn();
	}

	// This function is called when the game actually starts (After all enter game)
	private void _onGameStart(GameStart gs)
	{
		// We need to invoke weapon generation from the start
		InvokeRepeating("GenerateWeapon", 5f, WeaponDataStore.WeaponSpawnCD);
		_gamestart = true;
	}

	private void GenerateWeapon()
	{
		// If next weapon in array is deactivated
		// Then move it to the current random spawn location
		// Then activate it
		// First we need to shuffle the array
		System.Random rng = new System.Random();
		for (int i = Weapons.Length - 1; i > 0; i--)
		{
			int j = rng.Next(0, i + 1);

			GameObject temp = Weapons[i];
			Weapons[i] = Weapons[j];
			Weapons[j] = temp;
		}
		// Then search through it
		for (int i = 0; i < Weapons.Length; i++)
		{
			GameObject weapon = Weapons[i];
			if (!weapon.activeSelf && _nextspawnamount > 0)
			{
				_moveWeaponToSpawnArea(weapon);
				weapon.SetActive(true);
				_nextspawnamount--;
			}
		}

		// If we want to change next spawn amount accordingly, do it here
		_nextspawnamount = NextSpawnAmount;
	}

	private void _moveWeaponToSpawnArea(GameObject weapon)
	{
		Vector3 weaponSpawnerSize = WeaponDataStore.WeaponSpawnerSize;
		Vector3 targetPos = _weaponSpawnerCenter + new Vector3(
			UnityEngine.Random.Range(-weaponSpawnerSize.x / 2, weaponSpawnerSize.x / 2),
			UnityEngine.Random.Range(-weaponSpawnerSize.y / 2, weaponSpawnerSize.y / 2),
			UnityEngine.Random.Range(-weaponSpawnerSize.z / 2, weaponSpawnerSize.z / 2));
		while (!Physics.Raycast(targetPos, -Vector3.up, 100f, WeaponDataStore.Ground))
		{
			targetPos = _weaponSpawnerCenter + new Vector3(
			UnityEngine.Random.Range(-weaponSpawnerSize.x / 2, weaponSpawnerSize.x / 2),
			UnityEngine.Random.Range(-weaponSpawnerSize.y / 2, weaponSpawnerSize.y / 2),
			UnityEngine.Random.Range(-weaponSpawnerSize.z / 2, weaponSpawnerSize.z / 2));
		}
		weapon.transform.position = targetPos;
	}

	//make the space for weapon to respawn (weapon-spawner) visible in scene
	private void OnDrawGizmosSelected()
	{
		Gizmos.color = new Color(1, 0, 0, 0.5f);
		Gizmos.DrawCube(_weaponSpawnerCenter, WeaponDataStore.WeaponSpawnerSize);
		Gizmos.color = new Color(0, 0, 0, 0.5f);
		Gizmos.DrawCube(WeaponDataStore.WorldCenter, WeaponDataStore.WorldSize);
	}

	// This method set the weaponspawn area to follow the center of the player
	// Also, clamp the weeaponspawn area to not let it exceed the boundaries of the world
	private void _setWeaponSpawn()
	{
		_weaponSpawnerCenter.x = _cc.FollowTarget.x;
		_weaponSpawnerCenter.z = _cc.FollowTarget.z;

		Vector3 WorldCenter = WeaponDataStore.WorldCenter;
		Vector3 WorldSize = WeaponDataStore.WorldSize;
		Vector3 WeaponSpawnerSize = WeaponDataStore.WeaponSpawnerSize;
		// Trying to clamp the weapon Spawn Area within the world space
		float xmin = WorldCenter.x - WorldSize.x / 2 + WeaponSpawnerSize.x / 2;
		float xmax = WorldCenter.x + WorldSize.x / 2 - WeaponSpawnerSize.x / 2;
		float zmin = WorldCenter.z - WorldSize.z / 2 + WeaponSpawnerSize.z / 2;
		float zmax = WorldCenter.z + WorldSize.z / 2 - WeaponSpawnerSize.z / 2;

		_weaponSpawnerCenter.x = Mathf.Clamp(_weaponSpawnerCenter.x, xmin, xmax);
		_weaponSpawnerCenter.z = Mathf.Clamp(_weaponSpawnerCenter.z, zmin, zmax);
	}

	private void OnEnable()
	{
		EventManager.Instance.AddHandler<GameStart>(_onGameStart);
	}

	private void OnDisable()
	{
		EventManager.Instance.RemoveHandler<GameStart>(_onGameStart);
	}
}
