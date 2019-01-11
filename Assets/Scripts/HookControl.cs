using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HookControl : MonoBehaviour
{
    private rtHook _rth;

    private void Start()
    {
        _rth = GetComponentInParent<rtHook>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (GameManager.GM.AllPlayers != (GameManager.GM.AllPlayers | (1 << other.gameObject.layer)))
            return;

        _rth.HookOnHit(other.gameObject);
    }
}
