using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HookControl : MonoBehaviour
{
    [HideInInspector]
    public bool CanHook = false;
    private rtHook _rth;
    private bool _hitGround;

    private void Awake()
    {
        _rth = GetComponentInParent<rtHook>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_rth.Owner == null) return;
        if (other.gameObject.layer == _rth.Owner.layer)
        {
            _rth.CanCarryBack = false;
        }
        if (!CanHook) return;

        if (Services.Config.ConfigData.AllPlayerLayer != (Services.Config.ConfigData.AllPlayerLayer | (1 << other.gameObject.layer))
            || other.gameObject.layer == _rth.Owner.layer)
            return;
        if (other.GetComponent<WeaponBase>() != null) return;
        CanHook = false;
        _rth.HookOnHit(other.GetComponentInParent<PlayerController>().gameObject);
    }
}
