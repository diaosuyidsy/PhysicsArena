using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HookControl : MonoBehaviour
{
    [HideInInspector]
    public bool CanHook = false;
    private rtHook _rth;
    private GunPositionControl _gpc;
    private bool _hitGround;

    private void Awake()
    {
        _rth = GetComponentInParent<rtHook>();
        _gpc = GetComponentInParent<GunPositionControl>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_gpc.Owner == null) return;
        if (other.gameObject.layer == _gpc.Owner.layer)
        {
            _rth.CanCarryBack = false;
        }
        if (!CanHook) return;

        if (Services.Config.ConfigData.AllPlayerLayer != (Services.Config.ConfigData.AllPlayerLayer | (1 << other.gameObject.layer))
            || other.gameObject.layer == _gpc.Owner.layer)
            return;
        if (other.GetComponent<WeaponBase>() != null) return;
        CanHook = false;
        _rth.HookOnHit(other.GetComponentInParent<PlayerController>().gameObject);
    }
}
