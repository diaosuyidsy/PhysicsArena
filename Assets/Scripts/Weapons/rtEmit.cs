using System.Collections;
using System.Collections.Generic;
using Obi;
using UnityEngine;

public class rtEmit : WeaponBase
{
    public WaterGunLine WaterGunLine;
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
    private HashSet<GameObject> _shootTargets;

    protected override void Awake()
    {
        base.Awake();
        _waterGunData = WeaponDataBase as WaterGunData;
        _waterGunState = State.Empty;
        _ammo = _waterGunData.MaxAmmo;
        _shootTargets = new HashSet<GameObject>();
    }

    protected override void Update()
    {
        base.Update();
        switch (_waterGunState)
        {
            case State.Shooting:
                // _shootCD += Time.deltaTime;
                // if (_shootCD >= _waterGunData.ShootMaxCD)
                // {
                //     _shootCD = 0f;
                //     _waterGunState = State.Empty;
                //     return;
                // }
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
        if (buttondown && _waterGunState == State.Empty)
        {
            _waterGunState = State.Shooting;
            _shootTargets.Clear();
            WaterGunLine.OnFire(true);
            GunUI.SetActive(true);
            Owner.GetComponent<IHittable>().OnImpact(-Owner.transform.forward * _waterGunData.BackFireThrust, ForceMode.VelocityChange, Owner, ImpactType.WaterGun);
            EventManager.Instance.TriggerEvent(new WaterGunFired(gameObject, Owner, Owner.GetComponent<PlayerController>().PlayerNumber));
        }
        else if (!buttondown && _waterGunState == State.Shooting)
        {
            _waterGunState = State.Empty;
            WaterGunLine.OnFire(false);
            EventManager.Instance.TriggerEvent(new WaterGunStopped(gameObject));
        }
    }

    private void _detectPlayer()
    {
        // This layermask means we are only looking for Player1Body - Player6Body
        LayerMask layermask = _waterGunData.WaterCanHitLayer ^ (1 << Owner.layer);
        RaycastHit hit;

        for (int i = 0; i < WaterGunLine.segmentCount; i++)
        {
            Vector3 _currentPosition = transform.position;
            float blockLength = (_waterGunData.WaterCastDistance + _waterGunData.WaterCastRadius) / WaterGunLine.segmentCount;
            for (int j = 0; j < i; j++)
            {
                _currentPosition += WaterGunLine.ResultVectors[j] * blockLength;
            }
            if (Physics.SphereCast(_currentPosition, _waterGunData.WaterCastRadius, WaterGunLine.ResultVectors[i], out hit, blockLength, layermask))
            {
                GameObject target = null;
                if (hit.transform.GetComponentInParent<WeaponBase>() != null)
                {
                    target = hit.transform.GetComponentInParent<WeaponBase>().Owner;
                }
                else if (hit.transform.GetComponentInParent<IHittable>() != null)
                {
                    target = hit.transform.GetComponentInParent<IHittable>().GetGameObject();
                }
                if (target == null) return;
                // if (_shootTargets.Contains(target)) return;
                // _shootTargets.Add(target);
                if (!target.GetComponent<IHittable>().CanBlock(Owner.transform.forward))
                {
                    float dist = Vector3.Distance(hit.transform.position, transform.position);
                    float distPer = dist / _waterGunData.WaterCastDistance;
                    float forceMultiplier = _waterGunData.WaterForceDecreaseGraph.Evaluate(distPer);
                    Vector3 force = _waterGunData.WaterForce * forceMultiplier * Owner.transform.forward;
                    target.GetComponent<IHittable>().OnImpact(force,
                            ForceMode.VelocityChange,
                            Owner,
                            ImpactType.WaterGun);
                }
                break;
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
        _ammo = _waterGunData.MaxAmmo;
        ChangeAmmoUI();
        base._onWeaponDespawn();
    }

    public override void OnDrop(bool customForce, Vector3 force)
    {
        base.OnDrop(customForce, force);
        _waterGunState = State.Empty;
        WaterGunLine.OnFire(false);
        EventManager.Instance.TriggerEvent(new WaterGunStopped(gameObject));
        GunUI.SetActive(false);
    }
}
