using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HookControl : MonoBehaviour
{
    private rtHook _rth;
    private GunPositionControl _gpc;

    private void Start()
    {
        _rth = GetComponentInParent<rtHook>();
        _gpc = GetComponentInParent<GunPositionControl>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (GameManager.GM.AllPlayers != (GameManager.GM.AllPlayers | (1 << other.gameObject.layer))
            || other.gameObject.layer == _gpc.Owner.layer)
            return;

        _rth.HookOnHit(other.GetComponentInParent<PlayerController>().gameObject);
    }
}
