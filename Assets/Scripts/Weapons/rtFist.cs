using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rtFist : WeaponBase
{
	private Transform _fist;
	private GameObject _fistDup;
	private Fist _fistScript;
	private FistGunState _fistGunState = FistGunState.Idle;
	private enum FistGunState
	{
		Idle,
		Out,
		Recharging,
	}
	private Vector3 _maxDistance;

	protected override void Awake()
	{
		base.Awake();
		_fist = transform.Find("Fist");
		Debug.Assert(_fist != null);
		_fistScript = _fist.GetComponent<Fist>();
		_ammo = WeaponDataStore.FistGunDataStore.MaxAmmo;
	}

	// Update is called once per frame
	void Update()
	{
		if (_fistGunState == FistGunState.Out)
		{
			Vector3 nextPos = (_maxDistance - _fistDup.transform.position).normalized;
			_fistDup.transform.Translate(nextPos * Time.deltaTime * WeaponDataStore.FistGunDataStore.FistSpeed, Space.World);
			RaycastHit hit;
			if (Physics.SphereCast(_fistDup.transform.position, 0.3f, _fistDup.transform.forward, out hit, 0.1f, Services.Config.ConfigData.AllPlayerLayer))
			{
				hit.collider.GetComponentInParent<PlayerController>().GetComponent<Rigidbody>().AddForce(_fistDup.transform.up * WeaponDataStore.FistGunDataStore.FistHitForce, ForceMode.Impulse);
				_switchToRecharge();
				return;
			}
			if (Vector3.Distance(_fistDup.transform.position, _maxDistance) <= 0.2f)
			{
				_switchToRecharge();
			}
		}
	}

	public override void Fire(bool buttondown)
	{
		if (buttondown)
		{
			if (_fistGunState == FistGunState.Idle)
			{
				_maxDistance = transform.position + -transform.right * WeaponDataStore.FistGunDataStore.MaxFlyDistance;
				_fistDup = Instantiate(_fist.gameObject, transform);
				_fistDup.transform.position = _fist.position;
				_fistDup.transform.parent = null;
				_fistDup.GetComponent<Collider>().isTrigger = false;
				_fist.gameObject.SetActive(false);
				/// Add Backfire force to player as well
				_gpc.Owner.GetComponent<Rigidbody>().AddForce(transform.right * WeaponDataStore.FistGunDataStore.BackfireHitForce, ForceMode.VelocityChange);
				_fistGunState = FistGunState.Out;
			}
		}
	}

	protected override void _onWeaponDespawn()
	{
		StopAllCoroutines();
		_fistGunState = FistGunState.Idle;
		if (_fistDup != null) Destroy(_fistDup);
		_fistDup = null;
		_fist.gameObject.SetActive(true);
		_ammo = WeaponDataStore.FistGunDataStore.MaxAmmo;
		gameObject.SetActive(false);
	}

	private void _switchToRecharge()
	{
		_fistGunState = FistGunState.Recharging;
		_fistDup.GetComponent<Rigidbody>().isKinematic = false;
		_onWeaponUsedOnce();
		StartCoroutine(_recharge(WeaponDataStore.FistGunDataStore.ReloadTime));
	}

	IEnumerator _recharge(float time)
	{
		yield return new WaitForSeconds(time);
		Destroy(_fistDup);
		_fistDup = null;
		_fist.gameObject.SetActive(true);
		_fistGunState = FistGunState.Idle;
	}
}
