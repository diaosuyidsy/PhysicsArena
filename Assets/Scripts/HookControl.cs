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
        if (!CanHook) return;
        if (GameManager.GM.AllPlayers != (GameManager.GM.AllPlayers | (1 << other.gameObject.layer))
            || other.gameObject.layer == _gpc.Owner.layer)
            return;
        CanHook = false;
        _rth.HookOnHit(other.GetComponentInParent<PlayerController>().gameObject);
    }
}
