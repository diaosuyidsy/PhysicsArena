using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    public WeaponData WeaponDataStore;
    public WeaponDataBase WeaponDataBase;
    public virtual float HelpAimAngle { get; }
    public virtual float HelpAimDistance { get; }

    public GameObject Owner { get; protected set; }
    protected int _ammo { get; set; }
    protected bool _hitGroundOnce;
    public bool CanBePickedUp;
    protected bool _followHand;

    protected virtual void Awake()
    {
        OnSpawn();
    }

    protected virtual void Update()
    {
        if (Owner != null && _followHand)
        {
            Vector3 targetposition = (Owner.GetComponent<PlayerController>().LeftHand.transform.position
            + Owner.GetComponent<PlayerController>().RightHand.transform.position) / 2f;
            transform.position = targetposition;
            transform.position += transform.right * WeaponDataBase.XOffset;
            transform.position += transform.up * WeaponDataBase.YOffset;
            transform.position += transform.forward * WeaponDataBase.ZOffset;
            transform.eulerAngles = new Vector3(WeaponDataBase.XRotation, Owner.transform.eulerAngles.y + WeaponDataBase.YRotation, WeaponDataBase.ZRotation);
        }
    }

    /// <summary>
    /// The behavior after user interaction, mostly RT
    /// </summary>
    /// <param name="buttondown"></param>
    public abstract void Fire(bool buttondown);

    /// <summary>
    /// Clean up after the weapon is despawned
    /// Return it to it's initial state
    /// </summary>
    protected abstract void _onWeaponDespawn();

    protected void _onWeaponUsedOnce()
    {
        _ammo--;
        if (_ammo <= 0)
        {
            CanBePickedUp = false;
            EventManager.Instance.TriggerEvent(new WeaponUsedUp());
            if (Owner != null)
                Owner.GetComponent<PlayerController>().ForceDropHandObject();
        }
    }

    protected virtual void OnCollisionEnter(Collision other)
    {
        if (WeaponDataStore.OnHitDisappear == (WeaponDataStore.OnHitDisappear | 1 << other.gameObject.layer))
        {
            _onWeaponDespawn();
        }
        if ((WeaponDataStore.OnNoAmmoDropDisappear == (WeaponDataStore.OnNoAmmoDropDisappear | (1 << other.gameObject.layer))) && _ammo <= 0)
        {
            _onWeaponDespawn();
        }
        if ((WeaponDataStore.OnNoAmmoDropDisappear == (WeaponDataStore.OnNoAmmoDropDisappear | (1 << other.gameObject.layer))))
        {
            if (!_hitGroundOnce)
            {
                gameObject.layer = LayerMask.NameToLayer("Pickup");
                EventManager.Instance.TriggerEvent(new ObjectHitGround(gameObject));
                _hitGroundOnce = true;
            }
        }
    }

    public virtual void OnSpawn()
    {
        CanBePickedUp = true;
        _followHand = true;
        gameObject.layer = LayerMask.NameToLayer("Pickup");
    }

    public virtual void OnDrop()
    {
        _hitGroundOnce = false;
        Owner = null;
        GetComponent<Rigidbody>().isKinematic = false;
    }

    public virtual void OnPickUp(GameObject owner)
    {
        Owner = owner;
        GetComponent<Rigidbody>().isKinematic = true;
        gameObject.layer = owner.layer;
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("DeathZone"))
        {
            _hitGroundOnce = false;
            EventManager.Instance.TriggerEvent(new WeaponHitDeathTrigger());
            _onWeaponDespawn();
            return;
        }
        if (WeaponDataStore.OnHitDisappear == (WeaponDataStore.OnHitDisappear | 1 << other.gameObject.layer)
        && Owner == null)
        {
            _onWeaponDespawn();
        }
    }
}
