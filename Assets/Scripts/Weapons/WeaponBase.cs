using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    public WeaponDataBase WeaponDataBase;
    public virtual float HelpAimAngle { get; }
    public virtual float HelpAimDistance { get; }

    public GameObject Owner { get; protected set; }
    protected int _ammo { get; set; }
    protected bool _hitGroundOnce;
    public bool CanBePickedUp;
    protected bool _followHand;
    protected float _pickUpTimer;
    protected FSM<WeaponBase> WeaponBaseFSM;

    protected virtual void Awake()
    {
        // WeaponBaseFSM = new FSM<WeaponBase>(this);
        // WeaponBaseFSM.TransitionTo<InAirState>();
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
        // ((WeaponState)WeaponBaseFSM.CurrentState).OnCollisionEnter(other);
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
                EventManager.Instance.TriggerEvent(new ObjectHitGround(gameObject));
                CanBePickedUp = true;
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
        CanBePickedUp = false;
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
        // ((WeaponState)WeaponBaseFSM.CurrentState).OnTriggerEnter(other);
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
