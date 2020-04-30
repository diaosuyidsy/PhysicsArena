using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    public WeaponDataBase WeaponDataBase;
    public virtual float HelpAimAngle { get; }
    public virtual float HelpAimDistance { get; }
    public bool OnDropDisappear = true;
    public bool ShouldFloat = true;
    public GameObject Owner { get; protected set; }
    protected int _ammo { get; set; }
    protected bool _hitGroundOnce;
    public bool CanBePickedUp;
    protected bool _followHand;
    protected FSM<WeaponBase> WeaponFSM;
    protected Rigidbody Rigidbody;

    protected virtual void Awake()
    {
        WeaponFSM = new FSM<WeaponBase>(this);
        Rigidbody = GetComponent<Rigidbody>();
        OnSpawn();
    }

    protected virtual void Update()
    {
        WeaponFSM.Update();
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
            if (Owner != null)
                Owner.GetComponent<PlayerController>().ForceDropHandObject();
        }
    }

    protected virtual void OnCollisionEnter(Collision other)
    {
        if (!gameObject.activeSelf) return;
        if (WeaponFSM.CurrentState == null) return;
        (WeaponFSM.CurrentState as WeaponState).OnCollisionEnter(other);
    }

    public virtual void OnSpawn()
    {
        WeaponFSM.TransitionTo<SpawnInAirState>();
    }

    public virtual void OnDrop(bool customForce, Vector3 force)
    {
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
        WeaponFSM.TransitionTo<DroppedState>();
    }

    public virtual void OnPickUp(GameObject owner)
    {
        CanBePickedUp = false;
        Owner = owner;
        GetComponent<Rigidbody>().isKinematic = true;
        gameObject.layer = LayerMask.NameToLayer("PickedUpWeapon");
        WeaponFSM.TransitionTo<HoldingState>();
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (WeaponFSM.CurrentState == null) return;
        (WeaponFSM.CurrentState as WeaponState).OnTriggerEnter(other);
    }

    protected abstract class WeaponState : FSM<WeaponBase>.State
    {
        protected WeaponDataBase WeaponDataBase { get { return Context.WeaponDataBase; } }
        public override void OnEnter()
        {
            base.OnEnter();
        }
        public virtual void OnCollisionEnter(Collision other)
        {
        }
        public virtual void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("DeathZone"))
            {
                EventManager.Instance.TriggerEvent(new WeaponHitDeathTrigger());
                Context._onWeaponDespawn();
                return;
            }
            if (WeaponDataBase.OnHitDisappear == (WeaponDataBase.OnHitDisappear | 1 << other.gameObject.layer)
            && Context.Owner == null)
            {
                Context._onWeaponDespawn();
                return;
            }
        }
    }

    protected class SpawnInAirState : WeaponState
    {
        public override void OnEnter()
        {
            base.OnEnter();
            Context.CanBePickedUp = true;
            Context._followHand = true;
            Context.gameObject.layer = LayerMask.NameToLayer("Pickup");
            Context.Rigidbody.useGravity = true;
            Context.Rigidbody.isKinematic = false;
        }

        public override void Update()
        {
            base.Update();
            RaycastHit hit;
            if (Physics.SphereCast(Context.transform.position, 0.2f, Vector3.down, out hit, Context.WeaponDataBase.YFloatDistance, WeaponDataBase.OnNoAmmoDropDisappear))
            {
                TransitionTo<PickupableState>();
                return;
            }
        }

        public override void OnCollisionEnter(Collision other)
        {
            base.OnCollisionEnter(other);
            if (WeaponDataBase.OnHitDisappear == (WeaponDataBase.OnHitDisappear | 1 << other.gameObject.layer))
            {
                Context._onWeaponDespawn();
                return;
            }

            // if ((WeaponDataBase.OnNoAmmoDropDisappear == (WeaponDataBase.OnNoAmmoDropDisappear | (1 << other.gameObject.layer))))
            // {
            //     TransitionTo<PickupableState>();
            //     return;
            // }
        }
    }

    protected class PickupableState : WeaponState
    {
        private Vector3 floatingPoint;
        public override void OnEnter()
        {
            base.OnEnter();
            if (!Context.ShouldFloat) return;
            RaycastHit hit;
            if (Physics.Raycast(Context.transform.position, Vector3.down, out hit, Mathf.Infinity, WeaponDataBase.OnNoAmmoDropDisappear))
            {
                Context.Rigidbody.isKinematic = true;
                foreach (var colls in Context.GetComponents<Collider>())
                {
                    colls.isTrigger = true;
                }
                Vector3 hitPoint = hit.point;
                floatingPoint = hitPoint + Vector3.up * WeaponDataBase.YFloatDistance;
                Context.transform.position = floatingPoint;
                Context.transform.eulerAngles = WeaponDataBase.RotatingRotation;
            }
        }

        public override void Update()
        {
            base.Update();
            if (!Context.ShouldFloat) return;
            Context.transform.Rotate(Vector3.up, WeaponDataBase.RotationSpeed * Time.deltaTime, Space.World);
        }
    }

    protected class HoldingState : WeaponState
    {
        public override void Update()
        {
            base.Update();
            if (Context.Owner != null && Context._followHand)
            {
                Vector3 targetposition = (Context.Owner.GetComponent<PlayerController>().LeftHand.transform.position
                + Context.Owner.GetComponent<PlayerController>().RightHand.transform.position) / 2f;
                Context.transform.position = targetposition;
                Context.transform.position += Context.transform.right * WeaponDataBase.XOffset;
                Context.transform.position += Context.transform.up * WeaponDataBase.YOffset;
                Context.transform.position += Context.transform.forward * WeaponDataBase.ZOffset;
                Context.transform.eulerAngles = new Vector3(WeaponDataBase.XRotation, Context.Owner.transform.eulerAngles.y + WeaponDataBase.YRotation, WeaponDataBase.ZRotation);
            }
        }
    }

    protected class DroppedState : WeaponState
    {
        public override void OnEnter()
        {
            base.OnEnter();
            if (!Context.ShouldFloat) return;
            foreach (var colls in Context.GetComponents<Collider>())
            {
                colls.isTrigger = false;
            }
        }
        public override void OnCollisionEnter(Collision other)
        {
            base.OnCollisionEnter(other);
            if ((WeaponDataBase.OnNoAmmoDropDisappear == (WeaponDataBase.OnNoAmmoDropDisappear | (1 << other.gameObject.layer))))
            {
                if (Context.OnDropDisappear)
                {
                    Context._onWeaponDespawn();
                    return;
                }
                else
                {
                    TransitionTo<SpawnInAirState>();
                    return;
                }
            }
        }
    }
}
