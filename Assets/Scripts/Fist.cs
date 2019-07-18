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
		if (Services.Config.ConfigData.AllPlayerLayer != (Services.Config.ConfigData.AllPlayerLayer | (1 << other.gameObject.layer))
		   || other.gameObject.layer == _gpc.Owner.layer)
			return;
		CanHit = false;
	}
}
