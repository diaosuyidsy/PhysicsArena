using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rtHook : WeaponBase
{
    [HideInInspector]
    public GameObject Hooked = null;
    public override float HelpAimAngle { get { return _hookGunData.HelpAimAngle; } }
    public override float HelpAimDistance { get { return _hookGunData.HelpAimDistance; } }
    [HideInInspector]
    public HookGunData _hookGunData;
    private GameObject _hook;
    private Vector3 _hookinitlocalPos;
    private Vector3 _hookinitlocalScale;
    private Vector3 _hookinitialLocalRotation;
    private HookControl _hc;
    private LineRenderer _lr;
    private Transform _hookstartpoint;
    private Transform _hookendpoint;
    public bool CanHook
    {
        get
        {
            return _hookGunFSM != null &&
                _hookGunFSM.CurrentState != null &&
                Owner != null &&
                _hookGunFSM.CurrentState.GetType().Equals(typeof(HookFlyingOutState));
        }
    }

    private bool _released;
    private float _releasedTimer;
    private float _onTargetTimer;
    private float _hookOutTimer;
    private Vector3 _onTargetTransformForwardVector;
    private FSM<rtHook> _hookGunFSM;
    private GameObject _hookDup;

    protected override void Awake()
    {
        base.Awake();
        _hookGunData = WeaponDataBase as HookGunData;
        _hook = transform.GetChild(0).gameObject;
        _hookstartpoint = _hook.transform.GetChild(0);
        _hookendpoint = transform.GetChild(2);
        _hc = _hook.GetComponent<HookControl>();
        _hookinitlocalPos = new Vector3(_hook.transform.localPosition.x, _hook.transform.localPosition.y, _hook.transform.localPosition.z);
        _hookinitlocalScale = new Vector3(_hook.transform.localScale.x, _hook.transform.localScale.y, _hook.transform.localScale.z);
        _hookinitialLocalRotation = new Vector3(_hook.transform.localEulerAngles.x, _hook.transform.localEulerAngles.y, _hook.transform.localEulerAngles.z);
        _ammo = _hookGunData.MaxHookTimes;
        _lr = GetComponent<LineRenderer>();
        _hookGunFSM = new FSM<rtHook>(this);
        _hookGunFSM.TransitionTo<HookInState>();
    }

    protected override void Update()
    {
        base.Update();
        _hookGunFSM.Update();
        _lr.SetPosition(0, _hookendpoint.position);
        _lr.SetPosition(1, _hookstartpoint.position);
    }

    public override void Fire(bool buttondown)
    {
        // If button down
        if (buttondown)
        {
            if (_hookGunFSM.CurrentState.GetType().Equals(typeof(HookInState)))
            {
                _released = false;
                EventManager.Instance.TriggerEvent(new HookGunFired(gameObject, Owner, Owner.GetComponent<PlayerController>().PlayerNumber));
                _hookGunFSM.TransitionTo<HookFlyingOutState>();
                return;
            }
        }
        else
        {
            _released = true;
            _releasedTimer = Time.time;
        }
    }

    public override void OnDrop(bool customForce, Vector3 force)
    {
        base.OnDrop(customForce, force);
        if (_hookDup != null)
        {
            Destroy(_hookDup);
            _hookDup = null;
        }
        if (Hooked != null)
        {
            foreach (var rb in Hooked.GetComponentsInChildren<Rigidbody>())
            {
                rb.isKinematic = false;
            }
        }
        Hooked = null;
    }

    public void HookOnHit(GameObject hit)
    {
        if (Hooked != null) return;
        if (hit.transform.GetComponent<IHittable>().CanBlock(-_hook.transform.right))
        {
            EventManager.Instance.TriggerEvent(new HookBlocked(gameObject, Owner, Owner.GetComponent<PlayerController>().PlayerNumber, hit, hit.GetComponent<PlayerController>().PlayerNumber, _hook));
            _hookGunFSM.TransitionTo<HookBrokenState>();
            return;
        }

        Hooked = hit;
        hit.GetComponent<IHittable>().OnImpact(Owner, ImpactType.HookGun);
        EventManager.Instance.TriggerEvent(new HookHit(gameObject, Owner, Owner.GetComponent<PlayerController>().PlayerNumber, _hook, hit,
        hit.GetComponent<PlayerController>().PlayerNumber));
        foreach (var rb in hit.GetComponentsInChildren<Rigidbody>())
        {
            rb.isKinematic = true;
        }
        _hookGunFSM.TransitionTo<HookOnTargetState>();
    }

    protected override void _onWeaponDespawn()
    {
        _hookGunFSM.TransitionTo<HookInState>();
        if (_hookDup != null)
        {
            Destroy(_hookDup);
            _hookDup = null;
        }
        _ammo = _hookGunData.MaxHookTimes;
        base._onWeaponDespawn();
    }

    private abstract class HookGunState : FSM<rtHook>.State
    {
        protected HookGunData _hookGunData;
        public override void Init()
        {
            base.Init();
            _hookGunData = Context.WeaponDataBase as HookGunData;
        }
    }

    private class HookInState : HookGunState
    {
        public override void OnEnter()
        {
            base.OnEnter();
            if (Context._hookDup != null)
            {
                Destroy(Context._hookDup);
                Context._hookDup = null;
            }
            Context._hook.SetActive(true);
            Context._hook.transform.parent = Context.transform;
            Context._hook.transform.localScale = Context._hookinitlocalScale;
            Context._hook.transform.localEulerAngles = Context._hookinitialLocalRotation;
            Context._hook.transform.localPosition = Context._hookinitlocalPos;
            Context._released = false;
        }
    }

    private class HookFlyingOutState : HookGunState
    {
        private float _hookOutTimer;
        public override void OnEnter()
        {
            base.OnEnter();
            _hookOutTimer = 0f;
            Context._hook.transform.parent = null;
            Context._hookOutTimer = Time.time;
        }

        public override void Update()
        {
            base.Update();
            _hookOutTimer += Time.deltaTime;
            Context._hook.transform.Translate(-Context._hook.transform.right * Time.deltaTime * _hookGunData.HookSpeed, Space.World);
            if (_hookOutTimer >= _hookGunData.HookOutDuration)
            {
                TransitionTo<HookFlyingInState>();
                return;
            }
            if (_hookOutTimer >= _hookGunData.HookMinOutDuration && Context._released)
            {
                TransitionTo<HookFlyingInState>();
                return;
            }
        }
    }

    private class HookOnTargetState : HookGunState
    {
        private float _hookStopTimer;
        public override void OnEnter()
        {
            base.OnEnter();
            _hookStopTimer = Time.time + _hookGunData.HookedTime;
            Context._onTargetTimer = Time.time;
            Context._onTargetTransformForwardVector = (Context.Hooked.transform.position - Context.transform.position).normalized;
        }

        public override void Update()
        {
            base.Update();
            if (_hookStopTimer < Time.time)
            {
                TransitionTo<HookFlyingInState>();
                return;
            }
        }
    }

    private class HookFlyingInState : HookGunState
    {
        public override void Update()
        {
            base.Update();
            // Whatever we do, retract the hook
            Vector3 nextpos = (Context.transform.position - Context._hook.transform.position).normalized;
            if (Context.Hooked != null)
            {
                Vector3 vec = Context.transform.right;
                Vector3 finalVec1 = nextpos - vec;
                nextpos = (nextpos + finalVec1 * 10f).normalized;
            }
            Context._hook.transform.Translate(nextpos * Time.deltaTime * _hookGunData.HookSpeed, Space.World);

            // if hooked and not released
            // Then make hooked follow hook position
            if (Context.Hooked != null && !Context._released)
            {
                Context.Hooked.transform.Translate(nextpos * Time.deltaTime * _hookGunData.HookSpeed, Space.World);
            }

            // if hooked and released the trigger,
            // Slingshot them out
            if (Context.Hooked != null && Context._released)
            {
                Vector3 force = (Context.transform.position - Context._hook.transform.position).normalized;
                Vector3 vec2 = Context.transform.right;
                Vector3 finalVec = force - vec2;
                finalVec.y = 0f;
                if (finalVec.magnitude <= 0.3f)
                    finalVec = force;
                force = finalVec.normalized;
                foreach (var rb in Context.Hooked.GetComponentsInChildren<Rigidbody>())
                {
                    rb.isKinematic = false;
                }
                float HookForceMagnitude = _hookGunData.HookAwayForce;
                float releaseOnTargetTimeDiff = Context._releasedTimer - Context._onTargetTimer;
                if (releaseOnTargetTimeDiff <= 1f)
                {
                    HookForceMagnitude *= _hookGunData.HookAwayForceCurve.Evaluate(releaseOnTargetTimeDiff);
                }
                float GunTotalRotation = Vector3.Angle(Context._onTargetTransformForwardVector, -Context.transform.right);
                HookForceMagnitude *= _hookGunData.HookAwayForceRotationCurve.Evaluate(GunTotalRotation / 180f);
                Context.Hooked.GetComponent<IHittable>().OnImpact(force * HookForceMagnitude, ForceMode.Impulse, Context.Owner, ImpactType.HookGun);
                EventManager.Instance.TriggerEvent(new HookSlingShot(Context.gameObject, Context.Owner, Context.Owner.GetComponent<PlayerController>().PlayerNumber, Context.Hooked, force));
                Context.Hooked = null;
            }

            // If it's almost in, then get rid of the hooked player first
            // to make sure no hardcore collision happens
            if (Vector3.Distance(Context._hook.transform.position, Context.transform.position) <= 0.6f)
            {
                if (Context.Hooked != null)
                {
                    foreach (var rb in Context.Hooked.GetComponentsInChildren<Rigidbody>())
                    {
                        rb.isKinematic = false;
                    }
                    Context.Hooked = null;
                }
            }

            // If almost in, then count that as in
            if (Vector3.Distance(Context._hook.transform.position, Context.transform.position) <= 0.3f)
            {
                TransitionTo<HookRecoveryState>();
                return;
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            Context._onWeaponUsedOnce();
        }
    }

    private class HookRecoveryState : HookGunState
    {
        public override void OnEnter()
        {
            base.OnEnter();
            Context._hook.SetActive(true);
            Context._hook.transform.parent = Context.transform;
            Context._hook.transform.localScale = Context._hookinitlocalScale;
            Context._hook.transform.localEulerAngles = Context._hookinitialLocalRotation;
            Context._hook.transform.localPosition = Context._hookinitlocalPos;
        }

        public override void Update()
        {
            base.Update();
            if (Context._hookOutTimer + _hookGunData.HookGunUseCD < Time.time)
            {
                TransitionTo<HookInState>();
                return;
            }
        }
    }

    private class HookBrokenState : HookGunState
    {
        private float _brokenTimer;
        public override void OnEnter()
        {
            base.OnEnter();
            Context._hookDup = Instantiate(Context._hook, Context._hook.transform.position, Context._hook.transform.rotation);
            Destroy(Context._hookDup, _hookGunData.HookBlockReloadTime);
            Context._hook.transform.parent = Context.transform;
            Context._hook.transform.localScale = Context._hookinitlocalScale;
            Context._hook.transform.localEulerAngles = Context._hookinitialLocalRotation;
            Context._hook.transform.localPosition = Context._hookinitlocalPos;
            Context._hook.SetActive(false);
            Context._hookDup.GetComponent<Rigidbody>().isKinematic = false;
            Context._hookDup.GetComponent<Rigidbody>().useGravity = true;
            var coll = Context._hookDup.AddComponent<MeshCollider>();
            coll.convex = true;
            Vector3 _dir = -Context._hookDup.transform.right;
            _dir.y = _hookGunData.HookBlockYDirection;
            Context._hookDup.GetComponent<Rigidbody>().AddForce(_hookGunData.HookBlockReflectionForce * _dir, ForceMode.Impulse);
            _brokenTimer = Time.time + _hookGunData.HookBlockReloadTime;
        }

        public override void Update()
        {
            base.Update();
            if (_brokenTimer < Time.time)
            {
                TransitionTo<HookInState>();
                return;
            }
        }
    }
}
