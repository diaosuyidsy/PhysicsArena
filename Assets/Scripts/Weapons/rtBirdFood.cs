using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rtBirdFood : WeaponBase
{
	[HideInInspector]
	public int LastHolder = 7; // Initializ the Last Holder to an error value to assert game register before use

	protected override void Awake()
	{
		base.Awake();
		_ammo = 1;
	}
	public void RegisterLastHolder(int playernumber)
	{
		LastHolder = playernumber;
	}

	public override void Fire(bool buttondown)
	{
	}

	protected override void _onWeaponDespawn()
	{
		if (tag.Contains("1"))
		{
			GameManager.GM.Team1ResourceSpawnIndex = (GameManager.GM.Team1ResourceSpawnIndex + 1) % GameManager.GM.Team1ResourceRespawnPoints.Length;
			transform.position = GameManager.GM.Team1ResourceRespawnPoints[GameManager.GM.Team1ResourceSpawnIndex].transform.position;
		}
		else
		{
			GameManager.GM.Team2ResourceSpawnIndex = (GameManager.GM.Team2ResourceSpawnIndex + 1) % GameManager.GM.Team2ResrouceRespawnPoints.Length;
			transform.position = GameManager.GM.Team2ResrouceRespawnPoints[GameManager.GM.Team2ResourceSpawnIndex].transform.position;
		}

		EventManager.Instance.TriggerEvent(new ObjectDespawned(gameObject));
	}
}
