﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rtHook : WeaponBase
{
    [HideInInspector]
    public GameObject Hooked = null;
    [HideInInspector]
    public bool HookCanBend = true;
    public override float HelpAimAngle { get { return WeaponDataStore.HookGunDataStore.HelpAimAngle; } }
    public override float HelpAimDistance { get { return WeaponDataStore.HookGunDataStore.HelpAimDistance; } }
    private GameObject _hook;
    private Vector3 _hookinitlocalPos;
    private Vector3 _hookinitlocalScale;
    private GameObject _hookmax;
    private Vector3 _hookmaxPos = Vector3.zero;
    private HookControl _hc;
    private LineRenderer _lr;
    private Transform _hookstartpoint;
    private Transform _hookendpoint;
    [HideInInspector]
    public bool CanCarryBack = true;

    private enum State
    {
        Empty,
        FlyingOut,
        OnTarget,
        FlyingIn,
        Broken,
    }
    private State _hookState;
    private bool _released;
    private float _brokenTimer;

    protected override void Awake()
    {
        base.Awake();
        _hook = transform.GetChild(0).gameObject;
        _hookstartpoint = _hook.transform.GetChild(0);
        _hookendpoint = transform.GetChild(2);
        _hc = _hook.GetComponent<HookControl>();
        _hookinitlocalPos = new Vector3(_hook.transform.localPosition.x, _hook.transform.localPosition.y, _hook.transform.localPosition.z);
        _hookinitlocalScale = new Vector3(_hook.transform.localScale.x, _hook.transform.localScale.y, _hook.transform.localScale.z);
        _hookmax = transform.GetChild(1).gameObject;
        _hookState = State.Empty;
        _ammo = WeaponDataStore.HookGunDataStore.MaxHookTimes;
        _lr = GetComponent<LineRenderer>();
    }

    protected override void Update()
    {
        base.Update();
        _lr.SetPosition(0, _hookendpoint.position);
        _lr.SetPosition(1, _hookstartpoint.position);

        if (_hookState == State.FlyingOut)
        {
            Vector3 nextpos = (_hookmaxPos - _hook.transform.position).normalized;
            _hook.transform.Translate(nextpos * Time.deltaTime * WeaponDataStore.HookGunDataStore.HookSpeed, Space.World);
            if (Vector3.Distance(_hook.transform.position, _hookmaxPos) <= 0.1f)
            {
                _hookState = State.FlyingIn;
            }
        }

        if (_hookState == State.FlyingIn)
        {
            if (Hooked != null && _released)
            {
                foreach (var rb in Hooked.GetComponentsInChildren<Rigidbody>())
                {
                    rb.isKinematic = false;
                }
                Vector3 force = (transform.position - _hook.transform.position).normalized;
                Vector3 vec2 = transform.right;
                Vector3 finalVec = force - vec2;
                force = (force + finalVec * 10f).normalized;

                Hooked.GetComponent<Rigidbody>().AddForce(force * WeaponDataStore.HookGunDataStore.HookAwayForce, ForceMode.Impulse);
                Hooked = null;
            }
            Vector3 nextpos = (transform.position - _hook.transform.position).normalized;
            if (HookCanBend && Hooked != null)
            {
                Vector3 vec2 = transform.right;
                Vector3 finalVec = nextpos - vec2;
                nextpos = (nextpos + finalVec * 10f).normalized;
            }
            _hook.transform.Translate(nextpos * Time.deltaTime * WeaponDataStore.HookGunDataStore.HookSpeed, Space.World);
            if (Hooked != null && CanCarryBack)
            {
                Hooked.transform.Translate(nextpos * Time.deltaTime * WeaponDataStore.HookGunDataStore.HookSpeed, Space.World);
            }
            if (Vector3.Distance(_hook.transform.position, transform.position) <= 0.6f)
            {
                if (Hooked != null)
                {
                    foreach (var rb in Hooked.GetComponentsInChildren<Rigidbody>())
                    {
                        rb.isKinematic = false;
                    }
                }
                Hooked = null;
            }
            if (Vector3.Distance(_hook.transform.position, transform.position) <= 0.4f)
            {
                _hookState = State.Empty;
                // Add once to Hook Used Times
                _onWeaponUsedOnce();
                _hc.CanHook = false;
                _hook.transform.parent = transform;
                _hook.transform.localScale = _hookinitlocalScale;
                _hook.transform.localEulerAngles = Vector3.zero;
                _hook.transform.localPosition = _hookinitlocalPos;
                _released = false;
                // Need to set hooked's rigidbody back
                if (Hooked != null)
                {
                    foreach (var rb in Hooked.GetComponentsInChildren<Rigidbody>())
                    {
                        rb.isKinematic = false;
                    }
                }

                Hooked = null;
            }
        }
        if (_hookState == State.Broken)
        {
            if (Time.time > _brokenTimer + WeaponDataStore.HookGunDataStore.HookBlockReloadTime)
            {
                _hook.SetActive(true);
                _hookState = State.Empty;
            }
        }
    }

    public override void Fire(bool buttondown)
    {
        // If button down
        if (buttondown)
        {
            if (_hookState == State.Empty)
            {
                // Then we could fire the hook
                _hookState = State.FlyingOut;
                // Tell the hook that it can now hook players
                CanCarryBack = true;
                _hc.CanHook = true;
                // Record where the hook should go to in world position
                _hookmaxPos = new Vector3(_hookmax.transform.position.x, _hookmax.transform.position.y, _hookmax.transform.position.z);
                // Also need to make hook out of parent
                _hook.transform.parent = null;
                EventManager.Instance.TriggerEvent(new HookGunFired(gameObject, Owner, Owner.GetComponent<PlayerController>().PlayerNumber));
            }
        }
        else
        {
            if (_hookState == State.FlyingIn && Hooked != null)
            {
                foreach (var rb in Hooked.GetComponentsInChildren<Rigidbody>())
                {
                    rb.isKinematic = false;
                }
                Vector3 force = (transform.position - _hook.transform.position).normalized;
                Vector3 vec2 = transform.right;
                Vector3 finalVec = force - vec2;
                force = (force + finalVec * 10f).normalized;

                Hooked.GetComponent<Rigidbody>().AddForce(force * WeaponDataStore.HookGunDataStore.HookAwayForce, ForceMode.Impulse);
                Hooked = null;
            }
            if (_hookState == State.FlyingOut)
            {
                _hookState = State.FlyingIn;
            }
            if (_hookState == State.OnTarget)
            {
                _released = true;
            }

        }
    }

    public override void OnDrop()
    {
        base.OnDrop();
        Hooked = null;
    }

    public void HookOnHit(GameObject hit)
    {
        if (hit.GetComponent<PlayerController>().CanBlock(-_hook.transform.right))
        {
            GameObject hookDup = Instantiate(_hook, _hook.transform.position, _hook.transform.rotation);
            EventManager.Instance.TriggerEvent(new HookBlocked(gameObject, Owner, Owner.GetComponent<PlayerController>().PlayerNumber, hit, hit.GetComponent<PlayerController>().PlayerNumber, hookDup));
            Destroy(hookDup, WeaponDataStore.HookGunDataStore.HookBlockReloadTime);
            _hook.transform.parent = transform;
            _hook.transform.localPosition = _hookinitlocalPos;
            _hook.SetActive(false);
            // hookDup.GetComponent<HookControl>().enabled = false;
            Destroy(hookDup.GetComponent<HookControl>());
            hookDup.GetComponent<Rigidbody>().isKinematic = false;
            hookDup.GetComponent<Rigidbody>().useGravity = true;
            // hookDup.GetComponent<Collider>().isTrigger = false;
            var coll = hookDup.AddComponent<MeshCollider>();
            coll.convex = true;
            Vector3 _dir = -hookDup.transform.right;
            _dir.y = WeaponDataStore.HookGunDataStore.HookBlockYDirection;
            hookDup.GetComponent<Rigidbody>().AddForce(WeaponDataStore.HookGunDataStore.HookBlockReflectionForce * _dir, ForceMode.Impulse);
            _brokenTimer = Time.time;
            _hookState = State.Broken;
            return;
        }
        _hookState = State.OnTarget;
        Hooked = hit;
        Hooked.GetComponent<PlayerController>().OnImpact(Owner, ImpactType.HookGun);
        foreach (var rb in Hooked.GetComponentsInChildren<Rigidbody>())
        {
            rb.isKinematic = true;
        }
        StartCoroutine(hookhelper(WeaponDataStore.HookGunDataStore.HookedTime));
        EventManager.Instance.TriggerEvent(new HookHit(gameObject, Owner, Owner.GetComponent<PlayerController>().PlayerNumber, _hook, hit, hit.GetComponent<PlayerController>().PlayerNumber));
    }

    IEnumerator hookhelper(float time)
    {
        yield return new WaitForSeconds(time);
        _hookState = State.FlyingIn;
    }

    protected override void _onWeaponDespawn()
    {
        _hookState = State.Empty;
        _hc.CanHook = false;
        _hook.transform.parent = transform;
        _hook.transform.localScale = _hookinitlocalScale;
        _hook.transform.localEulerAngles = Vector3.zero;
        _hook.transform.localPosition = _hookinitlocalPos;
        _hook.gameObject.SetActive(true);
        _ammo = WeaponDataStore.HookGunDataStore.MaxHookTimes;
        EventManager.Instance.TriggerEvent(new ObjectDespawned(gameObject));
        // Need to set hooked's rigidbody back
        if (Hooked != null)
        {
            foreach (var rb in Hooked.GetComponentsInChildren<Rigidbody>())
            {
                rb.isKinematic = false;
            }
        }

        Hooked = null;
        gameObject.SetActive(false);
    }
}
