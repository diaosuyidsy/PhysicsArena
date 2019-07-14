using System.Collections;
using System.Collections.Generic;
using Obi;
using UnityEngine;

public class rtEmit : WeaponBase
{
	public ObiEmitter WaterBall;
	public GameObject WaterUI;
	public GameObject GunUI;

	private float _shootCD = 0f;

	private enum State
	{
		Empty,
		Shooting,
	}

	private State _waterGunState;

	protected override void Awake()
	{
		base.Awake();
		_waterGunState = State.Empty;
		_ammo = WeaponDataStore.WaterGunDataStore.MaxAmmo;
	}

	private void Update()
	{
		switch (_waterGunState)
		{
			case State.Shooting:
				_shootCD += Time.deltaTime;
				if (_shootCD >= WeaponDataStore.WaterGunDataStore.ShootMaxCD)
				{
					WaterBall.speed = 0f;
					_waterGunState = State.Empty;
					return;
				}
				// Statistics: Here we are using raycast for players hit
				_detectPlayer();
				// As long as player are actively spraying, should add that time to the record
				//_addToSprayTime();
				// Statistics: End
				//_ammo--;
				//if (_ammo <= 0)
				//{
				//	_gpc.CanBePickedUp = false;
				//	// If no ammo left, then drop the weapon
				//	_gpc.Owner.GetComponent<PlayerController>().DropHelper();
				//	_shootCD = 0f;
				//	WaterBall.speed = 0f;
				//}
				_onWeaponUsedOnce();
				// If we changed ammo, then need to change UI as well
				ChangeAmmoUI();
				break;
			case State.Empty:
				break;
		}
	}

	public override void Fire(bool buttondown)
	{
		/// means we pressed down button here
		if (buttondown)
		{
			_waterGunState = State.Shooting;
			GunUI.SetActive(true);
			WaterBall.speed = WeaponDataStore.WaterGunDataStore.Speed;
			if (_gpc != null)
			{
				_gpc.Owner.GetComponent<Rigidbody>().AddForce(-_gpc.Owner.transform.forward * WeaponDataStore.WaterGunDataStore.BackFireThrust, ForceMode.Impulse);
				_gpc.Owner.GetComponent<Rigidbody>().AddForce(_gpc.Owner.transform.up * WeaponDataStore.WaterGunDataStore.UpThrust, ForceMode.Impulse);
			}
			EventManager.Instance.TriggerEvent(new WaterGunFired(gameObject, _gpc.Owner, _gpc.Owner.GetComponent<PlayerController>().PlayerNumber));
		}
		else
		{
			_waterGunState = State.Empty;
			WaterBall.speed = 0f;
			_shootCD = 0f;
		}
	}

	//when gun leaves hands, UI disappears.
	public void KillUI()
	{
		GunUI.SetActive(false);
	}

	private void _detectPlayer()
	{
		// This layermask means we are only looking for Player1Body - Player6Body
		int layermask = (1 << 9) | (1 << 10) | (1 << 11) | (1 << 12) | (1 << 15) | (1 << 16);
		RaycastHit hit;
		if (Physics.Raycast(transform.position, -transform.right, out hit, Mathf.Infinity, layermask))
		{
			hit.transform.GetComponentInParent<PlayerController>().Mark(GetComponent<GunPositionControl>().Owner);
		}
	}

	private void _addToSprayTime()
	{
		int playerNumber = _gpc.Owner.GetComponent<PlayerController>().PlayerNumber;
		GameManager.GM.WaterGunUseTime[playerNumber] += Time.deltaTime;
	}

	private void ChangeAmmoUI()
	{
		float scaleY = _ammo * 1.0f / WeaponDataStore.WaterGunDataStore.MaxAmmo;
		WaterUI.transform.localScale = new Vector3(1f, scaleY, 1f);
	}

	protected override void _onWeaponDespawn()
	{
		_shootCD = 0f;
		WaterBall.speed = 0f;
		_ammo = WeaponDataStore.WaterGunDataStore.MaxAmmo;
		ChangeAmmoUI();
		EventManager.Instance.TriggerEvent(new ObjectDespawned(gameObject));
		gameObject.SetActive(false);
	}
}
