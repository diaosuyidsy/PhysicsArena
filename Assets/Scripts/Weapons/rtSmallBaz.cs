using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class rtSmallBaz : WeaponBase
{
	private enum SmallBazState
	{
		In,
		Out,
	}
	private SmallBazState _boomerangState;
	private Tweener _pathMoveTweener;
	private GameObject _firer;
	private HashSet<PlayerController> _hitSet;

	protected override void Awake()
	{
		base.Awake();
		_ammo = WeaponDataStore.BoomerangDataStore.MaxAmmo;
		_boomerangState = SmallBazState.In;
		_hitSet = new HashSet<PlayerController>();
	}

	protected override void Update()
	{
		base.Update();
		if (_boomerangState == SmallBazState.Out)
		{

		}
	}

	public override void Fire(bool buttondown)
	{
		if (!buttondown)
		{
			_firer = Owner;
			_boomerangState = SmallBazState.Out;
			Vector3[] localPath = new Vector3[WeaponDataStore.BoomerangDataStore.LocalMovePoints.Length];
			for (int i = 0; i < WeaponDataStore.BoomerangDataStore.LocalMovePoints.Length; i++)
			{
				localPath[i] = transform.position + transform.forward * WeaponDataStore.BoomerangDataStore.LocalMovePoints[i].z + transform.right * WeaponDataStore.BoomerangDataStore.LocalMovePoints[i].x;
			}
			_pathMoveTweener = transform.DOLocalPath(localPath, WeaponDataStore.BoomerangDataStore.BoomerangSpeed, PathType.CatmullRom)
			.SetSpeedBased(true)
			.SetEase(WeaponDataStore.BoomerangDataStore.BoomEase)
			.SetLookAt(0f)
			.OnComplete(() =>
			{
				GetComponent<Rigidbody>().velocity = transform.forward * WeaponDataStore.BoomerangDataStore.BoomerangSpeed;
			});
			_onWeaponUsedOnce();
		}
	}

	protected override void _onWeaponDespawn()
	{
		_boomerangState = SmallBazState.In;
		_ammo = WeaponDataStore.BoomerangDataStore.MaxAmmo;
		_hitSet.Clear();
		EventManager.Instance.TriggerEvent(new ObjectDespawned(gameObject));
		gameObject.SetActive(false);
	}
}
