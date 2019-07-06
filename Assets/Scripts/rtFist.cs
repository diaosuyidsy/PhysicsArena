using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rtFist : MonoBehaviour
{
	public WeaponData WeaponDataStore;

	private GunPositionControl _gpc;
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

	private void Awake()
	{
		_gpc = GetComponent<GunPositionControl>();
		Debug.Assert(_gpc != null);
		_fist = transform.Find("Fist");
		Debug.Assert(_fist != null);
		_fistScript = _fist.GetComponent<Fist>();
	}

    // Update is called once per frame
    void Update()
    {
        if(_fistGunState == FistGunState.Out)
		{
			Vector3 nextPos = (_maxDistance - _fistDup.transform.position).normalized;
			_fistDup.transform.Translate(nextPos * Time.deltaTime * WeaponDataStore.FistGunDataStore.FistSpeed, Space.World);
			RaycastHit hit;
			if(Physics.SphereCast(_fistDup.transform.position, 0.3f, _fistDup.transform.forward, out hit, 0.1f, GameManager.GM.AllPlayers))
			{
				hit.collider.GetComponentInParent<PlayerController> ().GetComponent<Rigidbody>().AddForce(_fistDup.transform.up * WeaponDataStore.FistGunDataStore.FistHitForce, ForceMode.Impulse);
				_switchToRecharge();
				return;
			}
			if (Vector3.Distance(_fistDup.transform.position, _maxDistance) <= 0.2f)
			{
				_switchToRecharge();
			}
		}
    }

	public void Fist(bool buttondown)
	{
		if (buttondown)
		{
			if(_fistGunState == FistGunState.Idle)
			{
				_maxDistance = transform.position + transform.forward * WeaponDataStore.FistGunDataStore.MaxFlyDistance;
				_fistDup = Instantiate(_fist.gameObject, transform);
				_fistDup.transform.position = _fist.position;
				_fistDup.transform.parent = null;
				_fist.gameObject.SetActive(false);
				_fistGunState = FistGunState.Out;
			}
		}
	}

	private void _switchToRecharge()
	{
		_fistGunState = FistGunState.Recharging;
		_fistDup.GetComponent<Rigidbody>().isKinematic = false;
		Destroy(_fistDup, WeaponDataStore.FistGunDataStore.ReloadTime);
		StartCoroutine(_recharge(WeaponDataStore.FistGunDataStore.ReloadTime));
		_fistDup = null;
	}

	IEnumerator _recharge(float time)
	{
		yield return new WaitForSeconds(time);
		_fist.gameObject.SetActive(true);
		_fistGunState = FistGunState.Idle;
	}
}
