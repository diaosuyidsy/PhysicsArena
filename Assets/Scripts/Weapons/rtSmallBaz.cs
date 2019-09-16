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
	private SmallBazState _smallBazState;
	private GameObject _firer;
	private SmallBazData _weaponData;

	protected override void Awake()
	{
		base.Awake();
		_weaponData = WeaponDataBase as SmallBazData;
		_ammo = _weaponData.MaxAmmo;
		_smallBazState = SmallBazState.In;
	}

	protected override void Update()
	{
		base.Update();
		if (_smallBazState == SmallBazState.Out)
		{
			RaycastHit hit;
			if (Physics.SphereCast(transform.position, 0.5f, Vector3.down, out hit, 0.5f, _weaponData.HitExplodeLayer))
			{
				List<GameObject> affectedPlayers = new List<GameObject>();
				// Hit Ground/Player/Obstacle
				foreach (PlayerController _pc in Services.GameStateManager.PlayerControllers)
				{
					float dis = Vector3.Distance(_pc.transform.position, transform.position);
					if (_pc.gameObject.activeInHierarchy &&
						dis < _weaponData.Radius &&
						_pc.gameObject != Owner &&
						!Physics.Linecast(_pc.transform.position, transform.position, _weaponData.CanHideLayer))
					{
						affectedPlayers.Add(_pc.gameObject);
						Vector3 dir = _pc.transform.position - transform.position;
						dir.y = 0f;
						_pc.OnImpact(_weaponData.OnHitForce * dir.normalized, ForceMode.Impulse, _firer, ImpactType.SmallBaz);
					}
				}
				EventManager.Instance.TriggerEvent(new BazookaBombed(gameObject, _firer, _firer.GetComponent<PlayerController>().PlayerNumber, affectedPlayers));
				_smallBazState = SmallBazState.In;
			}
		}
	}

	private void FixedUpdate()
	{
		if (_smallBazState == SmallBazState.Out)
		{
			GetComponent<Rigidbody>().AddForce(Vector3.down * _weaponData.DownwardAccelaration);
		}
	}

	public override void Fire(bool buttondown)
	{
		if (buttondown)
		{
			_firer = Owner;
			_onWeaponUsedOnce();
			GetComponent<Rigidbody>().isKinematic = true;
			Vector3 UpwardFinalPos = new Vector3(transform.localPosition.x, _weaponData.UpwardFinalY, transform.localPosition.z);
			Sequence seq = DOTween.Sequence();
			seq.Append(transform.DOLocalMove(UpwardFinalPos, _weaponData.UpwardDuration).SetEase(_weaponData.UpwardEase));
			seq.AppendCallback(() =>
			{
				GetComponent<Rigidbody>().isKinematic = false;
				GetComponent<Rigidbody>().velocity = Vector3.zero;
				GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
				_smallBazState = SmallBazState.Out;
			});
		}
	}

	protected override void _onWeaponDespawn()
	{
		_smallBazState = SmallBazState.In;
		_ammo = _weaponData.MaxAmmo;
		EventManager.Instance.TriggerEvent(new ObjectDespawned(gameObject));
		gameObject.SetActive(false);
	}
}
