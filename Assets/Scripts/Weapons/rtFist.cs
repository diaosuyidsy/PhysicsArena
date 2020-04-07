using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class rtFist : WeaponBase
{
    public override float HelpAimAngle { get { return _fistGunData.HelpAimAngle; } }
    public override float HelpAimDistance { get { return _fistGunData.HelpAimDistance; } }
    private FistGunData _fistGunData;
    private Transform _fist;
    private GameObject _fistDup;
    private Fist _fistScript;
    private FistGunState _fistGunState = FistGunState.Idle;
    private GameObject _fireOwner;
    private enum FistGunState
    {
        Idle,
        Out,
        Recharging,
    }
    private Vector3 _maxDistance;
    private float _chargeTimer;

    protected override void Awake()
    {
        base.Awake();
        _fistGunData = WeaponDataBase as FistGunData;
        _fist = transform.Find("Fist");
        Debug.Assert(_fist != null);
        _fistScript = _fist.GetComponent<Fist>();
        _ammo = _fistGunData.MaxAmmo;
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        if (_fistGunState == FistGunState.Out)
        {
            Vector3 nextPos = (_maxDistance - _fistDup.transform.position).normalized;
            _fistDup.transform.Translate(nextPos * Time.deltaTime * _fistGunData.FistSpeed, Space.World);
            RaycastHit hit;
            if (Physics.SphereCast(_fistDup.transform.position, _fistGunData.FistHitScanRadius, -_fistDup.transform.right, out hit, _fistGunData.FistHitScanDist, _fistGunData.AllThingFistCanCollideLayer ^ (1 << _fireOwner.layer)))
            {
                if (hit.collider.GetComponent<WeaponBase>() != null) return;
                IHittable IHittable = hit.collider.GetComponentInParent<IHittable>();
                PlayerController pc = hit.collider.GetComponentInParent<PlayerController>();
                if (IHittable != null && !IHittable.CanBlock(-_fistDup.transform.right))
                {
                    IHittable.OnImpact(-_fistDup.transform.right * _fistGunData.FistHitForce, ForceMode.Impulse, _fireOwner, ImpactType.FistGun);
                    EventManager.Instance.TriggerEvent(new FistGunHit(gameObject, _fistDup, Owner, ((MonoBehaviour)IHittable).gameObject, Owner.GetComponent<PlayerController>().PlayerNumber, pc == null ? 6 : pc.PlayerNumber));
                }
                else if (pc != null)
                {
                    _maxDistance = pc.transform.position + pc.transform.forward * _fistGunData.MaxFlyDistance;
                    _fireOwner = pc.gameObject;
                    _fistDup.transform.rotation = Quaternion.LookRotation(pc.transform.right, _fistDup.transform.up);
                    EventManager.Instance.TriggerEvent(new FistGunBlocked(gameObject, Owner, Owner.GetComponent<PlayerController>().PlayerNumber, _fistDup, pc.gameObject, pc.PlayerNumber));
                    return;
                }
                else if (pc == null)
                {
                    EventManager.Instance.TriggerEvent(new FistGunHit(gameObject, _fistDup, Owner, hit.collider.gameObject, Owner.GetComponent<PlayerController>().PlayerNumber, -1));
                }
                _switchToRecharge();
                return;
            }
            if (Vector3.Distance(_fistDup.transform.position, _maxDistance) <= 0.2f)
            {
                _switchToRecharge(true);
                return;
            }
        }
        if (_fistGunState == FistGunState.Recharging)
        {
            if (Time.time < _chargeTimer + _fistGunData.ReloadTime)
            {
                // Recharding
            }
            else
            {
                // Charged
                EventManager.Instance.TriggerEvent(new FistGunCharged(gameObject, Owner, _fistDup.transform.position));
                Destroy(_fistDup);
                _fistDup = null;
                _fist.gameObject.SetActive(true);
                _fistGunState = FistGunState.Idle;
                return;
            }
        }
    }

    public override void Fire(bool buttondown)
    {
        if (buttondown)
        {
            if (_fistGunState == FistGunState.Idle)
            {
                _fireOwner = Owner;
                _maxDistance = transform.position + -transform.right * _fistGunData.MaxFlyDistance;
                _fistDup = Instantiate(_fist.gameObject, transform);
                _fistDup.transform.position = _fist.position;
                _fistDup.transform.parent = null;
                _fistDup.GetComponent<Collider>().isTrigger = false;
                _fist.gameObject.SetActive(false);
                /// Add Backfire force to player as well
                if (!Owner.GetComponent<PlayerController>().IsIdle)
                    Owner.GetComponent<PlayerController>().OnImpact(-Owner.transform.forward * _fistGunData.BackfireHitForce, ForceMode.VelocityChange, Owner, ImpactType.FistGun);
                else
                    Owner.GetComponent<PlayerController>().OnImpact(-Owner.transform.forward * _fistGunData.IdleBackfireHitForce, ForceMode.VelocityChange, Owner, ImpactType.FistGun);
                EventManager.Instance.TriggerEvent(new FistGunFired(gameObject, Owner, Owner.GetComponent<PlayerController>().PlayerNumber, _fistDup));
                _fistGunState = FistGunState.Out;
            }
        }
    }

    protected override void _onWeaponDespawn()
    {
        _fistGunState = FistGunState.Idle;
        float rechargetimeleft = _chargeTimer + _fistGunData.ReloadTime - Time.time;
        rechargetimeleft = rechargetimeleft > 0f ? rechargetimeleft : 0f;
        if (_fistDup != null) Destroy(_fistDup, rechargetimeleft);
        _fistDup = null;
        _fist.gameObject.SetActive(true);
        _ammo = _fistGunData.MaxAmmo;
        EventManager.Instance.TriggerEvent(new ObjectDespawned(gameObject));
        gameObject.SetActive(false);
    }

    private void _switchToRecharge(bool maintainSpeed = false)
    {
        _fistGunState = FistGunState.Recharging;
        _fist.DOScale(0f, _fistGunData.ReloadTime).SetEase(_fistGunData.ReloadEase).From().OnPlay(() => _fist.gameObject.SetActive(true));
        EventManager.Instance.TriggerEvent(new FistGunStartCharging(gameObject, _fireOwner, _fireOwner.GetComponent<PlayerController>().PlayerNumber));
        _chargeTimer = Time.time;
        _fistDup.GetComponent<Rigidbody>().isKinematic = false;
        _fistDup.GetComponent<Rigidbody>().velocity = Vector3.zero;
        if (maintainSpeed)
            _fistDup.GetComponent<Rigidbody>().velocity = -_fistDup.transform.right * _fistGunData.FistSpeed;
        else
        {
            Vector3 rebound = _fistDup.transform.right;
            rebound.y = _fistGunData.FistReboundY;
            _fistDup.GetComponent<Rigidbody>().AddForce(_fistGunData.FistReboundForce * rebound, ForceMode.Impulse);
        }
        _onWeaponUsedOnce();
    }
}
