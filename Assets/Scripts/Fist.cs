using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fist : MonoBehaviour
{
	[HideInInspector] public bool CanHit;
	private rtFist _rtf;
	private GunPositionControl _gpc;

	private void Awake()
	{
		_rtf = GetComponentInParent<rtFist>();
		_gpc = GetComponentInParent<GunPositionControl>();
	}

	private void OnTriggerEnter(Collider other)
	{
		if (_gpc.Owner == null) return;
		if (!CanHit) return;
		if (GameManager.GM.AllPlayers != (GameManager.GM.AllPlayers | (1 << other.gameObject.layer))
		   || other.gameObject.layer == _gpc.Owner.layer)
			return;
		CanHit = false;
	}
}
