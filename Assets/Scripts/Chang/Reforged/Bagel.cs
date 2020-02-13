using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bagel : WeaponBase
{
    protected override void Awake()
    {
        base.Awake();
        _hitGroundOnce = true;
    }

    public override void Fire(bool buttondown)
    {
        
    }

    protected override void _onWeaponDespawn()
    {

    }

    protected override void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("DeathZone"))
        {
            _hitGroundOnce = false;
            EventManager.Instance.TriggerEvent(new ObjectDespawned(gameObject));
            EventManager.Instance.TriggerEvent(new BagelDespawn());
            return;
        }
        if (other.tag.Contains("Collector"))
        {
            EventManager.Instance.TriggerEvent(new BagelSent(other.transform.parent.gameObject));
            Destroy(gameObject);
        }
    }
}
