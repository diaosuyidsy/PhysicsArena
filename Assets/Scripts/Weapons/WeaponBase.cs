﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GunPositionControl))]
public abstract class WeaponBase : MonoBehaviour
{
    public WeaponData WeaponDataStore;

    protected int _ammo { get; set; }
    protected GunPositionControl _gpc;

    protected virtual void Awake()
    {
        _gpc = GetComponent<GunPositionControl>();
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
            _gpc.CanBePickedUp = false;
            _gpc.Owner.GetComponent<PlayerController>().ForceDropHandObject();
        }
    }

    protected void OnCollisionEnter(Collision other)
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

        }
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("DeathZone"))
        {
            _onWeaponDespawn();
        }
    }
}
