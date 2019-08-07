using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rtFist : WeaponBase
{
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
        _fist = transform.Find("Fist");
        Debug.Assert(_fist != null);
        _fistScript = _fist.GetComponent<Fist>();
        _ammo = WeaponDataStore.FistGunDataStore.MaxAmmo;
    }

    // Update is called once per frame
    void Update()
    {
        if (_fistGunState == FistGunState.Out)
        {
            Vector3 nextPos = (_maxDistance - _fistDup.transform.position).normalized;
            _fistDup.transform.Translate(nextPos * Time.deltaTime * WeaponDataStore.FistGunDataStore.FistSpeed, Space.World);
            RaycastHit hit;
            if (Physics.SphereCast(_fistDup.transform.position, WeaponDataStore.FistGunDataStore.FistHitScanRadius, -_fistDup.transform.right, out hit, WeaponDataStore.FistGunDataStore.FistHitScanDist, WeaponDataStore.FistGunDataStore.AllThingFistCanCollideLayer ^ (1 << _fireOwner.layer)))
            {
                PlayerController pc = hit.collider.GetComponentInParent<PlayerController>();
                if (pc != null && !pc.CanBlock(-_fistDup.transform.right))
                {
                    pc.OnImpact(-_fistDup.transform.right * WeaponDataStore.FistGunDataStore.FistHitForce, ForceMode.Impulse, _fireOwner, ImpactType.FistGun);
                    EventManager.Instance.TriggerEvent(new FistGunHit(gameObject, _fistDup, _gpc.Owner, pc.gameObject, _gpc.Owner.GetComponent<PlayerController>().PlayerNumber, pc.PlayerNumber));
                }
                else if (pc != null)
                {
                    EventManager.Instance.TriggerEvent(new FistGunBlocked(gameObject, _gpc.Owner, _gpc.Owner.GetComponent<PlayerController>().PlayerNumber, _fistDup, pc.gameObject, pc.PlayerNumber));
                }
                else if (pc == null)
                {
                    EventManager.Instance.TriggerEvent(new FistGunHit(gameObject, _fistDup, _gpc.Owner, hit.collider.gameObject, _gpc.Owner.GetComponent<PlayerController>().PlayerNumber, -1));
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
            if (Time.time < _chargeTimer + WeaponDataStore.FistGunDataStore.ReloadTime)
            {
                // Recharding
            }
            else
            {
                // Charged
                EventManager.Instance.TriggerEvent(new FistGunCharged(gameObject, _gpc.Owner, _fistDup.transform.position));
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
                _fireOwner = _gpc.Owner;
                _maxDistance = transform.position + -transform.right * WeaponDataStore.FistGunDataStore.MaxFlyDistance;
                _fistDup = Instantiate(_fist.gameObject, transform);
                _fistDup.transform.position = _fist.position;
                _fistDup.transform.parent = null;
                _fistDup.GetComponent<Collider>().isTrigger = false;
                _fist.gameObject.SetActive(false);
                /// Add Backfire force to player as well
                _gpc.Owner.GetComponent<Rigidbody>().AddForce(transform.right * WeaponDataStore.FistGunDataStore.BackfireHitForce, ForceMode.VelocityChange);
                EventManager.Instance.TriggerEvent(new FistGunFired(gameObject, _gpc.Owner, _gpc.Owner.GetComponent<PlayerController>().PlayerNumber, _fistDup));
                _fistGunState = FistGunState.Out;
            }
        }
    }

    protected override void _onWeaponDespawn()
    {
        _fistGunState = FistGunState.Idle;
        float rechargetimeleft = _chargeTimer + WeaponDataStore.FistGunDataStore.ReloadTime - Time.time;
        rechargetimeleft = rechargetimeleft > 0f ? rechargetimeleft : 0f;
        if (_fistDup != null) Destroy(_fistDup, rechargetimeleft);
        _fistDup = null;
        _fist.gameObject.SetActive(true);
        _ammo = WeaponDataStore.FistGunDataStore.MaxAmmo;
        EventManager.Instance.TriggerEvent(new ObjectDespawned(gameObject));
        gameObject.SetActive(false);
    }

    private void _switchToRecharge(bool maintainSpeed = false)
    {
        _fistGunState = FistGunState.Recharging;
        EventManager.Instance.TriggerEvent(new FistGunStartCharging(gameObject, _fireOwner, _fireOwner.GetComponent<PlayerController>().PlayerNumber));
        _chargeTimer = Time.time;
        _fistDup.GetComponent<Rigidbody>().isKinematic = false;
        _fistDup.GetComponent<Rigidbody>().velocity = Vector3.zero;
        if (maintainSpeed)
            _fistDup.GetComponent<Rigidbody>().velocity = -_fistDup.transform.right * WeaponDataStore.FistGunDataStore.FistSpeed;
        else
        {
            Vector3 rebound = _fistDup.transform.right;
            rebound.y = WeaponDataStore.FistGunDataStore.FistReboundY;
            _fistDup.GetComponent<Rigidbody>().AddForce(WeaponDataStore.FistGunDataStore.FistReboundForce * rebound, ForceMode.Impulse);
        }
        _onWeaponUsedOnce();
    }
}
