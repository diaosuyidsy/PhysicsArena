using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Mirror;

public class NetworkRtFist : NetworkWeaponBase
{
    public override float HelpAimAngle { get { return _fistGunData.HelpAimAngle; } }
    public override float HelpAimDistance { get { return _fistGunData.HelpAimDistance; } }
    private FistGunData _fistGunData;
    private Transform _fist;
    private GameObject _fistDup;
    private Fist _fistScript;
    [SyncVar]
    private FistGunState _fistGunState = FistGunState.Idle;
    [SyncVar]
    private GameObject _fireOwner;
    private enum FistGunState
    {
        Idle,
        Out,
        Recharging,
    }
    [SyncVar]
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
            if (isServer)
            {
                RaycastHit hit;
                if (Physics.SphereCast(_fistDup.transform.position, _fistGunData.FistHitScanRadius, -_fistDup.transform.right, out hit, _fistGunData.FistHitScanDist, _fistGunData.AllThingFistCanCollideLayer ^ (1 << _fireOwner.layer)))
                {
                    if (hit.collider.GetComponent<NetworkWeaponBase>() != null) return;
                    IHittableNetwork IHittableNetwork = hit.collider.GetComponentInParent<IHittableNetwork>();
                    PlayerControllerMirror pc = hit.collider.GetComponentInParent<PlayerControllerMirror>();
                    if (IHittableNetwork != null && !IHittableNetwork.CanBlock(-_fistDup.transform.right))
                    {
                        // IHittableNetwork.OnImpact(-_fistDup.transform.right * _fistGunData.FistHitForce, ForceMode.Impulse, _fireOwner, ImpactType.FistGun);
                        TargetHit(pc.GetComponent<NetworkIdentity>().connectionToClient, pc.gameObject, _fireOwner);
                        RpcTriggerFistGunHit(gameObject, Owner, pc.gameObject);
                        EventManager.Instance.TriggerEvent(new FistGunHit(gameObject, _fistDup, Owner, ((MonoBehaviour)IHittableNetwork).gameObject, Owner.GetComponent<PlayerControllerMirror>().PlayerNumber, pc.PlayerNumber));
                    }
                    else if (pc != null)
                    {
                        _maxDistance = pc.transform.position + pc.transform.forward * _fistGunData.MaxFlyDistance;
                        _fireOwner = pc.gameObject;
                        _fistDup.transform.rotation = Quaternion.LookRotation(pc.transform.right, _fistDup.transform.up);
                        RpcTriggerFistGunBlocked(gameObject, Owner, hit.collider.gameObject);
                        EventManager.Instance.TriggerEvent(new FistGunBlocked(gameObject, Owner, Owner.GetComponent<PlayerControllerMirror>().PlayerNumber, _fistDup, pc.gameObject, pc.PlayerNumber));
                        return;
                    }
                    else if (pc == null)
                    {
                        RpcTriggerFistGunHit(gameObject, Owner, hit.collider.gameObject);
                        EventManager.Instance.TriggerEvent(new FistGunHit(gameObject, _fistDup, Owner, hit.collider.gameObject, Owner.GetComponent<PlayerControllerMirror>().PlayerNumber, -1));
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
                if (isServer)
                    RpcFistGunCharged(gameObject, Owner, transform.position);
                Destroy(_fistDup);
                _fistDup = null;
                // _fist.gameObject.SetActive(true);
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
                // NetworkServer.Spawn(_fistDup);
                _fist.gameObject.SetActive(false);
                /// Add Backfire force to player as well
                RpcOnFire(Owner);
                RpcTriggerFistGunFired(gameObject, Owner);
                EventManager.Instance.TriggerEvent(new FistGunFired(gameObject, Owner, Owner.GetComponent<PlayerControllerMirror>().PlayerNumber, _fistDup));
                _fistGunState = FistGunState.Out;
            }
        }
    }

    protected override void _onWeaponDespawn()
    {
        _fistGunState = FistGunState.Idle;
        float rechargetimeleft = _chargeTimer + _fistGunData.ReloadTime - Time.time;
        rechargetimeleft = rechargetimeleft > 0f ? rechargetimeleft : 0f;
        RpcDespawnFistGun(rechargetimeleft, gameObject);
        if (_fistDup != null) Destroy(_fistDup, rechargetimeleft);
        _fistDup = null;
        _fist.gameObject.SetActive(true);
        _ammo = _fistGunData.MaxAmmo;
        base._onWeaponDespawn();
    }

    // This is only called from server
    private void _switchToRecharge(bool maintainSpeed = false)
    {
        _fistGunState = FistGunState.Recharging;
        RpcSwitchToRecharge(maintainSpeed);
        // _fist.DOScale(0f, _fistGunData.ReloadTime).SetEase(_fistGunData.ReloadEase).From().OnPlay(() => _fist.gameObject.SetActive(true));
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

    #region Network Functions
    [ClientRpc]
    private void RpcSwitchToRecharge(bool maintainSpeed)
    {
        _chargeTimer = Time.time;
        _fist.DOScale(0f, _fistGunData.ReloadTime).SetEase(_fistGunData.ReloadEase).From().OnPlay(() => _fist.gameObject.SetActive(true));
        EventManager.Instance.TriggerEvent(new FistGunStartCharging(gameObject, _fireOwner, _fireOwner.GetComponent<PlayerControllerMirror>().PlayerNumber));
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
    }
    [TargetRpc]
    private void TargetHit(NetworkConnection connection, GameObject receiver, GameObject owner)
    {
        receiver.GetComponent<IHittableNetwork>().OnImpact(-_fistDup.transform.right * _fistGunData.FistHitForce, ForceMode.Impulse, owner, ImpactType.FistGun);
    }

    [ClientRpc]
    private void RpcTriggerFistGunHit(GameObject fistgun, GameObject fistgunUser, GameObject hitted)
    {
        EventManager.Instance.TriggerEvent(new FistGunHit(fistgun, _fistDup, fistgunUser, hitted, fistgunUser.GetComponent<PlayerControllerMirror>().PlayerNumber, hitted.GetComponent<PlayerControllerMirror>().PlayerNumber));
    }

    [ClientRpc]
    private void RpcTriggerFistGunBlocked(GameObject fistgun, GameObject fistgunUser, GameObject hitted)
    {
        _maxDistance = hitted.transform.position + hitted.transform.forward * _fistGunData.MaxFlyDistance;
        _fireOwner = hitted.gameObject;
        _fistDup.transform.rotation = Quaternion.LookRotation(hitted.transform.right, _fistDup.transform.up);
        // EventManager.Instance.TriggerEvent(new FistGunBlocked(fistgun, fistgunUser, fistgunUser.GetComponent<PlayerControllerMirror>().PlayerNumber, _fistDup, hitted, hitted.GetComponent<PlayerControllerMirror>().PlayerNumber));
    }

    [ClientRpc]
    private void RpcTriggerFistGunFired(GameObject fistgun, GameObject fistgunUser)
    {
        EventManager.Instance.TriggerEvent(new FistGunFired(fistgun, fistgunUser, fistgunUser.GetComponent<PlayerControllerMirror>().PlayerNumber, _fistDup));
    }

    [ClientRpc]
    private void RpcFistGunCharged(GameObject fistGun, GameObject owner, Vector3 position)
    {
        EventManager.Instance.TriggerEvent(new FistGunCharged(fistGun, owner, position));
        Destroy(_fistDup);
        _fistDup = null;
        _fist.gameObject.SetActive(true);
    }


    [ClientRpc]
    public void RpcOnFire(GameObject owner)
    {
        _fistDup = Instantiate(_fist.gameObject, transform);
        _fistDup.transform.position = _fist.position;
        _fistDup.transform.parent = null;
        _fistDup.GetComponent<Collider>().isTrigger = false;
        _fist.gameObject.SetActive(false);
        owner.GetComponent<PlayerControllerMirror>().OnImpact(-owner.transform.forward * _fistGunData.BackfireHitForce, ForceMode.VelocityChange, owner, ImpactType.FistGun);
    }

    [ClientRpc]
    private void RpcDespawnFistGun(float rechargeTimeLeft, GameObject gun)
    {
        if (_fistDup != null) Destroy(_fistDup, rechargeTimeLeft);
        _fistDup = null;
        _fist.gameObject.SetActive(true);
    }
    #endregion
}
