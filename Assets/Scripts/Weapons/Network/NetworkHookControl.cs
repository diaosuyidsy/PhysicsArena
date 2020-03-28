using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkHookControl : MonoBehaviour
{
    private NetworkRtHook _rth;

    private void Awake()
    {
        _rth = GetComponentInParent<NetworkRtHook>();
    }

    // private void OnTriggerEnter(Collider other)
    // {
    //     if (!_rth.hasAuthority) return;
    //     if (_rth._hookGunData.HookableLayer != (_rth._hookGunData.HookableLayer | (1 << other.gameObject.layer))
    //         || other.gameObject.layer == _rth.Owner.layer)
    //         return;
    //     if (other.GetComponent<IHittableNetwork>() == null) return;
    //     _rth.CmdHookOnHit(other.gameObject);
    // }
}
