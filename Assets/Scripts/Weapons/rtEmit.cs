using System.Collections;
using System.Collections.Generic;
using Obi;
using UnityEngine;

public class rtEmit : WeaponBase
{
    public ObiEmitter WaterBall;
    public ObiParticleRenderer ParticleRenderer;
    public GameObject WaterUI;
    public GameObject GunUI;
    public override float HelpAimAngle { get { return _waterGunData.HelpAimAngle; } }
    public override float HelpAimDistance { get { return _waterGunData.HelpAimDistance; } }
    private WaterGunData _waterGunData;
    private float _shootCD = 0f;

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
                    WaterBall.speed = 0f;
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
            WaterBall.speed = _waterGunData.Speed;
            Owner.GetComponent<Rigidbody>().AddForce(-Owner.transform.forward * _waterGunData.BackFireThrust, ForceMode.Impulse);
            Owner.GetComponent<Rigidbody>().AddForce(Owner.transform.up * _waterGunData.UpThrust, ForceMode.Impulse);
            EventManager.Instance.TriggerEvent(new WaterGunFired(gameObject, Owner, Owner.GetComponent<PlayerController>().PlayerNumber));
        }
        else
        {
            _waterGunState = State.Empty;
            WaterBall.speed = 0f;
            _shootCD = 0f;
        }
    }

    private void _detectPlayer()
    {
        // This layermask means we are only looking for Player1Body - Player6Body
        LayerMask layermask = Services.Config.ConfigData.AllPlayerLayer;
        RaycastHit hit;
        if (Physics.Raycast(transform.position, -transform.right, out hit, Mathf.Infinity, layermask))
        {
            // if (hit.transform.GetComponentInParent<IHittable>() != null)
            //     hit.transform.GetComponentInParent<IHittable>().OnImpact(Owner, ImpactType.WaterGun);
            if (hit.transform.GetComponentInParent<IHittable>() != null &&
                !hit.transform.GetComponentInParent<IHittable>().CanBlock(Owner.transform.forward))
                hit.transform.GetComponentInParent<IHittable>().OnImpact(_waterGunData.WaterForce * Owner.transform.forward,
                ForceMode.Acceleration,
                Owner,
                ImpactType.WaterGun);
            else if (hit.transform.GetComponentInParent<IHittable>() != null)
            {
                hit.transform.GetComponentInParent<IHittable>().OnImpact(Owner, ImpactType.WaterGun);
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
        WaterBall.speed = 0f;
        _ammo = _waterGunData.MaxAmmo;
        ChangeAmmoUI();
        EventManager.Instance.TriggerEvent(new ObjectDespawned(gameObject));
        gameObject.SetActive(false);
    }

    public override void OnDrop()
    {
        base.OnDrop();
        Fire(false);
        GunUI.SetActive(false);
    }
}
