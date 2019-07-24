using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;

public class rtHammer : WeaponBase
{
	private enum HammerState
	{
		Idle,
		Out,
	}

	private HammerState _hammerState = HammerState.Idle;
	private Player _player;
	private float _curTravelTime;
	private float _HLAxis { get { return _player.GetAxis("Move Horizontal"); } }
	private float _VLAxis { get { return _player.GetAxis("Move Vertical"); } }

	protected override void Awake()
	{
		base.Awake();
		_ammo = WeaponDataStore.HammerDataStore.MaxAmmo;
	}
	private void Update()
	{
		if (_hammerState == HammerState.Out)
		{
			_curTravelTime += Time.deltaTime;
			if (_curTravelTime >= WeaponDataStore.HammerDataStore.MaxTravelTime)
			{
				_onWeaponUsedOnce();
				_hammerState = HammerState.Idle;
				return;
			}
			transform.position += -transform.forward * WeaponDataStore.HammerDataStore.Speed * Time.deltaTime;
			if (!Mathf.Approximately(_HLAxis, 0f) || !Mathf.Approximately(0f, _VLAxis))
			{
				Vector3 transeuler = transform.eulerAngles;
				transeuler.y = Mathf.LerpAngle(transeuler.y, Mathf.Atan2(_HLAxis, _VLAxis * -1f) * Mathf.Rad2Deg, Time.deltaTime * WeaponDataStore.HammerDataStore.RotationSpeed);
				transform.eulerAngles = transeuler;
			}
		}
	}

	public override void Fire(bool buttondown)
	{
		if (buttondown)
		{
			_player = ReInput.players.GetPlayer(_gpc.Owner.GetComponent<PlayerController>().PlayerNumber);
			_gpc.FollowHand = false;
			_hammerState = HammerState.Out;
		}
	}

	protected override void _onWeaponDespawn()
	{
		_hammerState = HammerState.Idle;
		_ammo = WeaponDataStore.HammerDataStore.MaxAmmo;
		_curTravelTime = 0f;
		_player = null;
		_gpc.FollowHand = true;
	}
}
