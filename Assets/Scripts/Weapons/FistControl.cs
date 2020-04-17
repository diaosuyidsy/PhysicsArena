using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FistControl : MonoBehaviour
{
    public FistGunData FistGunData;
    private rtFist RtFist;
    private Vector3 MaxDistance;
    private GameObject FireOwner;
    private FSM<FistControl> _fistFSM;

    private void Awake()
    {
        _fistFSM = new FSM<FistControl>(this);
    }

    public void Init(rtFist script, Vector3 _maxDistance, GameObject _fireOwner)
    {
        RtFist = script;
        MaxDistance = _maxDistance;
        FireOwner = _fireOwner;
        _fistFSM.TransitionTo<FistOutState>();
    }

    private void Update()
    {
        if (RtFist == null) return;
        _fistFSM.Update();
    }

    private abstract class FistStates : FSM<FistControl>.State
    {
        protected FistGunData _fistGunData { get { return Context.FistGunData; } }
    }

    private class FistOutState : FistStates
    {
        public override void Update()
        {
            base.Update();
            Vector3 nextPos = (Context.MaxDistance - Context.transform.position).normalized;
            Context.transform.Translate(nextPos * Time.deltaTime * _fistGunData.FistSpeed, Space.World);
            RaycastHit hit;
            if (Physics.SphereCast(Context.transform.position, _fistGunData.FistHitScanRadius, -Context.transform.right, out hit, _fistGunData.FistHitScanDist, _fistGunData.AllThingFistCanCollideLayer ^ (1 << Context.FireOwner.layer)))
            {
                if (hit.collider.GetComponent<WeaponBase>() != null) return;
                IHittable IHittable = hit.collider.GetComponentInParent<IHittable>();
                PlayerController pc = hit.collider.GetComponentInParent<PlayerController>();
                if (IHittable != null && !IHittable.CanBlock(-Context.transform.right))
                {
                    IHittable.SetVelocity(Vector3.zero);
                    IHittable.OnImpact(-Context.transform.right * _fistGunData.FistHitForce, ForceMode.Impulse, Context.FireOwner, ImpactType.FistGun);
                    EventManager.Instance.TriggerEvent(new FistGunHit(Context.RtFist.gameObject, Context.gameObject, Context.FireOwner, ((MonoBehaviour)IHittable).gameObject, Context.FireOwner.GetComponent<PlayerController>().PlayerNumber, pc == null ? 6 : pc.PlayerNumber));
                    TransitionTo<FistUpUselessState>();
                    return;
                }
                else if (pc != null)
                {
                    Context.MaxDistance = pc.transform.position + pc.transform.forward * _fistGunData.MaxFlyDistance;
                    Context.FireOwner = pc.gameObject;
                    Context.transform.rotation = Quaternion.LookRotation(pc.transform.right, Context.transform.up);
                    EventManager.Instance.TriggerEvent(new FistGunBlocked(Context.RtFist.gameObject, Context.FireOwner, Context.FireOwner.GetComponent<PlayerController>().PlayerNumber, Context.gameObject, pc.gameObject, pc.PlayerNumber));
                    return;
                }
                else if (pc == null)
                {
                    EventManager.Instance.TriggerEvent(new FistGunHit(Context.RtFist.gameObject, Context.gameObject, Context.FireOwner, hit.collider.gameObject, Context.FireOwner.GetComponent<PlayerController>().PlayerNumber, -1));
                    TransitionTo<FistUpUselessState>();
                    return;
                }
                return;
            }
            if (Vector3.Distance(Context.transform.position, Context.MaxDistance) <= 0.2f)
            {
                TransitionTo<FistUselessState>();
                return;
            }
        }
    }

    private class FistUselessState : FistStates
    {
        private float _stateTimer;
        public override void OnEnter()
        {
            base.OnEnter();
            Context.gameObject.GetComponent<Rigidbody>().isKinematic = false;
            Context.gameObject.GetComponent<Rigidbody>().velocity = -Context.transform.right * _fistGunData.FistSpeed;
            _stateTimer = Time.timeSinceLevelLoad;
        }

        public override void Update()
        {
            base.Update();
            if (_stateTimer + _fistGunData.FistUselessDuration < Time.timeSinceLevelLoad)
            {
                Destroy(Context.gameObject);
            }
        }
    }

    private class FistUpUselessState : FistStates
    {
        private float _stateTimer;
        public override void OnEnter()
        {
            base.OnEnter();
            Context.gameObject.GetComponent<Rigidbody>().isKinematic = false;
            Vector3 rebound = Context.transform.right;
            rebound.y = _fistGunData.FistReboundY;
            Context.gameObject.GetComponent<Rigidbody>().AddForce(_fistGunData.FistReboundForce * rebound, ForceMode.Impulse);
            _stateTimer = Time.timeSinceLevelLoad;
        }

        public override void Update()
        {
            base.Update();
            if (_stateTimer + _fistGunData.FistUselessDuration < Time.timeSinceLevelLoad)
            {
                Destroy(Context.gameObject);
            }
        }
    }
}
