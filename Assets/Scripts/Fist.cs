using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fist : MonoBehaviour
{
    [HideInInspector] public bool CanHit;
    private rtFist _rtf;

    private void Awake()
    {
        _rtf = GetComponentInParent<rtFist>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_rtf.Owner == null) return;
        if (!CanHit) return;
        if (Services.Config.ConfigData.AllPlayerLayer != (Services.Config.ConfigData.AllPlayerLayer | (1 << other.gameObject.layer))
           || other.gameObject.layer == _rtf.Owner.layer)
            return;
        CanHit = false;
    }
}
