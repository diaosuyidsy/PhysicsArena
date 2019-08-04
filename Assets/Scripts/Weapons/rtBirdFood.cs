using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rtBirdFood : WeaponBase
{
	[HideInInspector]
	public int LastHolder = 7; // Initializ the Last Holder to an error value to assert game register before use
	private Vector3 _originalPosition;

	protected override void Awake()
	{
		base.Awake();
		_originalPosition = new Vector3(transform.position.x, transform.position.y, transform.position.z);
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
		_originalPosition.y += 1f;
		transform.position = _originalPosition;

		EventManager.Instance.TriggerEvent(new ObjectDespawned(gameObject));
	}
}
