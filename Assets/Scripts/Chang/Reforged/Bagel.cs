using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bagel : WeaponBase
{
    public bool Hold;

    protected override void Awake()
    {
        base.Awake();
        _hitGroundOnce = true;
    }

    public override void OnPickUp(GameObject owner)
    {
        base.OnPickUp(owner);
        Hold = true;
    }

    public override void OnDrop()
    {
        base.OnDrop();
        Hold = false;
    }

    public override void Fire(bool buttondown)
    {
        
    }

    public void OnSucked()
    {
        gameObject.layer = 2;
        GetComponent<BoxCollider>().enabled = false;
        GetComponent<Rigidbody>().useGravity = false;
        GetComponent<Rigidbody>().velocity = Vector3.zero;
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
        /*if (other.tag.Contains("Collector"))
        {
            EventManager.Instance.TriggerEvent(new BagelSent(other.transform.parent.gameObject));
            Destroy(gameObject);
        }*/
    }
}
