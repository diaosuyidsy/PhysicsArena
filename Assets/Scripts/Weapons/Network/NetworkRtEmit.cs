using System.Collections;
using System.Collections.Generic;
using Obi;
using UnityEngine;
using Mirror;

public class NetworkRtEmit : NetworkWeaponBase
{
    public WaterGunLine WaterGunLine;
    public GameObject WaterUI;
    public GameObject GunUI;
    public override float HelpAimAngle { get { return _waterGunData.HelpAimAngle; } }
    public override float HelpAimDistance { get { return _waterGunData.HelpAimDistance; } }
    private WaterGunData _waterGunData;
    private float _shootCD = 0f;
    [SyncVar(hook = nameof(OnChangeWaterSpeed))]
    private float _waterSpeed = 0f;
    private enum State
    {
        Empty,
        Shooting,
    }
    [SyncVar]
    private State _waterGunState;

    protected override void Awake()
    {
        base.Awake();
        _waterGunData = WeaponDataBase as WaterGunData;
        _waterGunState = State.Empty;
        _ammo = _waterGunData.MaxAmmo;
    }

    protected override void Update()
    {
        base.Update();
        switch (_waterGunState)
        {
            case State.Shooting:
                _shootCD += Time.deltaTime;
                if (_shootCD >= _waterGunData.ShootMaxCD)
                {
                    _shootCD = 0f;
                    _waterSpeed = 0f;
                    WaterGunLine.OnFire(false);
                    _waterGunState = State.Empty;
                    return;
                }
                if (_ownerIsLocalPlayer)
                {
                    _detectPlayer();
                }
                // Statistics: Here we are using raycast for players hit
                if (isServer)
                {
                    _onWeaponUsedOnce();
                }
                // If we changed ammo, then need to change UI as well
                ChangeAmmoUI();
                break;
            case State.Empty:
                break;
        }
    }

    public override void Fire(bool buttondown)
    {
        /// means we pressed down button here
        if (buttondown)
        {
            _waterGunState = State.Shooting;
            _waterSpeed = _waterGunData.Speed;
            RpcOnFire();
            WaterGunLine.OnFire(true);
            GunUI.SetActive(true);
            Owner.GetComponent<IHittableNetwork>().OnImpact(-Owner.transform.forward * _waterGunData.BackFireThrust, ForceMode.VelocityChange, Owner, ImpactType.WaterGun);
            EventManager.Instance.TriggerEvent(new WaterGunFired(gameObject, Owner, Owner.GetComponent<PlayerControllerMirror>().PlayerNumber));
        }
    }

    [ClientRpc]
    private void RpcOnFire()
    {
        // Backfire just to keep the speed 0
        WaterGunLine.OnFire(true);
        GunUI.SetActive(true);
        Owner.GetComponent<IHittableNetwork>().OnImpact(-Owner.transform.forward * _waterGunData.BackFireThrust, ForceMode.VelocityChange, Owner, ImpactType.WaterGun);
        EventManager.Instance.TriggerEvent(new WaterGunFired(gameObject, Owner, Owner.GetComponent<PlayerControllerMirror>().PlayerNumber));

    }

    private void _detectPlayer()
    {
        // This layermask means we are only looking for Player1Body - Player6Body
        LayerMask layermask = NetworkServices.Config.ConfigData.AllPlayerLayer ^ (1 << Owner.layer);
        RaycastHit hit;
        if (Physics.SphereCast(transform.position - Owner.transform.forward * _waterGunData.WaterBackCastDistance, _waterGunData.WaterCastRadius, Owner.transform.forward, out hit, _waterGunData.WaterCastDistance, layermask))
        {
            GameObject target = null;
            if (hit.transform.GetComponentInParent<NetworkWeaponBase>() != null)
            {
                target = hit.transform.GetComponentInParent<NetworkWeaponBase>().Owner;
            }
            else if (hit.transform.GetComponentInParent<IHittableNetwork>() != null)
            {
                target = hit.transform.gameObject;
            }
            if (target == null) return;
            if (!target.transform.GetComponentInParent<IHittableNetwork>().CanBlock(Owner.transform.forward))
            {
                GameObject receiver = target.transform.GetComponentInParent<PlayerControllerMirror>().gameObject;
                print("Hit");
                // TargetHit(receiver.GetComponent<NetworkIdentity>().connectionToClient, receiver, Owner, true);
                CmdHit(receiver, Owner, true);
            }
            else if (target.transform.GetComponentInParent<IHittableNetwork>() != null)
            {
                GameObject receiver = target.transform.GetComponentInParent<PlayerControllerMirror>().gameObject;
                // TargetHit(receiver.GetComponent<NetworkIdentity>().connectionToClient, receiver, Owner, false);
                CmdHit(receiver, Owner, false);
            }
        }
    }

    private void ChangeAmmoUI()
    {
        float scaleY = _ammo * 1.0f / _waterGunData.MaxAmmo;
        WaterUI.transform.localScale = new Vector3(1f, scaleY, 1f);
    }

    protected override void _onWeaponDespawn()
    {
        _shootCD = 0f;
        _waterSpeed = 0f;
        _ammo = _waterGunData.MaxAmmo;
        ChangeAmmoUI();
        EventManager.Instance.TriggerEvent(new ObjectDespawned(gameObject));
        gameObject.SetActive(false);
    }

    public override void OnDrop(bool customForce, Vector3 force)
    {
        base.OnDrop(customForce, force);
        WaterGunLine.OnFire(false);
        GunUI.SetActive(false);
        RpcOnDrop();
    }

    [ClientRpc]
    private void RpcOnDrop()
    {
        WaterGunLine.OnFire(false);
        GunUI.SetActive(false);
    }

    public void OnChangeWaterSpeed(float oldWaterSpeed, float newWaterSpeed)
    {
    }

    [Command]
    private void CmdHit(GameObject receiver, GameObject owner, bool withForce)
    {
        TargetHit(receiver.GetComponent<NetworkIdentity>().connectionToClient, receiver, owner, withForce);
    }

    [TargetRpc]
    private void TargetHit(NetworkConnection connection, GameObject receiver, GameObject owner, bool withForce)
    {
        if (withForce)
            receiver.GetComponent<IHittableNetwork>().OnImpact(_waterGunData.WaterForce * owner.transform.forward,
            ForceMode.VelocityChange,
            owner,
            ImpactType.WaterGun);
        else
            receiver.GetComponent<IHittableNetwork>().OnImpact(owner, ImpactType.WaterGun);
    }
}
