﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public abstract class NetworkWeaponBase : NetworkBehaviour
{
    public WeaponDataBase WeaponDataBase;
    public virtual float HelpAimAngle { get; }
    public virtual float HelpAimDistance { get; }
    [SyncVar]
    public GameObject Owner;
    [SyncVar]
    protected int _ammo;
    [SyncVar]
    protected bool _hitGroundOnce;
    [SyncVar]
    public bool CanBePickedUp;
    public bool TransitAuthorityOnPickUp = true;
    protected bool _followHand;
    protected float _pickUpTimer;
    protected bool _ownerIsLocalPlayer => Owner == null ? false : Owner.GetComponent<NetworkIdentity>().isLocalPlayer;

    protected virtual void Awake()
    {
        OnSpawn();
    }

    protected virtual void Update()
    {
        if (Owner != null && _followHand)
        {
            Vector3 targetposition = (Owner.GetComponent<PlayerControllerMirror>().LeftHand.transform.position
            + Owner.GetComponent<PlayerControllerMirror>().RightHand.transform.position) / 2f;
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
    protected virtual void _onWeaponDespawn()
    {
        GetComponent<NetworkIdentity>().RemoveClientAuthority();
        EventManager.Instance.TriggerEvent(new ObjectDespawned(gameObject));
        gameObject.SetActive(false);
        RpcOnWeaponDespawn();
    }

    [ClientRpc]
    private void RpcOnWeaponDespawn()
    {
        EventManager.Instance.TriggerEvent(new ObjectDespawned(gameObject));
        gameObject.SetActive(false);
    }

    protected void _onWeaponUsedOnce()
    {
        _ammo--;
        if (_ammo <= 0)
        {
            CanBePickedUp = false;
            EventManager.Instance.TriggerEvent(new WeaponUsedUp());
            RpcAmmoUsedUp(Owner);
        }
    }

    [ClientRpc]
    private void RpcAmmoUsedUp(GameObject owner)
    {
        EventManager.Instance.TriggerEvent(new WeaponUsedUp());
        if (owner != null)
            owner.GetComponent<PlayerControllerMirror>().ForceDropHandObject();
    }

    protected virtual void OnCollisionEnter(Collision other)
    {
        if (!isServer) return;
        if (WeaponDataBase.OnHitDisappear == (WeaponDataBase.OnHitDisappear | 1 << other.gameObject.layer))
        {
            _onWeaponDespawn();
        }
        if ((WeaponDataBase.OnNoAmmoDropDisappear == (WeaponDataBase.OnNoAmmoDropDisappear | (1 << other.gameObject.layer))) && _ammo <= 0)
        {
            _onWeaponDespawn();
        }
        if ((WeaponDataBase.OnNoAmmoDropDisappear == (WeaponDataBase.OnNoAmmoDropDisappear | (1 << other.gameObject.layer))))
        {
            if (!_hitGroundOnce)
            {
                GetComponent<NetworkIdentity>().RemoveClientAuthority();
                gameObject.layer = LayerMask.NameToLayer("Pickup");
                RpcHitGround();
                CanBePickedUp = true;
                _hitGroundOnce = true;
            }
        }
    }

    [ClientRpc]
    public void RpcHitGround()
    {
        gameObject.layer = LayerMask.NameToLayer("Pickup");
        EventManager.Instance.TriggerEvent(new ObjectHitGround(gameObject));
    }

    public virtual void OnSpawn()
    {
        CanBePickedUp = true;
        _followHand = true;
        gameObject.layer = LayerMask.NameToLayer("Pickup");
    }

    public virtual void OnDrop(bool customForce, Vector3 force)
    {
        GetComponent<Smooth.SmoothSyncMirror>().positionLerpSpeed = 0.85f;
        GetComponent<Smooth.SmoothSyncMirror>().rotationLerpSpeed = 0.85f;
        RpcOnDrop(Owner, customForce, force);
        _hitGroundOnce = false;
        CanBePickedUp = false;
        GetComponent<Rigidbody>().isKinematic = false;
        GetComponent<Rigidbody>().velocity = Vector3.zero;
        if (!customForce)
            GetComponent<Rigidbody>().AddForce(Owner.transform.right * WeaponDataBase.DropForce.x +
            Owner.transform.up * WeaponDataBase.DropForce.y +
            Owner.transform.forward * WeaponDataBase.DropForce.z, ForceMode.VelocityChange);
        else
            GetComponent<Rigidbody>().AddForce(Owner.transform.right * force.x +
                Owner.transform.up * force.y +
                Owner.transform.forward * force.z, ForceMode.VelocityChange);
        Owner = null;
    }

    [ClientRpc]
    public void RpcOnDrop(GameObject owner, bool customForce, Vector3 force)
    {
        GetComponent<Smooth.SmoothSyncMirror>().positionLerpSpeed = 0.85f;
        GetComponent<Smooth.SmoothSyncMirror>().rotationLerpSpeed = 0.85f;
        GetComponent<Rigidbody>().isKinematic = false;
        GetComponent<Rigidbody>().velocity = Vector3.zero;
        if (!customForce)
            GetComponent<Rigidbody>().AddForce(owner.transform.right * WeaponDataBase.DropForce.x +
            owner.transform.up * WeaponDataBase.DropForce.y +
            owner.transform.forward * WeaponDataBase.DropForce.z, ForceMode.VelocityChange);
        else
            GetComponent<Rigidbody>().AddForce(owner.transform.right * force.x +
            owner.transform.up * force.y +
            owner.transform.forward * force.z, ForceMode.VelocityChange);
    }

    public virtual void OnPickUp(GameObject owner)
    {
        CanBePickedUp = false;
        Owner = owner;
        gameObject.layer = owner.layer;
        GetComponent<Rigidbody>().isKinematic = true;
        GetComponent<Smooth.SmoothSyncMirror>().positionLerpSpeed = 0f;
        GetComponent<Smooth.SmoothSyncMirror>().rotationLerpSpeed = 0f;
        if (TransitAuthorityOnPickUp)
            GetComponent<NetworkIdentity>().AssignClientAuthority(Owner.GetComponent<NetworkIdentity>().connectionToClient);
        RpcOnPickUp(owner);
    }

    [ClientRpc]
    public void RpcOnPickUp(GameObject owner)
    {
        GetComponent<Smooth.SmoothSyncMirror>().positionLerpSpeed = 0f;
        GetComponent<Smooth.SmoothSyncMirror>().rotationLerpSpeed = 0f;
        GetComponent<Rigidbody>().isKinematic = true;
        gameObject.layer = owner.layer;
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (!isServer) return;
        if (other.CompareTag("DeathZone"))
        {
            _hitGroundOnce = false;
            EventManager.Instance.TriggerEvent(new WeaponHitDeathTrigger());
            _onWeaponDespawn();
            return;
        }
        if (WeaponDataBase.OnHitDisappear == (WeaponDataBase.OnHitDisappear | 1 << other.gameObject.layer)
        && Owner == null)
        {
            _onWeaponDespawn();
        }
    }

    protected abstract class WeaponState : FSM<WeaponBase>.State
    {
        protected WeaponDataBase WeaponBaseData;

        public override void Init()
        {
            base.Init();
            WeaponBaseData = Context.WeaponDataBase;
        }

        public virtual void OnCollisionEnter(Collision other)
        {

        }

        public virtual void OnTriggerEnter(Collider other)
        {

        }
    }

    protected class InAirState : WeaponState
    {
        public override void OnCollisionEnter(Collision other)
        {
            base.OnCollisionEnter(other);
            // if ((WeaponBaseData.OnNoAmmoDropDisappear == (WeaponBaseData.OnNoAmmoDropDisappear | (1 << other.gameObject.layer))))
            // {
            //     Context.gameObject.layer = LayerMask.NameToLayer("Pickup");
            //     EventManager.Instance.TriggerEvent(new ObjectHitGround(Context.gameObject));
            // }
        }
    }

    protected class OnGroundState : WeaponState
    {

    }

    protected class PickedUpState : WeaponState
    {

    }

    protected class DeadState : WeaponState
    {

    }
}
