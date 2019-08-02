﻿using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class WeaponGenerationManager
{
	private WeaponData WeaponDataStore;
	private Vector3 _weaponSpawnerCenter = new Vector3(0f, 6.5f, 0f);
	private CameraController _cc;
	private bool _gamestart;
	private float _spawnTimer;
	private GameMapData GameMapData;

	private List<int> _weaponBag;
	private List<List<GameObject>> _curWeapons;
	private int _totalActiveWeaponCount
	{
		get
		{
			int count = 0;
			foreach (List<GameObject> gos in _curWeapons)
			{
				for (int i = 0; i < gos.Count; i++)
				{
					if (gos[i].activeInHierarchy) count++;
				}
			}
			return count;
		}
	}

	public WeaponGenerationManager(GameMapData gmp, WeaponData _wd, GameObject _weaponsHolder)
	{
		WeaponDataStore = _wd;
		GameMapData = gmp;
		EventManager.Instance.AddHandler<GameStart>(_onGameStart);
		_cc = Camera.main.GetComponent<CameraController>();
		_weaponBag = new List<int>();
		_shuffleWeaponBag();
		_curWeapons = new List<List<GameObject>>();
		for (int i = 0; i < GameMapData.WeaponsInformation.Length + 1; i++)
		{
			_curWeapons.Add(new List<GameObject>());
		}
		for (int i = 0; i < _weaponsHolder.transform.childCount; i++)
		{
			_curWeapons[GameMapData.WeaponsInformation.Length].Add(_weaponsHolder.transform.GetChild(i).gameObject);
			_weaponsHolder.transform.GetChild(i).gameObject.SetActive(false);
		}
	}

	private void _shuffleWeaponBag()
	{
		for (int i = 0; i < GameMapData.WeaponsInformation.Length; i++)
		{
			for (int j = 0; j < GameMapData.WeaponsInformation[i].WeaponSetNumber; j++)
			{
				_weaponBag.Add(i);
			}
		}
		for (int i = 0; i < GameMapData.WaterGunSetAmount; i++)
		{
			_weaponBag.Add(GameMapData.WeaponsInformation.Length);
		}
	}

	public void Update()
	{
		if (_gamestart)
		{
			_setWeaponSpawn();
			_spawnTimer += Time.deltaTime;
			if (_spawnTimer > GameMapData.WeaponSpawnCD && _totalActiveWeaponCount < GameMapData.MaxAmountWeaponAtOnetime)
			{
				_generateWeapon();
				_spawnTimer = 0f;
			}
		}
	}

	// This function is called when the game actually starts (After all enter game)
	private void _onGameStart(GameStart gs)
	{
		// We need to invoke weapon generation from the start
		_gamestart = true;
	}

	private void _generateWeapon()
	{
		Debug.Assert(_weaponBag.Count > 0, "Weapon Bag's count must be larger than 0");
		int rand = UnityEngine.Random.Range(0, _weaponBag.Count);
		int index = _weaponBag[rand];
		_weaponBag.RemoveAt(rand);
		if (_weaponBag.Count == 0) _shuffleWeaponBag();
		/// We would like to generate weapon at index
		/// If it can be object pooled, then no need to instantiate a new one
		bool hasInactiveWeapon = false;
		foreach (GameObject weapon in _curWeapons[index])
		{
			if (!weapon.activeInHierarchy)
			{
				_moveWeaponToSpawnArea(weapon);
				weapon.SetActive(true);
				hasInactiveWeapon = true;
				break;
			}
		}
		/// If it cannot be object pooled, then need to instantitate a new one
		if (!hasInactiveWeapon && index != GameMapData.WeaponsInformation.Length)
		{
			GameObject weapon = GameObject.Instantiate(GameMapData.WeaponsInformation[index].WeaponPrefab);
			_moveWeaponToSpawnArea(weapon);
			_curWeapons[index].Add(weapon);
		}
	}

	private void _moveWeaponToSpawnArea(GameObject weapon)
	{
		Vector3 weaponSpawnerSize = GameMapData.WeaponSpawnerSize;
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

	// This method set the weaponspawn area to follow the center of the player
	// Also, clamp the weeaponspawn area to not let it exceed the boundaries of the world
	private void _setWeaponSpawn()
	{
		_weaponSpawnerCenter.x = _cc.FollowTarget.x;
		_weaponSpawnerCenter.z = _cc.FollowTarget.z;

		Vector3 WorldCenter = GameMapData.WorldCenter;
		Vector3 WorldSize = GameMapData.WorldSize;
		Vector3 WeaponSpawnerSize = GameMapData.WeaponSpawnerSize;
		// Trying to clamp the weapon Spawn Area within the world space
		float xmin = WorldCenter.x - WorldSize.x / 2 + WeaponSpawnerSize.x / 2;
		float xmax = WorldCenter.x + WorldSize.x / 2 - WeaponSpawnerSize.x / 2;
		float zmin = WorldCenter.z - WorldSize.z / 2 + WeaponSpawnerSize.z / 2;
		float zmax = WorldCenter.z + WorldSize.z / 2 - WeaponSpawnerSize.z / 2;

		_weaponSpawnerCenter.x = Mathf.Clamp(_weaponSpawnerCenter.x, xmin, xmax);
		_weaponSpawnerCenter.z = Mathf.Clamp(_weaponSpawnerCenter.z, zmin, zmax);
	}

	public void Destroy()
	{
		EventManager.Instance.RemoveHandler<GameStart>(_onGameStart);
	}
}
