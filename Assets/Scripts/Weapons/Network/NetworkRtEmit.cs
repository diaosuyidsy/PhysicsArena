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
                    _waterSpeed = 0f;
                    _waterGunState = State.Empty;
                    return;
                }
                // Statistics: Here we are using raycast for players hit
                _detectPlayer();
                _onWeaponUsedOnce();
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
            GunUI.SetActive(true);
            _waterSpeed = _waterGunData.Speed;
            Owner.GetComponent<Rigidbody>().AddForce(-Owner.transform.forward * _waterGunData.BackFireThrust, ForceMode.Impulse);
            EventManager.Instance.TriggerEvent(new WaterGunFired(gameObject, Owner, Owner.GetComponent<PlayerControllerMirror>().PlayerNumber));
        }
        else
        {
            _waterGunState = State.Empty;
            _waterSpeed = 0f;
            _shootCD = 0f;
        }
    }

    private void _detectPlayer()
    {
        // This layermask means we are only looking for Player1Body - Player6Body
        LayerMask layermask = NetworkServices.Config.ConfigData.AllPlayerLayer;
        RaycastHit hit;
        if (Physics.Raycast(transform.position, -transform.right, out hit, Mathf.Infinity, layermask))
        {
            if (hit.transform.GetComponentInParent<IHittableNetwork>() != null &&
                !hit.transform.GetComponentInParent<IHittableNetwork>().CanBlock(Owner.transform.forward))
            {
                GameObject receiver = hit.transform.GetComponentInParent<PlayerControllerMirror>().gameObject;
                TargetHit(receiver.GetComponent<NetworkIdentity>().connectionToClient, receiver, Owner, true);
            }
            else if (hit.transform.GetComponentInParent<IHittableNetwork>() != null)
            {
                GameObject receiver = hit.transform.GetComponentInParent<PlayerControllerMirror>().gameObject;
                TargetHit(receiver.GetComponent<NetworkIdentity>().connectionToClient, receiver, Owner, true);
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
        Fire(false);
        GunUI.SetActive(false);
    }

    public void OnChangeWaterSpeed(float oldWaterSpeed, float newWaterSpeed)
    {
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
