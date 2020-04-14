using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HookControl : MonoBehaviour
{
    private rtHook _rth;

    private void Awake()
    {
        _rth = GetComponentInParent<rtHook>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_rth == null) return;
        if (_rth.Owner == null) return;
        if (!_rth.CanHook) return;
        if (_rth._hookGunData.HookableLayer != (_rth._hookGunData.HookableLayer | (1 << other.gameObject.layer))
            || other.gameObject.layer == _rth.Owner.layer)
            return;
        if (other.GetComponent<WeaponBase>() != null) return;
        if (other.GetComponent<IHittable>() == null) return;
        _rth.HookOnHit(other.gameObject);
    }
}
