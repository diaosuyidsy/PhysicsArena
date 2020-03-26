using System.Collections;
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
    protected bool _followHand;
    protected float _pickUpTimer;
    // protected FSM<WeaponBase> WeaponBaseFSM;

    protected virtual void Awake()
    {
        // WeaponBaseFSM = new FSM<WeaponBase>(this);
        OnSpawn();
    }

    protected virtual void Update()
    {
        // WeaponBaseFSM.Update();
        if (Owner != null && _followHand)
        {
            GetComponent<Smooth.SmoothSyncMirror>().positionLerpSpeed = 0f;
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
        gameObject.SetActive(false);
        RpcOnWeaponDespawn();
    }

    [ClientRpc]
    protected virtual void RpcOnWeaponDespawn()
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
        // ((WeaponState)WeaponBaseFSM.CurrentState).OnCollisionEnter(other);
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
        // WeaponBaseFSM.TransitionTo<InAirState>();
        CanBePickedUp = true;
        _followHand = true;
        gameObject.layer = LayerMask.NameToLayer("Pickup");
    }

    public virtual void OnDrop()
    {
        GetComponent<Smooth.SmoothSyncMirror>().positionLerpSpeed = 0.85f;
        RpcOnDrop(Owner);
        _hitGroundOnce = false;
        CanBePickedUp = false;
        GetComponent<Rigidbody>().isKinematic = false;
        GetComponent<Rigidbody>().velocity = Vector3.zero;
        GetComponent<Rigidbody>().AddForce(Owner.transform.right * WeaponDataBase.DropForce.x +
        Owner.transform.up * WeaponDataBase.DropForce.y +
        Owner.transform.forward * WeaponDataBase.DropForce.z, ForceMode.VelocityChange);
        Owner = null;
    }

    [ClientRpc]
    public void RpcOnDrop(GameObject owner)
    {
        // GetComponent<Smooth.SmoothSyncMirror>().positionLerpSpeed = 0.85f;
        GetComponent<Rigidbody>().isKinematic = false;
        GetComponent<Rigidbody>().velocity = Vector3.zero;
        GetComponent<Rigidbody>().AddForce(owner.transform.right * WeaponDataBase.DropForce.x +
        owner.transform.up * WeaponDataBase.DropForce.y +
        owner.transform.forward * WeaponDataBase.DropForce.z, ForceMode.VelocityChange);
    }

    public virtual void OnPickUp(GameObject owner)
    {
        Owner = owner;
        gameObject.layer = owner.layer;
        GetComponent<Rigidbody>().isKinematic = true;
        RpcOnPickUp(owner);
    }

    [ClientRpc]
    public void RpcOnPickUp(GameObject owner)
    {
        GetComponent<Rigidbody>().isKinematic = true;
        gameObject.layer = owner.layer;
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        // ((WeaponState)WeaponBaseFSM.CurrentState).OnTriggerEnter(other);
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
