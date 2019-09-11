using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class rtBoomerang : WeaponBase
{
    private enum BoomerangState
    {
        In,
        Out,
    }
    private BoomerangState _boomerangState;
    private Tweener _pathMoveTweener;

    protected override void Awake()
    {
        base.Awake();
        _ammo = WeaponDataStore.BoomerangDataStore.MaxAmmo;
        _boomerangState = BoomerangState.In;
    }

    protected override void Update()
    {
        base.Update();
        if (_boomerangState == BoomerangState.Out)
        {
            RaycastHit hit;
            if (Physics.SphereCast(transform.position + transform.forward * 0.2f, 0.5f, transform.forward, out hit, 0.2f, WeaponDataStore.BoomerangDataStore.CanHitLayer))
            {
                print("hit1");

                if (hit.collider.GetComponent<WeaponBase>() != null) return;
                print("hit");
                PlayerController pc = hit.collider.GetComponentInParent<PlayerController>();
                if (pc != null)
                {
                    pc.OnImpact((pc.transform.position - transform.position).normalized * WeaponDataStore.BoomerangDataStore.OnHitForce, ForceMode.Impulse, Owner, ImpactType.Boomerang);
                }

            }
        }
    }

    public override void Fire(bool buttondown)
    {
        if (!buttondown)
        {
            _boomerangState = BoomerangState.Out;
            Vector3[] localPath = new Vector3[WeaponDataStore.BoomerangDataStore.LocalMovePoints.Length];
            for (int i = 0; i < WeaponDataStore.BoomerangDataStore.LocalMovePoints.Length; i++)
            {
                localPath[i] = transform.position + WeaponDataStore.BoomerangDataStore.LocalMovePoints[i];
            }
            _pathMoveTweener = transform.DOLocalPath(localPath, WeaponDataStore.BoomerangDataStore.BoomerangSpeed, PathType.CatmullRom)
            .SetSpeedBased(true)
            .SetEase(WeaponDataStore.BoomerangDataStore.BoomEase);
            _ammo--;
        }
    }

    protected override void _onWeaponDespawn()
    {
        _boomerangState = BoomerangState.In;
        _ammo = WeaponDataStore.BoomerangDataStore.MaxAmmo;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Vector3[] localMovePoints = WeaponDataStore.BoomerangDataStore.LocalMovePoints;
        for (int i = 0; i < localMovePoints.Length; i++)
        {
            Gizmos.DrawSphere(transform.position + localMovePoints[i], 0.2f);
        }
    }
}
