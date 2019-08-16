using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rtBirdFood : WeaponBase
{
    [HideInInspector]
    public int LastHolder = 7; // Initializ the Last Holder to an error value to assert game register before use
    [HideInInspector]
    public GameObject PickUpVFXHolder;
    private Vector3 _originalPosition;

    protected override void Awake()
    {
        base.Awake();
        _originalPosition = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        _ammo = 1;
        _hitGroundOnce = true;
    }

    public override void Fire(bool buttondown)
    {
    }

    protected override void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("DeathZone"))
        {
            _hitGroundOnce = false;
            _onWeaponDespawn();
            return;
        }
        if (other.tag.Contains("Collector"))
        {
            if ((other.GetComponent<ResourceCollector>().Team == TeamNum.Team1 && tag.Contains("2"))
             || (other.GetComponent<ResourceCollector>().Team == TeamNum.Team2 && tag.Contains("1"))) _onWeaponDespawn();
        }
    }

    protected override void _onWeaponDespawn()
    {
        EventManager.Instance.TriggerEvent(new ObjectDespawned(gameObject));

        _originalPosition.y += 1f;
        transform.position = _originalPosition;
        GetComponent<Rigidbody>().velocity = Vector3.zero;
    }

    public override void OnDrop()
    {
        LastHolder = Owner.GetComponent<PlayerController>().PlayerNumber;
        base.OnDrop();
    }
}
