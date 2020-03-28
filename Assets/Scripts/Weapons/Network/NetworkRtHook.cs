using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NetworkRtHook : NetworkWeaponBase
{
    [SyncVar]
    public GameObject Hooked = null;
    public override float HelpAimAngle { get { return _hookGunData.HelpAimAngle; } }
    public override float HelpAimDistance { get { return _hookGunData.HelpAimDistance; } }
    [HideInInspector]
    public HookGunData _hookGunData;
    private GameObject _hook;
    private Vector3 _hookinitlocalPos;
    private Vector3 _hookinitlocalScale;
    private Vector3 _hookinitialLocalRotation;
    private LineRenderer _lr;
    private Transform _hookstartpoint;
    private Transform _hookendpoint;
    [SyncVar]
    private bool _released;
    private FSM<NetworkRtHook> _hookGunFSM;

    protected override void Awake()
    {
        base.Awake();
        _hookGunData = WeaponDataBase as HookGunData;
        _hook = transform.GetChild(0).gameObject;
        _hookstartpoint = _hook.transform.GetChild(0);
        _hookendpoint = transform.GetChild(2);
        _hookinitlocalPos = new Vector3(_hook.transform.localPosition.x, _hook.transform.localPosition.y, _hook.transform.localPosition.z);
        _hookinitlocalScale = new Vector3(_hook.transform.localScale.x, _hook.transform.localScale.y, _hook.transform.localScale.z);
        _hookinitialLocalRotation = new Vector3(_hook.transform.localEulerAngles.x, _hook.transform.localEulerAngles.y, _hook.transform.localEulerAngles.z);
        _ammo = _hookGunData.MaxHookTimes;
        _lr = GetComponent<LineRenderer>();
        _hookGunFSM = new FSM<NetworkRtHook>(this);
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
        RpcFire(buttondown);
        // If button down
        if (buttondown)
        {
            if (_hookGunFSM.CurrentState.GetType().Equals(typeof(HookInState)))
            {
                EventManager.Instance.TriggerEvent(new HookGunFired(gameObject, Owner, Owner.GetComponent<PlayerControllerMirror>().PlayerNumber));
                _hookGunFSM.TransitionTo<HookFlyingOutState>();
                return;
            }
        }
        else
        {
            _released = true;
            if (_hookGunFSM.CurrentState.GetType().Equals(typeof(HookFlyingOutState)))
            {
                _hookGunFSM.TransitionTo<HookFlyingInState>();
                return;
            }
        }
    }

    [ClientRpc]
    private void RpcFire(bool buttondown)
    {
        // If button down
        if (buttondown)
        {
            if (_hookGunFSM.CurrentState.GetType().Equals(typeof(HookInState)))
            {
                EventManager.Instance.TriggerEvent(new HookGunFired(gameObject, Owner, Owner.GetComponent<PlayerControllerMirror>().PlayerNumber));
                _hookGunFSM.TransitionTo<HookFlyingOutState>();
                return;
            }
        }
        else
        {
            if (_hookGunFSM.CurrentState.GetType().Equals(typeof(HookFlyingOutState)))
            {
                _hookGunFSM.TransitionTo<HookFlyingInState>();
                return;
            }
        }
    }

    protected override void _onWeaponDespawn()
    {
        _hookGunFSM.TransitionTo<HookInState>();
        _ammo = _hookGunData.MaxHookTimes;
        RpcOnHookDespawn();
        base._onWeaponDespawn();
    }

    [ClientRpc]
    private void RpcOnHookDespawn()
    {
        _hookGunFSM.TransitionTo<HookInState>();
    }

    [Command]
    public void CmdHookOnHit(GameObject hit)
    {
        if (_hookGunFSM.CurrentState == null) return;
        if (!_hookGunFSM.CurrentState.GetType().Equals(typeof(HookFlyingOutState))) return;
        // Decide if he blocked
        if (hit.transform.GetComponent<IHittableNetwork>().CanBlock(-_hook.transform.right))
        {
            RpcHookBlocked(hit);
            EventManager.Instance.TriggerEvent(new HookBlocked(gameObject, Owner, Owner.GetComponent<PlayerControllerMirror>().PlayerNumber, hit, hit.GetComponent<PlayerControllerMirror>().PlayerNumber, _hook));
            _hookGunFSM.TransitionTo<HookBrokenState>();
            return;
        }
        Hooked = hit.transform.gameObject;
        RpcHookHit(hit);
        EventManager.Instance.TriggerEvent(new HookHit(gameObject, Owner, Owner.GetComponent<PlayerControllerMirror>().PlayerNumber, _hook, hit.transform.gameObject,
        hit.transform.GetComponent<PlayerControllerMirror>().PlayerNumber));
        Hooked.GetComponent<IHittableNetwork>().OnImpact(Owner, ImpactType.HookGun);
        foreach (var rb in Hooked.GetComponentsInChildren<Rigidbody>())
        {
            rb.isKinematic = true;
        }
        _hookGunFSM.TransitionTo<HookOnTargetState>();
        return;
    }

    private abstract class HookGunState : FSM<NetworkRtHook>.State
    {
        protected HookGunData _hookGunData;
        public override void Init()
        {
            base.Init();
            _hookGunData = Context.WeaponDataBase as HookGunData;
        }
        public override void OnEnter()
        {
            base.OnEnter();
            print(GetType().Name);
        }
    }

    private class HookInState : HookGunState
    {
        public override void OnEnter()
        {
            base.OnEnter();
            Context._hook.SetActive(true);
            Context._hook.transform.parent = Context.transform;
            Context._hook.transform.localScale = Context._hookinitlocalScale;
            Context._hook.transform.localEulerAngles = Vector3.zero;
            Context._hook.transform.localPosition = Context._hookinitlocalPos;
            if (Context.isServer)
                Context._released = false;
        }
    }

    private class HookFlyingOutState : HookGunState
    {
        private Vector3 _lastFramePosition;
        private float _hookOutTimer;
        public override void OnEnter()
        {
            base.OnEnter();
            _hookOutTimer = 0f;
            Context._hook.transform.parent = null;
            _lastFramePosition = Context._hook.transform.position;
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
            _checkHook();
            _lastFramePosition = Context._hook.transform.position;
        }

        private void _checkHook()
        {
            RaycastHit hit;
            if (Physics.SphereCast(Context._hook.transform.position,
                                _hookGunData.HookRadius,
                                -Context._hook.transform.right,
                                out hit,
                                _hookGunData.HookDistance,
                                _hookGunData.HookableLayer ^ (1 << Context.gameObject.layer)) ||
                Physics.Linecast(_lastFramePosition, Context._hook.transform.position, out hit, _hookGunData.HookableLayer ^ (1 << Context.gameObject.layer)))
            {
                if (Context._ownerIsLocalPlayer && hit.transform.GetComponent<IHittableNetwork>() != null)
                {
                    // Decide if he blocked
                    if (hit.transform.GetComponent<IHittableNetwork>().CanBlock(-Context._hook.transform.right))
                    {
                        Context.CmdHookBlocked(hit.transform.gameObject);
                        EventManager.Instance.TriggerEvent(new HookBlocked(Context.gameObject, Context.Owner, Context.Owner.GetComponent<PlayerControllerMirror>().PlayerNumber, hit.transform.gameObject, hit.transform.GetComponent<PlayerControllerMirror>().PlayerNumber, Context._hook));
                        TransitionTo<HookBrokenState>();
                        return;
                    }

                    Context.Hooked = hit.transform.gameObject;
                    // Context.RpcHookHit(hit.transform.gameObject);
                    Context.CmdHookHit(hit.transform.gameObject);
                    Context.Hooked.GetComponent<IHittableNetwork>().OnImpact(Context.Owner, ImpactType.HookGun);
                    foreach (var rb in Context.Hooked.GetComponentsInChildren<Rigidbody>())
                    {
                        rb.isKinematic = true;
                    }
                    TransitionTo<HookOnTargetState>();
                    return;
                }
            }
        }
    }

    [Command]
    private void CmdHookHit(GameObject hit)
    {
        RpcHookHit(hit);
        Hooked = hit.transform.gameObject;
        EventManager.Instance.TriggerEvent(new HookHit(gameObject, Owner, Owner.GetComponent<PlayerControllerMirror>().PlayerNumber, _hook, hit,
        hit.GetComponent<PlayerControllerMirror>().PlayerNumber));
        hit.GetComponent<IHittableNetwork>().OnImpact(Owner, ImpactType.HookGun);
        foreach (var rb in hit.GetComponentsInChildren<Rigidbody>())
        {
            rb.isKinematic = true;
        }
        _hookGunFSM.TransitionTo<HookOnTargetState>();
    }

    [ClientRpc]
    private void RpcHookHit(GameObject hit)
    {
        if (_ownerIsLocalPlayer) return;
        EventManager.Instance.TriggerEvent(new HookHit(gameObject, Owner, Owner.GetComponent<PlayerControllerMirror>().PlayerNumber, _hook, hit,
        hit.GetComponent<PlayerControllerMirror>().PlayerNumber));
        hit.GetComponent<IHittableNetwork>().OnImpact(Owner, ImpactType.HookGun);
        foreach (var rb in hit.GetComponentsInChildren<Rigidbody>())
        {
            rb.isKinematic = true;
        }
        _hookGunFSM.TransitionTo<HookOnTargetState>();
    }

    [Command]
    private void CmdHookBlocked(GameObject hookblocker)
    {
        EventManager.Instance.TriggerEvent(new HookBlocked(gameObject, Owner, Owner.GetComponent<PlayerControllerMirror>().PlayerNumber, hookblocker, hookblocker.GetComponent<PlayerControllerMirror>().PlayerNumber, _hook));
        _hookGunFSM.TransitionTo<HookBrokenState>();
        RpcHookBlocked(hookblocker);
    }

    [ClientRpc]
    private void RpcHookBlocked(GameObject hookblocker)
    {
        if (_ownerIsLocalPlayer) return;
        EventManager.Instance.TriggerEvent(new HookBlocked(gameObject, Owner, Owner.GetComponent<PlayerControllerMirror>().PlayerNumber, hookblocker, hookblocker.GetComponent<PlayerControllerMirror>().PlayerNumber, _hook));
        _hookGunFSM.TransitionTo<HookBrokenState>();
    }

    private class HookOnTargetState : HookGunState
    {
        private float _hookStopTimer;
        public override void OnEnter()
        {
            base.OnEnter();
            if (Context._ownerIsLocalPlayer)
                Context.Hooked.GetComponent<Smooth.SmoothSyncMirror>().positionLerpSpeed = 0f;
            _hookStopTimer = Time.time + _hookGunData.HookedTime;
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
                foreach (var rb in Context.Hooked.GetComponentsInChildren<Rigidbody>())
                {
                    rb.isKinematic = false;
                }

                Vector3 force = (Context.transform.position - Context._hook.transform.position).normalized;
                Vector3 vec2 = Context.transform.right;
                Vector3 finalVec = force - vec2;
                force = (force + finalVec * 10f).normalized;

                Context.Hooked.GetComponent<IHittableNetwork>().OnImpact(force * _hookGunData.HookAwayForce, ForceMode.Impulse, Context.Owner, ImpactType.HookGun);
                if (Context._ownerIsLocalPlayer)
                    Context.Hooked.GetComponent<Smooth.SmoothSyncMirror>().positionLerpSpeed = 0.85f;
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
                    if (Context._ownerIsLocalPlayer)
                        Context.Hooked.GetComponent<Smooth.SmoothSyncMirror>().positionLerpSpeed = 0.85f;
                    Context.Hooked = null;
                }
            }

            // If almost in, then count that as in
            if (Vector3.Distance(Context._hook.transform.position, Context.transform.position) <= 0.3f)
            {
                TransitionTo<HookInState>();
                return;
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            if (Context.isServer)
                Context._onWeaponUsedOnce();
        }
    }

    private class HookBrokenState : HookGunState
    {
        private float _brokenTimer;
        public override void OnEnter()
        {
            base.OnEnter();
            GameObject hookDup = Instantiate(Context._hook, Context._hook.transform.position, Context._hook.transform.rotation);
            Destroy(hookDup, _hookGunData.HookBlockReloadTime);
            Context._hook.SetActive(false);
            hookDup.GetComponent<Rigidbody>().isKinematic = false;
            hookDup.GetComponent<Rigidbody>().useGravity = true;
            var coll = hookDup.AddComponent<MeshCollider>();
            coll.convex = true;
            Vector3 _dir = -hookDup.transform.right;
            _dir.y = _hookGunData.HookBlockYDirection;
            hookDup.GetComponent<Rigidbody>().AddForce(_hookGunData.HookBlockReflectionForce * _dir, ForceMode.Impulse);
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

    private class HookStaticFlyingIn : HookGunState
    {
    }
}
