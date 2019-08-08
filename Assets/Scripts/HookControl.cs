using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HookControl : MonoBehaviour
{
    [HideInInspector]
    public bool CanHook = false;
    private rtHook _rth;
    private GunPositionControl _gpc;

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
        CanHook = false;
        if (other.GetComponent<WeaponBase>() != null) return;
        _rth.HookOnHit(other.GetComponentInParent<PlayerController>().gameObject);
    }
}
